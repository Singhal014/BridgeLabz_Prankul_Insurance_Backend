﻿using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyBL _policyBL;
        private readonly ILogger<PolicyController> _logger;

        public PolicyController(IPolicyBL policyBL, ILogger<PolicyController> logger)
        {
            _policyBL = policyBL;
            _logger = logger;
        }

        [HttpPost]
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


        [Authorize(Roles = "Customer")]
        [HttpPost("create-policy/customer")]
        public async Task<IActionResult> CreatePolicyForCustomer([FromBody] CustomerPolicyModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var idClaim = User.FindFirst("Id");
                if (idClaim == null || !int.TryParse(idClaim.Value, out int customerId))
                {
                    _logger.LogError("Customer Id not found in JWT token.");
                    return Unauthorized(new ResponseModel<string>
                    {
                        Success = false,
                        Message = "Invalid token - customer identification missing",
                        Data = null
                    });
                }

                var policyModel = new PolicyModel
                {
                    CustomerId = customerId,  
                    PlanId = model.PlanId,
                    SchemeId = model.SchemeId,
                    MaturityPeriod = model.MaturityPeriod
                };

                var result = await _policyBL.CreatePolicyAsync(policyModel, customerId);

                if (result != null)
                {
                    _logger.LogInformation("Policy created successfully for CustomerId: {CustomerId}", customerId);
                    return Ok(new ResponseModel<Policy>
                    {
                        Success = true,
                        Message = "Policy created successfully",
                        Data = result
                    });
                }

                return BadRequest(new ResponseModel<string>
                {
                    Success = false,
                    Message = "Policy creation failed",
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
                return Unauthorized(new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating policy for customer.");
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = ex.Message
                });
            }
        }



        [HttpGet]
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

        [HttpGet("by-customer")]
        [Authorize(Roles = "Admin,Employee,Agent,Customer")]
        public async Task<IActionResult> GetByCustomerId(int customerId) 
        {
            var policies = await _policyBL.GetPoliciesByCustomerIdAsync(customerId);
            return Ok(new ResponseModel<List<Policy>>
            {
                Success = true,
                Message = "Customer policies fetched",
                Data = policies
            });
        }

        [HttpGet("by-status")]
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
        [HttpPut("cancel")]
        public async Task<IActionResult> CancelPolicy(int policyId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var result = await _policyBL.CancelPolicyAsync(policyId, userId);

                var response = new ResponseModel<Policy>
                {
                    Success = true,
                    Message = "Policy cancelled successfully",
                    Data = result
                };

                _logger.LogInformation("Policy with ID {PolicyId} cancelled by user {UserId}", policyId, userId);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized cancel attempt for policy {PolicyId}: {Message}", policyId, ex.Message);
                return Unauthorized(new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling policy with ID {PolicyId}", policyId);
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "Failed to cancel policy",
                    Data = ex.Message
                });
            }
        }

    }
}