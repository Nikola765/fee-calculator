using FeeCalculator.Core.Models;
using System.Text.Json;

namespace FeeCalculator.Infrastructure.RuleProcessors
{
    // Rule #2: 1.8% of the amount + €0.15, but no more than €120, for e-commerce transactions
    public class ECommerceRuleProcessor : BaseRuleProcessor
    {
        public override int RuleId => 2;
        public override string RuleName => "E-Commerce Transaction Fee";
        public override string Description => "1.8% of the amount + €0.15, but no more than €120, for e-commerce transactions";
        public override string RuleType => RuleTypes.BASE;
        public override int Priority => 10;

        public ECommerceRuleProcessor()
        {
            ParametersJson = JsonSerializer.Serialize(new
            {
                PercentageFee = 0.018m,
                FixedFeeAmount = 0.15m,
                MaxFeeAmount = 120.00m
            });

            ConditionsJson = JsonSerializer.Serialize(new
            {
                TransactionType = TransactionTypes.E_COMMERCE
            });
        }

        public override bool IsApplicable(TransactionRequest request)
        {
            return IsActive && request.TransactionType.Equals(TransactionTypes.E_COMMERCE, StringComparison.OrdinalIgnoreCase);
        }

        public override decimal CalculateFee(TransactionRequest request, decimal currentFee)
        {
            var parameters = GetRuleParameters();
            var percentageFee = GetDecimalParameter(parameters, "PercentageFee", 0.018m);
            var fixedFee = GetDecimalParameter(parameters, "FixedFeeAmount", 0.15m);
            var maxFee = GetDecimalParameter(parameters, "MaxFeeAmount", 120.00m);

            var calculatedFee = (request.Amount * percentageFee) + fixedFee;
            return Math.Min(calculatedFee, maxFee);
        }
    }
}