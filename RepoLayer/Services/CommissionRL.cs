using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class CommissionRL : ICommissionRL
    {
        private readonly ApplicationDbContext _context;

        public CommissionRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Commission> CalculateCommissionAsync(Commission commission)
        {
            var paramId = new SqlParameter("@CommissionId", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
            var parameters = new[]
            {
                new SqlParameter("@AgentId", SqlDbType.BigInt) { Value = commission.AgentId },
                new SqlParameter("@PolicyId", SqlDbType.BigInt) { Value = commission.PolicyId },
                new SqlParameter("@CommissionAmount", SqlDbType.Decimal) { Value = commission.CommissionAmount },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = commission.CreatedAt },
                paramId
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_CalculateCommission @AgentId, @PolicyId, @CommissionAmount, @CreatedAt, @CommissionId OUTPUT",
                parameters);

            var created = await _context.Commissions.FindAsync((long)paramId.Value);
            return created ?? throw new System.Exception("Failed to retrieve commission.");
        }
    }
}
