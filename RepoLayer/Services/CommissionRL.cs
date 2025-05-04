using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
namespace RepoLayer.Services
{
    public class CommissionRL : ICommissionRL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommissionRL> _logger;

        public CommissionRL(ApplicationDbContext context, ILogger<CommissionRL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Commission> CalculateCommissionAsync(Commission commission)
        {
            try
            {
                var existingCommission = await _context.Commissions
                    .FirstOrDefaultAsync(c => c.AgentId == commission.AgentId && c.PolicyId == commission.PolicyId);

                if (existingCommission != null)
                {
                    _logger.LogWarning("Duplicate commission entry found for AgentId: {AgentId}, PolicyId: {PolicyId}.", commission.AgentId, commission.PolicyId);
                    return existingCommission;
                }

                var paramId = new SqlParameter("@CommissionId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var parameters = new[]
                {
                    new SqlParameter("@AgentId", SqlDbType.Int) { Value = commission.AgentId },
                    new SqlParameter("@PolicyId", SqlDbType.Int) { Value = commission.PolicyId },
                    new SqlParameter("@CommissionAmount", SqlDbType.Decimal) { Value = commission.CommissionAmount },
                    new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = commission.CreatedAt },
                    paramId
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_CalculateCommission @AgentId, @PolicyId, @CommissionAmount, @CreatedAt, @CommissionId OUTPUT",
                    parameters);

                var commissionId = Convert.ToInt32(paramId.Value);
                var created = await _context.Commissions.FindAsync(commissionId);

                return created ?? throw new Exception("Failed to retrieve the calculated commission.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calculating commission for AgentId: {AgentId}, PolicyId: {PolicyId}.", commission.AgentId, commission.PolicyId);
                throw;
            }
        }

        public async Task<List<Commission>> GetAllCommissionsByAgentIdAsync(int agentId)
        {
            try
            {
                var commissions = await _context.Commissions
                    .FromSqlRaw("EXEC dbo.sp_GetAllCommissionsByAgentId @AgentId",
                        new SqlParameter("@AgentId", agentId))
                    .ToListAsync();

                return commissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all commissions for AgentId: {AgentId}", agentId);
                throw;
            }
        }
    }
}
