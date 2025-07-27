using FeeCalculator.Core.Models;

namespace FeeCalculator.Core.Interfaces
{
    public interface IRuleConfigurationService
    {
        Task<List<FeeRule>> GetActiveRulesAsync();
        Task<FeeRule?> GetRuleByIdAsync(int ruleId);
        Task<int> AddRuleAsync(FeeRule rule);
        Task<bool> UpdateRuleAsync(FeeRule rule);
        Task<bool> DeactivateRuleAsync(int ruleId);
        Task<bool> ActivateRuleAsync(int ruleId);
    }
}