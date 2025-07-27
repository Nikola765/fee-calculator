namespace FeeCalculator.Core.Models
{
    public class FeeResult
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public string Currency { get; set; } = "EUR";
        public List<AppliedRule> AppliedRules { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    public class BatchFeeResult
    {
        public string BatchId { get; set; } = string.Empty;
        public List<FeeResult> Results { get; set; } = new();
        public int TotalTransactions { get; set; }
        public int SuccessfulCalculations { get; set; }
        public int FailedCalculations { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class AppliedRule
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public decimal FeeAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty; // BASE, DISCOUNT, SURCHARGE, CAP
        public Dictionary<string, object> RuleParameters { get; set; } = new();
    }

    public class FeeCalculationHistory
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string RequestJson { get; set; } = string.Empty;
        public string ResultJson { get; set; } = string.Empty;
        public DateTime CalculatedAt { get; set; }
    }
}