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
    public class PaymentBL : IPaymentBL
    {
        private readonly IPaymentRL _paymentRL;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentBL> _logger;

        public PaymentBL(IPaymentRL paymentRL, ApplicationDbContext context, ILogger<PaymentBL> logger)
        {
            _paymentRL = paymentRL;
            _context = context;
            _logger = logger;
        }

        public async Task<Payment> ProcessPaymentAsync(PaymentModel model)
        {
            var policy = await _context.Policies.FindAsync(model.PolicyId);
            if (policy == null || policy.CustomerId != model.CustomerId)
            {
                _logger.LogWarning("Invalid policy ({PolicyId}) or customer mismatch ({CustomerId})", model.PolicyId, model.CustomerId);
                throw new Exception("Invalid policy or customer mismatch.");
            }

            var payment = new Payment
            {
                CustomerId = model.CustomerId,
                PolicyId = model.PolicyId,
                Amount = policy.PremiumAmount,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            return await _paymentRL.ProcessPaymentAsync(payment);
        }
        public async Task<List<PaymentViewModel>> GetCustomerPaymentHistoryAsync(int customerId)
        {
            return await _paymentRL.GetCustomerPaymentHistoryAsync(customerId);
        }

    }
}
