using FeeCalculator.Core.Models;

namespace FeeCalculator.Core.Interfaces
{
    public interface IFeeRuleProcessor
    {
        int RuleId { get; }
        string RuleName { get; }
        string Description { get; }
        string RuleType { get; }
        int Priority { get; }
        bool IsActive { get; set; }
        
        bool IsApplicable(TransactionRequest request);
        decimal CalculateFee(TransactionRequest request, decimal currentFee);
        Dictionary<string, object> GetRuleParameters();
    }
}