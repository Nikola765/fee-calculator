using FeeCalculator.Core.Interfaces;
using FeeCalculator.Core.Models;
using System.Text.Json;

namespace FeeCalculator.Infrastructure.RuleProcessors
{
    public abstract class BaseRuleProcessor : IFeeRuleProcessor
    {
        public abstract int RuleId { get; }
        public abstract string RuleName { get; }
        public abstract string Description { get; }
        public abstract string RuleType { get; }
        public abstract int Priority { get; }
        public virtual bool IsActive { get; set; } = true;

        protected string ParametersJson { get; set; } = "{}";
        protected string ConditionsJson { get; set; } = "{}";

        public abstract bool IsApplicable(TransactionRequest request);
        public abstract decimal CalculateFee(TransactionRequest request, decimal currentFee);

        public virtual Dictionary<string, object> GetRuleParameters()
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(ParametersJson);
                var result = new Dictionary<string, object>();
                
                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    result[property.Name] = ConvertJsonElement(property.Value);
                }
                
                return result;
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        protected Dictionary<string, object> GetConditions()
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(ConditionsJson);
                var result = new Dictionary<string, object>();
                
                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    result[property.Name] = ConvertJsonElement(property.Value);
                }
                
                return result;
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private static object ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.TryGetDecimal(out var decimalValue) ? decimalValue : 
                                       element.TryGetInt32(out var intValue) ? intValue : 
                                       element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                _ => element.ToString()
            };
        }

        protected static decimal GetDecimalParameter(Dictionary<string, object> parameters, string key, decimal defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value switch
            {
                decimal d => d,
                double db => (decimal)db,
                float f => (decimal)f,
                int i => (decimal)i,
                long l => (decimal)l,
                string s when decimal.TryParse(s, out var parsed) => parsed,
                _ => defaultValue
            };
        }

        protected static int GetIntParameter(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value switch
            {
                int i => i,
                long l => (int)l,
                decimal d => (int)d,
                double db => (int)db,
                float f => (int)f,
                string s when int.TryParse(s, out var parsed) => parsed,
                _ => defaultValue
            };
        }
    }
}