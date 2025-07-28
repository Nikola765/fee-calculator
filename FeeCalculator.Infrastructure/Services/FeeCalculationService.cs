using FeeCalculator.Core.Interfaces;
using FeeCalculator.Core.Models;
using FeeCalculator.Infrastructure.RuleProcessors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;

namespace FeeCalculator.Infrastructure.Services
{
    public class FeeCalculationService : IFeeCalculationService
    {
        private readonly ILogger<FeeCalculationService> _logger;
        private readonly IMemoryCache _cache;
        private readonly List<IFeeRuleProcessor> _ruleProcessors;
        private readonly List<FeeCalculationHistory> _calculationHistory;
        
        private const string PROCESSORS_CACHE_KEY = "active_processors";
        private const int CACHE_EXPIRATION_MINUTES = 30;

        public FeeCalculationService(ILogger<FeeCalculationService> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
            _ruleProcessors = new List<IFeeRuleProcessor>();
            _calculationHistory = new List<FeeCalculationHistory>();
            
            InitializeRuleProcessors();
        }

        private void InitializeRuleProcessors()
        {
            var processors = new List<IFeeRuleProcessor>
            {
                new PosTransactionRuleProcessor(),
                new ECommerceRuleProcessor(),
                new CreditScoreDiscountProcessor(),
                new VipClientRuleProcessor()
            };

            _ruleProcessors.AddRange(processors);
            _logger.LogInformation("Initialized {ProcessorCount} rule processors", processors.Count);
        }

        public async Task<FeeResult> CalculateFeeAsync(TransactionRequest request)
        {
            var result = new FeeResult
            {
                TransactionId = request.TransactionId,
                Currency = request.Currency
            };

            try
            {
                var activeProcessors = await GetActiveProcessorsAsync();

                var applicableProcessors = activeProcessors
                    .Where(processor => processor.IsApplicable(request))
                    .OrderBy(processor => processor.Priority)
                    .ToList();

                if (!applicableProcessors.Any())
                {
                    _logger.LogWarning("No applicable rules found for transaction {TransactionId} of type {TransactionType}", 
                        request.TransactionId, request.TransactionType);
                    result.IsSuccess = false;
                    result.ErrorMessage = "No applicable fee rules found";
                    return result;
                }

                decimal currentFee = 0;
                var appliedRules = new List<AppliedRule>();

                // Base rules
                var baseRules = applicableProcessors.Where(p => p.RuleType == RuleTypes.BASE).ToList();
                
                foreach (var processor in baseRules)
                {
                    try
                    {
                        var previousFee = currentFee;
                        currentFee = processor.CalculateFee(request, currentFee);
                        
                        appliedRules.Add(new AppliedRule
                        {
                            RuleId = processor.RuleId,
                            RuleName = processor.RuleName,
                            Description = processor.Description,
                            FeeAmount = currentFee - previousFee,
                            RuleType = processor.RuleType,
                            RuleParameters = processor.GetRuleParameters()
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error applying base rule {RuleName} for transaction {TransactionId}", 
                            processor.RuleName, request.TransactionId);
                        result.IsSuccess = false;
                        result.ErrorMessage = $"Error applying rule {processor.RuleName}: {ex.Message}";
                        return result;
                    }
                }

                // Discount rules
                var discountRules = applicableProcessors.Where(p => p.RuleType == RuleTypes.DISCOUNT).ToList();
                _logger.LogInformation("Applying {DiscountRuleCount} discount rules", discountRules.Count);
                
                foreach (var processor in discountRules)
                {
                    try
                    {
                        var previousFee = currentFee;
                        currentFee = processor.CalculateFee(request, currentFee);
                        
                        _logger.LogInformation("Applied discount rule {RuleName}: {PreviousFee} -> {CurrentFee}", 
                            processor.RuleName, previousFee, currentFee);
                        
                        appliedRules.Add(new AppliedRule
                        {
                            RuleId = processor.RuleId,
                            RuleName = processor.RuleName,
                            Description = processor.Description,
                            FeeAmount = currentFee - previousFee, // This will be negative for discounts
                            RuleType = processor.RuleType,
                            RuleParameters = processor.GetRuleParameters()
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error applying discount rule {RuleName} for transaction {TransactionId}", 
                            processor.RuleName, request.TransactionId);
                        result.IsSuccess = false;
                        result.ErrorMessage = $"Error applying rule {processor.RuleName}: {ex.Message}";
                        return result;
                    }
                }

                // Other rule types
                var otherRules = applicableProcessors
                    .Where(p => p.RuleType != RuleTypes.BASE && p.RuleType != RuleTypes.DISCOUNT)
                    .ToList();
                
                _logger.LogInformation("Applying {OtherRuleCount} other rules", otherRules.Count);
                
                foreach (var processor in otherRules)
                {
                    try
                    {
                        var previousFee = currentFee;
                        currentFee = processor.CalculateFee(request, currentFee);
                        
                        _logger.LogInformation("Applied other rule {RuleName}: {PreviousFee} -> {CurrentFee}", 
                            processor.RuleName, previousFee, currentFee);
                        
                        appliedRules.Add(new AppliedRule
                        {
                            RuleId = processor.RuleId,
                            RuleName = processor.RuleName,
                            Description = processor.Description,
                            FeeAmount = currentFee - previousFee,
                            RuleType = processor.RuleType,
                            RuleParameters = processor.GetRuleParameters()
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error applying other rule {RuleName} for transaction {TransactionId}", 
                            processor.RuleName, request.TransactionId);
                        result.IsSuccess = false;
                        result.ErrorMessage = $"Error applying rule {processor.RuleName}: {ex.Message}";
                        return result;
                    }
                }

                result.Fee = Math.Max(0, currentFee); // Ensure fee is never negative
                result.AppliedRules = appliedRules;

                // Store in history
                await StoreCalculationHistoryAsync(request, result);

                _logger.LogInformation("Successfully calculated fee {Fee} for transaction {TransactionId} using {RuleCount} rules", 
                    result.Fee, request.TransactionId, appliedRules.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calculating fee for transaction {TransactionId}: {ErrorMessage}", 
                    request.TransactionId, ex.Message);
                result.IsSuccess = false;
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
                return result;
            }
        }

        public async Task<BatchFeeResult> CalculateBatchFeesAsync(BatchTransactionRequest batchRequest)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var result = new BatchFeeResult
            {
                BatchId = batchRequest.BatchId,
                TotalTransactions = batchRequest.Transactions.Count
            };

            try
            {
                // Process transactions in parallel for better performance with large batches
                var tasks = batchRequest.Transactions.Select(async transaction =>
                {
                    return await CalculateFeeAsync(transaction);
                });

                var results = await Task.WhenAll(tasks);
                
                result.Results = results.ToList();
                result.SuccessfulCalculations = results.Count(r => r.IsSuccess);
                result.FailedCalculations = results.Count(r => !r.IsSuccess);
                
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Processed batch {BatchId} with {TotalTransactions} transactions in {ProcessingTime}ms. Success: {SuccessCount}, Failed: {FailedCount}",
                    batchRequest.BatchId, 
                    result.TotalTransactions, 
                    result.ProcessingTime.TotalMilliseconds,
                    result.SuccessfulCalculations,
                    result.FailedCalculations);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch {BatchId}", batchRequest.BatchId);
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                return result;
            }
        }

        // Method to get calculation history
        public async Task<List<FeeCalculationHistory>> GetCalculationHistoryAsync(int skip = 0, int take = 100)
        {
            return await Task.FromResult(
                _calculationHistory
                    .OrderByDescending(h => h.CalculatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToList()
            );
        }

        private async Task<List<IFeeRuleProcessor>> GetActiveProcessorsAsync()
        {
            return await Task.FromResult(
                _cache.GetOrCreate(PROCESSORS_CACHE_KEY, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES);
                    return _ruleProcessors.Where(p => p.IsActive).ToList();
                }) ?? new List<IFeeRuleProcessor>()
            );
        }

