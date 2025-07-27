using FeeCalculator.Core.Models;

namespace FeeCalculator.Core.Interfaces
{
    public interface IFeeCalculationService
    {
        Task<FeeResult> CalculateFeeAsync(TransactionRequest request);
        Task<BatchFeeResult> CalculateBatchFeesAsync(BatchTransactionRequest batchRequest);
        Task<List<FeeCalculationHistory>> GetCalculationHistoryAsync(int skip = 0, int take = 100);
    }
}