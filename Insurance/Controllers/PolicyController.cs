using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee,Agent")]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyBL _policyBL;
        private readonly ILogger<PolicyController> _logger;

        public PolicyController(IPolicyBL policyBL, ILogger<PolicyController> logger)
        {
            _policyBL = policyBL;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePolicy([FromBody] PolicyModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0"); 
                var role = User.FindFirst("Role")?.Value ?? "";

                var policy = await _policyBL.CreatePolicyAsync(model, userId);
                var response = new ResponseModel<Policy>
                {
                    Success = true,
                    Message = "Policy created successfully",
                    Data = policy
                };
                _logger.LogInformation("Policy created successfully for customer {CustomerId} by user {UserId}", model.CustomerId, userId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt to create policy: {Message}", ex.Message);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
                return Unauthorized(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create policy for customer {CustomerId}", model.CustomerId);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = "Failed to create policy",
                    Data = ex.Message
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Employee,Agent")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var policies = await _policyBL.GetAllPoliciesAsync();
            return Ok(new ResponseModel<List<Policy>>
            {
                Success = true,
                Message = "All policies fetched",
                Data = policies
            });
        }

        [HttpGet("by-customer/{customerId}")]
        [Authorize(Roles = "Admin,Employee,Agent,Customer")]
        public async Task<IActionResult> GetByCustomerId(int customerId) // Changed from long to int
        {
            var policies = await _policyBL.GetPoliciesByCustomerIdAsync(customerId);
            return Ok(new ResponseModel<List<Policy>>
            {
                Success = true,
                Message = "Customer policies fetched",
                Data = policies
            });
        }

        [HttpGet("by-status/{status}")]
        [Authorize(Roles = "Admin,Employee,Agent")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var policies = await _policyBL.GetPoliciesByStatusAsync(status);
            return Ok(new ResponseModel<List<Policy>>
            {
                Success = true,
                Message = "Policies with given status fetched",
                Data = policies
            });
        }
    }
}