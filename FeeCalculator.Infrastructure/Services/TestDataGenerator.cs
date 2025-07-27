using FeeCalculator.Core.Models;

namespace FeeCalculator.Infrastructure.Services
{
    public static class TestDataGenerator
    {
        private static readonly Random Random = new();
        
        public static List<TransactionRequest> GenerateTestTransactions(int count = 1000)
        {
            var transactions = new List<TransactionRequest>();
            var transactionTypes = new[] { TransactionTypes.POS, TransactionTypes.E_COMMERCE, TransactionTypes.ATM, TransactionTypes.TRANSFER };
            
            for (int i = 1; i <= count; i++)
            {
                var transactionType = transactionTypes[Random.Next(transactionTypes.Length)];
                var amount = GenerateRealisticAmount(transactionType);
                
                var transaction = new TransactionRequest
                {
                    TransactionId = $"TXN_{i:D6}",
                    TransactionType = transactionType,
                    Amount = amount,
                    Currency = Random.Next(10) < 9 ? "EUR" : GetRandomCurrency(),
                    IsDomestic = Random.Next(10) < 8,
                    MerchantCategory = GetRandomMerchantCategory(transactionType),
                    Channel = GetRandomChannel(),
                    IsRecurring = Random.Next(10) < 2,
                    TransactionDate = DateTime.UtcNow.AddDays(-Random.Next(0, 30)),
                    Client = GenerateRandomClient($"CLIENT_{Random.Next(1, 501):D3}")
                };
                
                transactions.Add(transaction);
            }
            
            return transactions;
        }
        
        private static decimal GenerateRealisticAmount(string transactionType)
        {
            var baseAmount = transactionType switch
            {
                TransactionTypes.POS => Random.Next(5, 500),
                TransactionTypes.E_COMMERCE => Random.Next(10, 2000),
                TransactionTypes.ATM => Random.Next(20, 500),
                TransactionTypes.TRANSFER => Random.Next(50, 10000),
                TransactionTypes.INTERNATIONAL => Random.Next(100, 50000),
                _ => Random.Next(10, 1000)
            };

            // Add cents (0.00 to 0.99) and round to 2 decimal places
            var cents = (decimal)(Random.NextDouble() * 0.99);
            return Math.Round(baseAmount + cents, 2);
        }
        
        private static string GetRandomCurrency()
        {
            var currencies = new[] { "USD", "GBP", "JPY", "CAD", "AUD", "CHF" };
            return currencies[Random.Next(currencies.Length)];
        }
        
        private static string GetRandomMerchantCategory(string transactionType)
        {
            return transactionType switch
            {
                TransactionTypes.POS => GetRandomValue(new[] { "GROCERY", "RESTAURANT", "RETAIL", "GAS_STATION", "PHARMACY" }),
                TransactionTypes.E_COMMERCE => GetRandomValue(new[] { "ONLINE_RETAIL", "DIGITAL_SERVICES", "SUBSCRIPTION", "MARKETPLACE" }),
                TransactionTypes.ATM => "ATM_WITHDRAWAL",
                _ => "OTHER"
            };
        }
        
        private static string GetRandomChannel()
        {
            var channels = new[] { "MOBILE", "WEB", "POS", "ATM", "BRANCH", "PHONE" };
            return channels[Random.Next(channels.Length)];
        }
        
        private static string GetRandomValue(string[] values)
        {
            return values[Random.Next(values.Length)];
        }
        
        private static ClientInfo GenerateRandomClient(string clientId)
        {
            var segments = new[] { ClientSegments.STANDARD, ClientSegments.PREMIUM, ClientSegments.VIP };
            var businessTypes = new[] { BusinessTypes.INDIVIDUAL, BusinessTypes.BUSINESS, BusinessTypes.CORPORATE };
            var riskLevels = new[] { RiskLevels.LOW, RiskLevels.MEDIUM, RiskLevels.HIGH };
            
            return new ClientInfo
            {
                ClientId = clientId,
                CreditScore = Random.Next(200, 850),
                ClientSegment = segments[Random.Next(segments.Length)],
                BusinessType = businessTypes[Random.Next(businessTypes.Length)],
                RiskLevel = riskLevels[Random.Next(riskLevels.Length)],
                HasActivePromotions = Random.Next(10) < 3,
                ClientSince = DateTime.UtcNow.AddDays(-Random.Next(30, 1825)),
                MonthlyVolume = Math.Round((decimal)Random.Next(100, 50000), 2),
                TransactionCountThisMonth = Random.Next(1, 50),
                ActivePromotions = GenerateRandomPromotions()
            };
        }
        
