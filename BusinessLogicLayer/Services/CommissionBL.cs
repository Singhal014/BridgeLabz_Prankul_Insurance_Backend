using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class CommissionBL : ICommissionBL
    {
        private readonly ICommissionRL _commissionRL;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommissionBL> _logger;
        private const decimal COMMISSION_RATE = 0.10m; // 10% commission rate

        public CommissionBL(ICommissionRL commissionRL,
                            ApplicationDbContext context,
                            ILogger<CommissionBL> logger)
        {
            _commissionRL = commissionRL;
            _context = context;
            _logger = logger;
        }

        public async Task<Commission> CalculateCommissionAsync(CommissionModel model)
        {
            // Fetch the policy details
            var policy = await _context.Policies.FindAsync(model.PolicyId);
            if (policy == null)
                throw new Exception($"Policy with ID {model.PolicyId} not found.");

            // Fetch the customer who owns the policy
            var customer = await _context.Customers.FindAsync(policy.CustomerId);
            if (customer == null)
                throw new Exception($"Customer with ID {policy.CustomerId} not found.");

            // Ensure the customer has an assigned agent
            if (!customer.AgentID.HasValue)
                throw new Exception($"Customer {customer.CustomerID} has no assigned agent.");

            // Cast AgentID (int?) → long (or whatever your Commission.AgentId is)
            long agentId = Convert.ToInt64(customer.AgentID.Value);

            // Calculate the commission based on the premium amount
            decimal commissionAmount = policy.PremiumAmount * COMMISSION_RATE;

            var commission = new Commission
            {
                AgentId = agentId,
                PolicyId = policy.PolicyId,
                CommissionAmount = commissionAmount,
                CreatedAt = DateTime.UtcNow
            };

            // Save commission via repository
            var result = await _commissionRL.CalculateCommissionAsync(commission);
            _logger.LogInformation(
                "Commission {CommissionId} calculated for Agent {AgentId}",
                result.CommissionId, agentId);

            return result;
        }

    }
}
