using Microsoft.AspNetCore.Mvc;
using FeeCalculator.Core.Interfaces;
using FeeCalculator.Infrastructure.Services;

namespace FeeCalculator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RuleManagementController : ControllerBase
    {
        private readonly FeeCalculationService _feeCalculationService;
        private readonly ILogger<RuleManagementController> _logger;

        public RuleManagementController(
            IFeeCalculationService feeCalculationService,
            ILogger<RuleManagementController> logger)
        {
            _feeCalculationService = (FeeCalculationService)feeCalculationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all available rule processors
        /// </summary>
        /// <returns>List of rule processors with their status</returns>
        [HttpGet("processors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetRuleProcessors()
        {
            try
            {
                var processors = await _feeCalculationService.GetAllProcessorsAsync();
                
                var result = processors.Select(p => new
                {
                    p.RuleId,
                    p.RuleName,
                    p.Description,
                    p.RuleType,
                    p.Priority,
                    p.IsActive,
                    Parameters = p.GetRuleParameters()
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rule processors");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Toggle rule processor status (activate/deactivate)
        /// </summary>
        /// <param name="ruleId">Rule ID to toggle</param>
        /// <param name="isActive">New status</param>
        /// <returns>Success response</returns>
        [HttpPut("processors/{ruleId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ToggleRuleStatus(int ruleId, [FromQuery] bool isActive)
        {
            try
            {
                var success = await _feeCalculationService.ToggleProcessorStatusAsync(ruleId, isActive);
                
                if (!success)
                {
                    return NotFound($"Rule processor with ID {ruleId} not found");
                }
                
                _logger.LogInformation("{Action} rule processor {RuleId}", 
                    isActive ? "Activated" : "Deactivated", ruleId);
                
                return Ok(new { message = $"Rule processor {ruleId} {(isActive ? "activated" : "deactivated")} successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling rule processor status for {RuleId}", ruleId);
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }
}