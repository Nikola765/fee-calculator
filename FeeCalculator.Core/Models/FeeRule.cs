using System.ComponentModel.DataAnnotations;

namespace FeeCalculator.Core.Models
{
    public class FeeRule
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]  
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        [Range(1, 1000)]
        public int Priority { get; set; } = 100; // Lower number = higher priority
        
        [Required]
        public string RuleType { get; set; } = "BASE"; // BASE, DISCOUNT, SURCHARGE, CAP
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Rule conditions and parameters as JSON for flexibility
        public string ConditionsJson { get; set; } = "{}";
        public string ParametersJson { get; set; } = "{}";
        
        // Rule processor class name for dynamic loading
        public string ProcessorClassName { get; set; } = string.Empty;
    }

    public class RuleTypes
    {
        public const string BASE = "BASE";
        public const string DISCOUNT = "DISCOUNT";  
        public const string SURCHARGE = "SURCHARGE";
        public const string CAP = "CAP";
    }

    public class ClientSegments
    {
        public const string STANDARD = "STANDARD";
        public const string PREMIUM = "PREMIUM";
        public const string VIP = "VIP";
    }

    public class BusinessTypes
    {
        public const string INDIVIDUAL = "INDIVIDUAL";
        public const string BUSINESS = "BUSINESS";
        public const string CORPORATE = "CORPORATE";
    }

    public class RiskLevels
    {
        public const string LOW = "LOW";
        public const string MEDIUM = "MEDIUM";
        public const string HIGH = "HIGH";
    }

    public class TransactionTypes
    {
        public const string POS = "POS";
        public const string E_COMMERCE = "E_COMMERCE";
        public const string ATM = "ATM";
        public const string TRANSFER = "TRANSFER";
        public const string INTERNATIONAL = "INTERNATIONAL";
    }
}