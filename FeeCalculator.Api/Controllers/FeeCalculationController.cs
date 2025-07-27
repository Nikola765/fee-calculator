using Microsoft.AspNetCore.Mvc;
using FeeCalculator.Core.Interfaces;
using FeeCalculator.Core.Models;

namespace FeeCalculator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FeeCalculatorController : ControllerBase
    {
        private readonly IFeeCalculationService _feeCalculationService;
        private readonly ILogger<FeeCalculatorController> _logger;

        public FeeCalculatorController(
            IFeeCalculationService feeCalculationService, 
            ILogger<FeeCalculatorController> logger)
        {
            _feeCalculationService = feeCalculationService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate fee for a single transaction
        /// </summary>
        /// <param name="request">Transaction details</param>
        /// <returns>Calculated fee with applied rules</returns>
        [HttpPost("calculate")]
        [ProducesResponseType(typeof(FeeResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FeeResult>> CalculateFee([FromBody] TransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Calculating fee for transaction {TransactionId} of type {TransactionType}", 
                    request.TransactionId, request.TransactionType);
                
                var result = await _feeCalculationService.CalculateFeeAsync(request);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Fee calculation failed for transaction {TransactionId}: {Error}", 
                        request.TransactionId, result.ErrorMessage);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calculating fee for transaction {TransactionId}", request.TransactionId);
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Calculate fees for multiple transactions in batch
        /// </summary>
        /// <param name="batchRequest">Batch of transactions</param>
        /// <returns>Batch calculation results</returns>
        [HttpPost("calculate-batch")]
        [ProducesResponseType(typeof(BatchFeeResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BatchFeeResult>> CalculateBatchFees([FromBody] BatchTransactionRequest batchRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!batchRequest.Transactions.Any())
                {
                    return BadRequest("Batch must contain at least one transaction");
                }

                if (batchRequest.Transactions.Count > 10000)
                {
                    return BadRequest("Batch size cannot exceed 10,000 transactions");
                }

                _logger.LogInformation("Processing batch {BatchId} with {TransactionCount} transactions", 
                    batchRequest.BatchId, batchRequest.Transactions.Count);

                var result = await _feeCalculationService.CalculateBatchFeesAsync(batchRequest);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch {BatchId}", batchRequest.BatchId);
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Get calculation history with pagination
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take (max 1000)</param>
        /// <returns>List of calculation history entries</returns>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<FeeCalculationHistory>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<FeeCalculationHistory>>> GetCalculationHistory(
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 100)
        {
            try
            {
                if (take > 1000)
                {
                    take = 1000;
                }

                if (skip < 0 || take <= 0)
                {
                    return BadRequest("Skip must be non-negative and take must be positive");
                }

                var history = await _feeCalculationService.GetCalculationHistoryAsync(skip, take);
                
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving calculation history");
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }
}