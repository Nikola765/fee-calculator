using System.ComponentModel.DataAnnotations;

namespace FeeCalculator.Core.Models
{
    public class TransactionRequest
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;
        
        [Required]
        public string TransactionType { get; set; } = string.Empty; // POS, E_COMMERCE, ATM, TRANSFER
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public string Currency { get; set; } = "EUR";
        
        public bool IsDomestic { get; set; } = true;
        public string MerchantCategory { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } = false;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public ClientInfo Client { get; set; } = new();
        
        public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
    }

    public class ClientInfo
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;
        
        public int? CreditScore { get; set; }
        public string ClientSegment { get; set; } = "STANDARD"; // STANDARD, PREMIUM, VIP
        public bool HasActivePromotions { get; set; } = false;
        public DateTime ClientSince { get; set; } = DateTime.UtcNow;
        public decimal MonthlyVolume { get; set; }
        public int TransactionCountThisMonth { get; set; }
        public string BusinessType { get; set; } = "INDIVIDUAL"; // INDIVIDUAL, BUSINESS, CORPORATE
        public string RiskLevel { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH
        public List<string> ActivePromotions { get; set; } = new();
        
        public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
    }

    public class BatchTransactionRequest
    {
        [Required]
        public List<TransactionRequest> Transactions { get; set; } = new();
        
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}