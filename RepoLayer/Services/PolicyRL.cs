using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RepoLayer.Services
{
    public class PolicyRL : IPolicyRL
    {
        private readonly ApplicationDbContext _context;

        public PolicyRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Policy> CreatePolicyAsync(Policy policyEntity)
        {
            var policyIdParam = new SqlParameter("@PolicyId", SqlDbType.Int) { Direction = ParameterDirection.Output }; // Changed from BigInt to Int
            var parameters = new[]
            {
                new SqlParameter("@CustomerId", SqlDbType.Int) { Value = policyEntity.CustomerId }, // Changed from BigInt to Int
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

            var policyId = (int)policyIdParam.Value; // Changed from long to int
            var createdPolicy = await _context.Policies.FindAsync(policyId);
            if (createdPolicy == null)
            {
                throw new Exception("Failed to retrieve the newly created policy.");
            }

            return createdPolicy;
        }

        public async Task<List<Policy>> GetAllPoliciesAsync()
        {
            return await _context.Policies.ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId) // Changed from long to int
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
    }
}