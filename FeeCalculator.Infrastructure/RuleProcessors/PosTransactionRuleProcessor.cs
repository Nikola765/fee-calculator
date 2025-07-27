using FeeCalculator.Core.Models;
using System.Text.Json;

namespace FeeCalculator.Infrastructure.RuleProcessors
{
    // Rule #1: Fixed fee of €0.20 for POS transactions up to €100. For all POS transactions >€100, 0.2% of the amount
    public class PosTransactionRuleProcessor : BaseRuleProcessor
    {
        public override int RuleId => 1;
        public override string RuleName => "POS Transaction Fee";
        public override string Description => "Fixed fee of €0.20 for POS transactions up to €100. For all POS transactions >€100, 0.2% of the amount";
        public override string RuleType => RuleTypes.BASE;
        public override int Priority => 10;

        public PosTransactionRuleProcessor()
        {
            ParametersJson = JsonSerializer.Serialize(new
            {
                FixedFeeAmount = 0.20m,
                ThresholdAmount = 100.00m,
                PercentageFee = 0.002m
            });

            ConditionsJson = JsonSerializer.Serialize(new
            {
                TransactionType = TransactionTypes.POS
            });
        }

        public override bool IsApplicable(TransactionRequest request)
        {
            return IsActive && request.TransactionType.Equals(TransactionTypes.POS, StringComparison.OrdinalIgnoreCase);
        }

        public override decimal CalculateFee(TransactionRequest request, decimal currentFee)
        {
            var parameters = GetRuleParameters();
            var fixedFee = GetDecimalParameter(parameters, "FixedFeeAmount", 0.20m);
            var threshold = GetDecimalParameter(parameters, "ThresholdAmount", 100.00m);
            var percentageFee = GetDecimalParameter(parameters, "PercentageFee", 0.002m);

            if (request.Amount <= threshold)
            {
                return fixedFee;
            }
            else
            {
                return request.Amount * percentageFee;
            }
        }
    }
}