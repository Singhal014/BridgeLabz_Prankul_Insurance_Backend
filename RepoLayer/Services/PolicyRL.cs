using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace RepoLayer.Services
{
    public class PolicyRL : IPolicyRL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PolicyRL> _logger;

        public PolicyRL(ApplicationDbContext context, ILogger<PolicyRL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Policy> CreatePolicyAsync(Policy policyEntity)
        {
            try
            {
                var existingPolicy = await _context.Policies
                    .FirstOrDefaultAsync(p => p.CustomerId == policyEntity.CustomerId &&
                                               p.PlanId == policyEntity.PlanId &&
                                               p.SchemeId == policyEntity.SchemeId &&
                                               p.Status == "Active");

                if (existingPolicy != null)
                {
                    _logger.LogWarning($"Policy already exists and is active for CustomerId: {policyEntity.CustomerId}, PlanId: {policyEntity.PlanId}, SchemeId: {policyEntity.SchemeId}");
                    throw new Exception("Policy already exists and is active.");
                }

                var policyIdParam = new SqlParameter("@PolicyId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var parameters = new[]
                {
                    new SqlParameter("@CustomerId", SqlDbType.Int) { Value = policyEntity.CustomerId },
                    new SqlParameter("@PlanId", SqlDbType.Int) { Value = policyEntity.PlanId },
                    new SqlParameter("@SchemeId", SqlDbType.Int) { Value = policyEntity.SchemeId },
                    new SqlParameter("@PremiumAmount", SqlDbType.Decimal) { Value = policyEntity.PremiumAmount },
                    new SqlParameter("@MaturityPeriod", SqlDbType.Int) { Value = policyEntity.MaturityPeriod },
                    new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = policyEntity.Status },
                    new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = policyEntity.CreatedAt },
                    policyIdParam
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_CreatePolicy @CustomerId, @PlanId, @SchemeId, @PremiumAmount, @MaturityPeriod, @Status, @CreatedAt, @PolicyId OUTPUT",
                    parameters);

                var policyId = Convert.ToInt32(policyIdParam.Value);
                var createdPolicy = await _context.Policies.FindAsync(policyId);

                if (createdPolicy == null)
                {
                    throw new Exception("Failed to retrieve the newly created policy.");
                }

                _logger.LogInformation($"Policy created successfully for CustomerId: {policyEntity.CustomerId}, PlanId: {policyEntity.PlanId}, SchemeId: {policyEntity.SchemeId}");
                return createdPolicy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating policy for CustomerId: {CustomerId}", policyEntity.CustomerId);
                throw;
            }
        }

        public async Task<List<Policy>> GetAllPoliciesAsync()
        {
            return await _context.Policies.ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            return await _context.Policies
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByStatusAsync(string status)
        {
            return await _context.Policies
                .Where(p => p.Status == status)
                .ToListAsync();
        }
        public async Task<Policy> GetPolicyByCustomerPlanSchemeAsync(int customerId, int planId, int schemeId)
        {
            return await _context.Policies
                .FirstOrDefaultAsync(p => p.CustomerId == customerId &&
                                           p.PlanId == planId &&
                                           p.SchemeId == schemeId);
        }
        public async Task<Policy> CancelPolicyAsync(int policyId)
        {
            try
            {
                var policyIdParam = new SqlParameter("@PolicyId", policyId);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_CancelPolicy @PolicyId",
                    policyIdParam
                );

                var updatedPolicy = await _context.Policies.FindAsync(policyId);
                if (updatedPolicy == null)
                {
                    throw new Exception("Policy cancellation failed. Policy not found after update.");
                }

                _logger.LogInformation("Policy with ID {PolicyId} cancelled successfully.", policyId);
                return updatedPolicy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling policy with ID {PolicyId}", policyId);
                throw;
            }
        }
        public async Task<List<Policy>> GetExpiredPoliciesAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Policies
                .Where(p => p.Status == "Active" &&
                           EF.Functions.DateDiffDay(p.CreatedAt, currentDate) >= p.MaturityPeriod * 365)
                .ToListAsync();
        }

        public async Task<string> GetCustomerEmailAsync(int customerId)
        {
            var customer = await _context.Customers
                .Where(c => c.CustomerID == customerId)
                .FirstOrDefaultAsync();

            return customer?.Email;
        }

    }
}
