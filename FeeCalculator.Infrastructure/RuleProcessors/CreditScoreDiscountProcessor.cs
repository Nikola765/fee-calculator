using FeeCalculator.Core.Models;
using System.Text.Json;

namespace FeeCalculator.Infrastructure.RuleProcessors
{
    // Rule #3: 1% discount on all transactions for clients with creditScore>400
    public class CreditScoreDiscountProcessor : BaseRuleProcessor
    {
        public override int RuleId => 3;
        public override string RuleName => "High Credit Score Discount";
        public override string Description => "1% discount on all transactions for clients with creditScore > 400";
        public override string RuleType => RuleTypes.DISCOUNT;
        public override int Priority => 50; // Applied after base fee calculation

        public CreditScoreDiscountProcessor()
        {
            ParametersJson = JsonSerializer.Serialize(new
            {
                MinCreditScore = 400,
                DiscountPercentage = 0.01m
            });
        }

        public override bool IsApplicable(TransactionRequest request)
        {
            var parameters = GetRuleParameters();
            var minCreditScore = GetIntParameter(parameters, "MinCreditScore", 400);
            
            return IsActive && 
                   request.Client.CreditScore.HasValue && 
                   request.Client.CreditScore.Value > minCreditScore;
        }

        public override decimal CalculateFee(TransactionRequest request, decimal currentFee)
        {
            var parameters = GetRuleParameters();
            var discountPercentage = GetDecimalParameter(parameters, "DiscountPercentage", 0.01m);
            
            // Apply 1% discount to the calculated fee (not transaction amount)
            var discount = currentFee * discountPercentage;
            return currentFee - discount;
        }
    }
}