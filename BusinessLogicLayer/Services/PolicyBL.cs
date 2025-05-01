using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class PolicyBL : IPolicyBL
    {
        private readonly IPolicyRL _policyRL;
        private readonly ILogger<PolicyBL> _logger;

        public PolicyBL(IPolicyRL policyRL, ILogger<PolicyBL> logger)
        {
            _policyRL = policyRL;
            _logger = logger;
        }

        public async Task<Policy> CreatePolicyAsync(PolicyModel model, int userId) 
        {
            try
            {
                decimal premium = CalculatePremium(model.MaturityPeriod);

                var policyEntity = new Policy
                {
                    CustomerId = model.CustomerId,
                    PlanId = model.PlanId,
                    SchemeId = model.SchemeId,
                    PremiumAmount = premium,
                    MaturityPeriod = model.MaturityPeriod,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                var createdPolicy = await _policyRL.CreatePolicyAsync(policyEntity);

                _logger.LogInformation("Policy created successfully for customer {CustomerId} by user {UserId} with premium {Premium:C}",
                    model.CustomerId, userId, premium);

                return createdPolicy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create policy for customer {CustomerId} by user {UserId}", model.CustomerId, userId);
                throw;
            }
        }

        public async Task<List<Policy>> GetAllPoliciesAsync()
        {
            return await _policyRL.GetAllPoliciesAsync();
        }

        public async Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId) // Changed from long to int
        {
            return await _policyRL.GetPoliciesByCustomerIdAsync(customerId);
        }

        public async Task<List<Policy>> GetPoliciesByStatusAsync(string status)
        {
            return await _policyRL.GetPoliciesByStatusAsync(status);
        }

        private decimal CalculatePremium(int maturityPeriod)
        {
            decimal staticBaseRate = 100.00m;
            decimal additionalFactor = staticBaseRate * 0.10m;
            return (staticBaseRate * maturityPeriod) + additionalFactor;
        }
    }
}