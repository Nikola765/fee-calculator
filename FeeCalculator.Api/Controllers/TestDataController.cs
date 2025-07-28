using Microsoft.AspNetCore.Mvc;
using FeeCalculator.Core.Models;
using FeeCalculator.Infrastructure.Services;

namespace FeeCalculator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class TestDataController : ControllerBase
    {
        private readonly ILogger<TestDataController> _logger;

        public TestDataController(ILogger<TestDataController> logger)
        {
            _logger = logger;
        }

        // Generate random transaction data for testing
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

        // Generate scenario-specific test data for rule validation
        [HttpGet("scenarios")]
        [ProducesResponseType(typeof(List<TransactionRequest>), StatusCodes.Status200OK)]
        public ActionResult<List<TransactionRequest>> GenerateScenarios()
        {
            var scenarios = TestDataGenerator.GenerateScenarioTestData();
            
            _logger.LogInformation("Generated {Count} scenario test transactions", scenarios.Count);
            return Ok(scenarios);
        }

        // Generate a batch for performance testing
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
