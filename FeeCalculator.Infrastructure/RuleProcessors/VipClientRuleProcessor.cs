using FeeCalculator.Core.Models;
using System.Text.Json;

namespace FeeCalculator.Infrastructure.RuleProcessors
{
    // Example future rule - VIP Client Discount
    public class VipClientRuleProcessor : BaseRuleProcessor
    {
        public override int RuleId => 4;
        public override string RuleName => "VIP Client Discount";
        public override string Description => "5% discount for VIP clients";
        public override string RuleType => RuleTypes.DISCOUNT;
        public override int Priority => 60;

        public VipClientRuleProcessor()
        {
            IsActive = false; // Disabled by default
            
            ParametersJson = JsonSerializer.Serialize(new
            {
                DiscountPercentage = 0.05m
            });

            ConditionsJson = JsonSerializer.Serialize(new
            {
                ClientSegment = ClientSegments.VIP
            });
        }

        public override bool IsApplicable(TransactionRequest request)
        {
            return IsActive && 
                   request.Client.ClientSegment.Equals(ClientSegments.VIP, StringComparison.OrdinalIgnoreCase);
        }

        public override decimal CalculateFee(TransactionRequest request, decimal currentFee)
        {
            var parameters = GetRuleParameters();
            var discountPercentage = Convert.ToDecimal(parameters.GetValueOrDefault("DiscountPercentage", 0.05m));
            
            // Apply 1% discount to the calculated fee (not transaction amount)
            var discount = currentFee * discountPercentage;
            return currentFee - discount;
        }
    }
}