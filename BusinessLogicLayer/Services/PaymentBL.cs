using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Collections.Generic;
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
            try
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

                var result = await _paymentRL.ProcessPaymentAsync(payment);
                _logger.LogInformation("Payment processed for PaymentId: {PaymentId}", result.PaymentId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for CustomerId: {CustomerId}, PolicyId: {PolicyId}", model.CustomerId, model.PolicyId);
                throw;
            }
        }

        public async Task<List<PaymentViewModel>> GetCustomerPaymentHistoryAsync(int customerId)
        {
            try
            {
                return await _paymentRL.GetCustomerPaymentHistoryAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for CustomerId: {CustomerId}", customerId);
                throw;
            }
        }
    }
}
