using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModelLayer.Models;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class PaymentRL : IPaymentRL
    {
        private readonly ApplicationDbContext _context;

        public PaymentRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            var paymentIdParam = new SqlParameter("@PaymentId", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var parameters = new[]
            {
    new SqlParameter("@CustomerId", SqlDbType.Int) { Value = payment.CustomerId },
    new SqlParameter("@PolicyId", SqlDbType.Int) { Value = payment.PolicyId },
    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = payment.Amount },
    new SqlParameter("@PaymentDate", SqlDbType.DateTime) { Value = payment.PaymentDate },
    new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = payment.CreatedAt },
    paymentIdParam
};

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_ProcessPayment @CustomerId, @PolicyId, @Amount, @PaymentDate, @CreatedAt, @PaymentId OUTPUT",
                parameters);

            var paymentId = Convert.ToInt32(paymentIdParam.Value);
            var createdPayment = await _context.Payments.FindAsync(paymentId);
            return createdPayment ?? throw new Exception("Failed to retrieve newly created payment.");
        }
        public async Task<List<PaymentViewModel>> GetCustomerPaymentHistoryAsync(long customerId)
        {
            return await (from payment in _context.Payments
                          join policy in _context.Policies on payment.PolicyId equals policy.PolicyId
                          where payment.CustomerId == customerId
                          select new PaymentViewModel
                          {
                              PaymentId = payment.PaymentId,
                              PolicyId = payment.PolicyId,
                              Amount = payment.Amount,
                              PaymentDate = payment.PaymentDate,
                              PremiumAmount = policy.PremiumAmount,
                              PolicyStatus = policy.Status
                          }).ToListAsync();
        }

    }
}
