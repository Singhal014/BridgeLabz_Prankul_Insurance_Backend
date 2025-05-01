using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Interfaces;
using ModelLayer.Models;
using System.Data;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class PremiumRL : IPremiumRL
    {
        private readonly ApplicationDbContext _context;

        public PremiumRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculatePremiumAsync(PremiumModel model)
        {
            // OUTPUT parameter for the stored procedure
            var premiumParam = new SqlParameter("@PremiumAmount", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Direction = ParameterDirection.Output
            };

            var parameters = new[]
            {
                new SqlParameter("@CustomerId", SqlDbType.BigInt) { Value = model.CustomerId },
                new SqlParameter("@PlanId", SqlDbType.Int) { Value = model.PlanId },
                new SqlParameter("@SchemeId", SqlDbType.Int) { Value = model.SchemeId },
                new SqlParameter("@MaturityPeriod", SqlDbType.Int) { Value = model.MaturityPeriod },
                premiumParam
            };

            // Execute stored procedure
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_CalculatePremium @CustomerId, @PlanId, @SchemeId, @MaturityPeriod, @PremiumAmount OUTPUT",
                parameters);

            return (decimal)premiumParam.Value;
        }
    }
}