        private async Task StoreCalculationHistoryAsync(TransactionRequest request, FeeResult result)
        {
            try
            {
                var historyEntry = new FeeCalculationHistory
                {
                    Id = _calculationHistory.Count + 1,
                    TransactionId = request.TransactionId,
                    RequestJson = JsonSerializer.Serialize(request),
                    ResultJson = JsonSerializer.Serialize(result),
                    CalculatedAt = DateTime.UtcNow
                };

                _calculationHistory.Add(historyEntry);
                
                _logger.LogDebug("Stored calculation history for transaction {TransactionId}. Total history count: {Count}", 
                    request.TransactionId, _calculationHistory.Count);
                
                // Keep only last 10000 entries to prevent memory issues
                if (_calculationHistory.Count > 10000)
                {
                    _calculationHistory.RemoveRange(0, 1000);
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing calculation history for transaction {TransactionId}", request.TransactionId);
            }
        }

        // Method to get processors for management (used by controllers)
        public Task<List<IFeeRuleProcessor>> GetAllProcessorsAsync()
        {
            return Task.FromResult(_ruleProcessors.ToList());
        }

        // Method to toggle processor status
        public Task<bool> ToggleProcessorStatusAsync(int ruleId, bool isActive)
        {
            var processor = _ruleProcessors.FirstOrDefault(p => p.RuleId == ruleId);
            if (processor != null)
            {
                processor.IsActive = isActive;
                _cache.Remove(PROCESSORS_CACHE_KEY); // Invalidate cache
                
                _logger.LogInformation("{Action} rule processor {RuleId}: {RuleName}", 
                    isActive ? "Activated" : "Deactivated", ruleId, processor.RuleName);
                
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
    }
}