        private static List<string> GenerateRandomPromotions()
        {
            var allPromotions = new[]
            {
                "SUMMER2025", "NEWCLIENT", "LOYALTY", "CASHBACK", "PREMIUM", "VOLUME_DISCOUNT"
            };
            
            var promotionCount = Random.Next(0, 3);
            return allPromotions
                .OrderBy(x => Random.Next())
                .Take(promotionCount)
                .ToList();
        }
        
        // Specific test scenarios to cover the required rules from the task
        public static List<TransactionRequest> GenerateScenarioTestData()
        {
            var scenarios = new List<TransactionRequest>();
            
            // Scenario 1: POS transactions to test Rule #1
            scenarios.AddRange(new[]
            {
                // POS under €100 - should get €0.20 fee
                new TransactionRequest
                {
                    TransactionId = "POS_UNDER_100_01",
                    TransactionType = TransactionTypes.POS,
                    Amount = 50.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "GROCERY",
                    Channel = "POS",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_01", 
                        CreditScore = 350, 
                        ClientSegment = ClientSegments.STANDARD,
                        BusinessType = BusinessTypes.INDIVIDUAL,  
                        RiskLevel = RiskLevels.MEDIUM,
                        HasActivePromotions = false,
                        ClientSince = DateTime.UtcNow.AddMonths(-6),
                        MonthlyVolume = 1000,
                        TransactionCountThisMonth = 10,
                        ActivePromotions = new List<string>()
                    }
                },
                new TransactionRequest
                {
                    TransactionId = "POS_UNDER_100_02",
                    TransactionType = TransactionTypes.POS,
                    Amount = 99.99m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "RESTAURANT",
                    Channel = "POS",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_02", 
                        CreditScore = 600, 
                        ClientSegment = ClientSegments.PREMIUM,
                        BusinessType = BusinessTypes.INDIVIDUAL,
                        RiskLevel = RiskLevels.LOW,
                        HasActivePromotions = false,
                        ClientSince = DateTime.UtcNow.AddYears(-1),
                        MonthlyVolume = 2000,
                        TransactionCountThisMonth = 15,
                        ActivePromotions = new List<string>()
                    }
                },
                // POS over €100 - should get 0.2% fee
                new TransactionRequest
                {
                    TransactionId = "POS_OVER_100_01",
                    TransactionType = TransactionTypes.POS,
                    Amount = 150.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "RETAIL",
                    Channel = "POS",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_03", 
                        CreditScore = 300, 
                        ClientSegment = ClientSegments.STANDARD,
                        BusinessType = BusinessTypes.INDIVIDUAL,
                        RiskLevel = RiskLevels.MEDIUM,
                        HasActivePromotions = false,
                        ClientSince = DateTime.UtcNow.AddMonths(-3),
                        MonthlyVolume = 800,
                        TransactionCountThisMonth = 8,
                        ActivePromotions = new List<string>()
                    }
                },
                new TransactionRequest
                {
                    TransactionId = "POS_OVER_100_02",
                    TransactionType = TransactionTypes.POS,
                    Amount = 500.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "GAS_STATION",
                    Channel = "POS",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_04", 
                        CreditScore = 450, 
                        ClientSegment = ClientSegments.PREMIUM,
                        BusinessType = BusinessTypes.INDIVIDUAL,
                        RiskLevel = RiskLevels.LOW,
                        HasActivePromotions = false,
                        ClientSince = DateTime.UtcNow.AddYears(-2),
                        MonthlyVolume = 5000,
                        TransactionCountThisMonth = 20,
                        ActivePromotions = new List<string>()
                    }
                }
            });
            
            // Scenario 2: E-commerce transactions to test Rule #2
            scenarios.AddRange(new[]
            {
                // E-commerce with normal fee calculation
                new TransactionRequest
                {
                    TransactionId = "ECOM_NORMAL_01",
                    TransactionType = TransactionTypes.E_COMMERCE,
                    Amount = 100.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "ONLINE_RETAIL",
                    Channel = "WEB",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_05", 
                        CreditScore = 380, 
                        ClientSegment = ClientSegments.STANDARD,
                        BusinessType = BusinessTypes.INDIVIDUAL,
                        RiskLevel = RiskLevels.MEDIUM,
                        HasActivePromotions = false,
                        ClientSince = DateTime.UtcNow.AddMonths(-8),
                        MonthlyVolume = 1500,
                        TransactionCountThisMonth = 12,
                        ActivePromotions = new List<string>()
                    }
                },
                // E-commerce hitting the €120 cap
                new TransactionRequest
                {
                    TransactionId = "ECOM_CAP_01",
                    TransactionType = TransactionTypes.E_COMMERCE,
                    Amount = 10000.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "DIGITAL_SERVICES",
                    Channel = "WEB",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_06", 
                        CreditScore = 320, 
                        ClientSegment = ClientSegments.STANDARD,
                        BusinessType = BusinessTypes.BUSINESS,
                        RiskLevel = RiskLevels.MEDIUM,
                        HasActivePromotions = false,
                        ClientSince = DateTime.UtcNow.AddMonths(-4),
                        MonthlyVolume = 15000,
                        TransactionCountThisMonth = 25,
                        ActivePromotions = new List<string>()
                    }
                }
            });
            
            // Scenario 3: High credit score discount to test Rule #3
            scenarios.AddRange(new[]
            {
                // POS with high credit score (should get 1% discount)
                new TransactionRequest
                {
                    TransactionId = "HIGH_CREDIT_POS_01",
                    TransactionType = TransactionTypes.POS,
                    Amount = 200.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "GROCERY",
                    Channel = "POS",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_07", 
                        CreditScore = 450, 
                        ClientSegment = ClientSegments.PREMIUM,
                        BusinessType = BusinessTypes.INDIVIDUAL,
                        RiskLevel = RiskLevels.LOW,
                        HasActivePromotions = false,
                        ClientSince = DateTime.UtcNow.AddYears(-3),
                        MonthlyVolume = 8000,
                        TransactionCountThisMonth = 30,
                        ActivePromotions = new List<string>()
                    }
                },
                // E-commerce with high credit score
                new TransactionRequest
                {
                    TransactionId = "HIGH_CREDIT_ECOM_01",
                    TransactionType = TransactionTypes.E_COMMERCE,
                    Amount = 1000.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "ONLINE_RETAIL",
                    Channel = "WEB",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "TEST_CLIENT_08", 
                        CreditScore = 750, 
                        ClientSegment = ClientSegments.VIP,
                        BusinessType = BusinessTypes.INDIVIDUAL,
                        RiskLevel = RiskLevels.LOW,
                        HasActivePromotions = true,
                        ClientSince = DateTime.UtcNow.AddYears(-5),
                        MonthlyVolume = 25000,
                        TransactionCountThisMonth = 50,
                        ActivePromotions = new List<string> { "LOYALTY", "PREMIUM" }
                    }
                }
            });
            
            // Scenario 4: VIP clients (for future rule testing)
            scenarios.AddRange(new[]
            {
                new TransactionRequest
                {
                    TransactionId = "VIP_CLIENT_01",
                    TransactionType = TransactionTypes.POS,
                    Amount = 300.00m,
                    Currency = "EUR",
                    IsDomestic = true,
                    MerchantCategory = "RETAIL",
                    Channel = "POS",
                    IsRecurring = false,
                    TransactionDate = DateTime.UtcNow,
                    Client = new ClientInfo 
                    { 
                        ClientId = "VIP_CLIENT_001", 
                        CreditScore = 800, 
                        ClientSegment = ClientSegments.VIP,
                        BusinessType = BusinessTypes.INDIVIDUAL,
                        RiskLevel = RiskLevels.LOW,
                        HasActivePromotions = true,
                        ClientSince = DateTime.UtcNow.AddYears(-10),
                        MonthlyVolume = 25000,
                        TransactionCountThisMonth = 40,
                        ActivePromotions = new List<string> { "VIP_EXCLUSIVE", "LOYALTY" }
                    }
                }
            });
            
            return scenarios;
        }
        
        // Generate a batch for performance testing
        public static BatchTransactionRequest GeneratePerformanceTestBatch(int size = 1000)
        {
            return new BatchTransactionRequest
            {
                BatchId = $"PERF_TEST_BATCH_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Transactions = GenerateTestTransactions(size),
                RequestedAt = DateTime.UtcNow
            };
        }
    }
}