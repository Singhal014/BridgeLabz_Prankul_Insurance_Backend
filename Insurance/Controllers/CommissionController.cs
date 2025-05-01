using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using System;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CommissionModel model)
        {
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
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
