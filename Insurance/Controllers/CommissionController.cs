using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using RepoLayer.Entity;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Employee,Agent")]
public class CommissionController : ControllerBase
{
    private readonly ICommissionBL _commissionBL;
    private readonly ILogger<CommissionController> _logger;

    public CommissionController(ICommissionBL commissionBL, ILogger<CommissionController> logger)
    {
        _commissionBL = commissionBL;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Calculate([FromBody] CommissionModel model)
    {
        if (model == null || model.PolicyId <= 0)
        {
            return BadRequest(new ResponseModel<string>
            {
                Success = false,
                Message = "Invalid input: PolicyId must be greater than 0",
                Data = null
            });
        }

        try
        {
            var commission = await _commissionBL.CalculateCommissionAsync(model);

            return Ok(new ResponseModel<Commission>
            {
                Success = true,
                Message = "Commission calculated successfully",
                Data = commission
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Commission calculation failed for Policy {PolicyId}", model.PolicyId);

            return StatusCode(500, new ResponseModel<string>
            {
                Success = false,
                Message = "Internal server error: " + ex.Message,
                Data = null
            });
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllCommissions()
    {
        try
        {
            var agentId = GetAgentIdFromClaims(); 
            if (agentId == null)
            {
                return Unauthorized(new ResponseModel<string>
                {
                    Success = false,
                    Message = "Agent ID not found in JWT claims.",
                    Data = null
                });
            }

            var (commissions, totalCommission) = await _commissionBL.GetAllCommissionsByAgentIdAsync(agentId.Value);
            return Ok(new ResponseModel<object>
            {
                Success = true,
                Message = "All commissions for the agent with total commission",
                Data = new { Commissions = commissions, TotalCommission = totalCommission }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commissions for agent");
            return StatusCode(500, new ResponseModel<string>
            {
                Success = false,
                Message = "Failed to fetch all commissions",
                Data = ex.Message
            });
        }
    }

    private int? GetAgentIdFromClaims()
    {
        var agentIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "Id")?.Value;
        if (int.TryParse(agentIdClaim, out int agentId))
        {
            return agentId;
        }
        return null;
    }
}
