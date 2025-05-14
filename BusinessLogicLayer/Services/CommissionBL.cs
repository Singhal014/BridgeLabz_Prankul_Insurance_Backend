using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services
{
    public class CommissionBL : ICommissionBL
    {
        private readonly ICommissionRL _commissionRL;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommissionBL> _logger;
        private const decimal COMMISSION_RATE = 0.10m; 

        public CommissionBL(
            ICommissionRL commissionRL,
            ApplicationDbContext context,
            ILogger<CommissionBL> logger)
        {
            _commissionRL = commissionRL;
            _context = context;
            _logger = logger;
        }

        public async Task<Commission> CalculateCommissionAsync(CommissionModel model)
        {
            try
            {
                var policy = await _context.Policies.FindAsync(model.PolicyId);
                if (policy == null)
                {
                    _logger.LogWarning("Policy with ID {PolicyId} not found.", model.PolicyId);
                    throw new Exception($"Policy with ID {model.PolicyId} not found.");
                }

                var customer = await _context.Customers.FindAsync(policy.CustomerId);
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found.", policy.CustomerId);
                    throw new Exception($"Customer with ID {policy.CustomerId} not found.");
                }

                if (!customer.AgentID.HasValue)
                {
                    _logger.LogWarning("Customer {CustomerId} has no assigned agent.", customer.CustomerID);
                    throw new Exception($"Customer {customer.CustomerID} has no assigned agent.");
                }

                int agentId = customer.AgentID.Value;
                decimal commissionAmount = policy.PremiumAmount * COMMISSION_RATE;

                var commission = new Commission
                {
                    AgentId = agentId,
                    PolicyId = policy.PolicyId,
                    CommissionAmount = commissionAmount,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _commissionRL.CalculateCommissionAsync(commission);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calculating commission.");
                throw;
            }
        }

        public async Task<(List<Commission> Commissions, decimal TotalCommission)> GetAllCommissionsByAgentIdAsync(int agentId)
        {
            try
            {
                var commissions = await _commissionRL.GetAllCommissionsByAgentIdAsync(agentId);
                var totalCommission = commissions.Sum(c => c.CommissionAmount);

                return (commissions, totalCommission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all commissions for AgentId: {AgentId}", agentId);
                throw;
            }
        }

        public async Task<string> PayPendingCommissionsAsync()
        {
            try
            {
                var unpaidCommissions = await _context.Commissions
                    .Where(c => !c.IsPaid)
                    .ToListAsync();

                if (unpaidCommissions.Count == 0)
                {
                    return "No pending commissions to pay.";
                }

                decimal totalCommissionAmount = 0m;
                foreach (var commission in unpaidCommissions)
                {
                    commission.IsPaid = true;
                    commission.PaidDate = DateTime.UtcNow;
                    totalCommissionAmount += commission.CommissionAmount;

                    _logger.LogInformation("Commission paid: AgentId={AgentId}, Amount={Amount}, PolicyId={PolicyId}",
                        commission.AgentId, commission.CommissionAmount, commission.PolicyId);
                }

                int rowsAffected = await _context.SaveChangesAsync();

                return $"Payment of {totalCommissionAmount} rupees has been initiated for {rowsAffected} policies.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing pending commission payments.");
                throw;
            }
        }

    }
}
