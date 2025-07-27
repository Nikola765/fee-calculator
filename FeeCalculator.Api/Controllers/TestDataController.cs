using Microsoft.AspNetCore.Mvc;
using FeeCalculator.Core.Models;
using FeeCalculator.Infrastructure.Services;

namespace FeeCalculator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = false)] // Show in Swagger for demo purposes
    public class TestDataController : ControllerBase
    {
        private readonly ILogger<TestDataController> _logger;

        public TestDataController(ILogger<TestDataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate sample transaction data for testing
        /// </summary>
        /// <param name="count">Number of transactions to generate (max 1000)</param>
        /// <returns>List of sample transactions</returns>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(List<TransactionRequest>), StatusCodes.Status200OK)]
        public ActionResult<List<TransactionRequest>> GenerateTransactions([FromQuery] int count = 10)
        {
            if (count > 1000)
            {
                return BadRequest("Maximum 1000 transactions can be generated at once");
            }

            if (count <= 0)
            {
                return BadRequest("Count must be positive");
            }

            var transactions = TestDataGenerator.GenerateTestTransactions(count);
            
            _logger.LogInformation("Generated {Count} test transactions", count);
            return Ok(transactions);
        }

        /// <summary>
        /// Generate scenario-specific test data for rule validation
        /// </summary>
        /// <returns>Test transactions for specific scenarios</returns>
        [HttpGet("scenarios")]
        [ProducesResponseType(typeof(List<TransactionRequest>), StatusCodes.Status200OK)]
        public ActionResult<List<TransactionRequest>> GenerateScenarios()
        {
            var scenarios = TestDataGenerator.GenerateScenarioTestData();
            
            _logger.LogInformation("Generated {Count} scenario test transactions", scenarios.Count);
            return Ok(scenarios);
        }

        /// <summary>
        /// Generate a batch for performance testing
        /// </summary>
        /// <param name="size">Batch size (max 10000)</param>
        /// <returns>Batch transaction request</returns>
        [HttpGet("performance-batch")]
        [ProducesResponseType(typeof(BatchTransactionRequest), StatusCodes.Status200OK)]
        public ActionResult<BatchTransactionRequest> GeneratePerformanceBatch([FromQuery] int size = 1000)
        {
            if (size > 10000)
            {
                return BadRequest("Maximum batch size is 10,000 transactions");
            }

            if (size <= 0)
            {
                return BadRequest("Size must be positive");
            }

            var batch = TestDataGenerator.GeneratePerformanceTestBatch(size);
            
            _logger.LogInformation("Generated performance test batch with {Size} transactions", size);
            return Ok(batch);
        }
    }
}
