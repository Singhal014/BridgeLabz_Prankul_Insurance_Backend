using BusinessLogicLayer.Helper;
using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using NLog;
using RepoLayer.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class PolicyExpiryService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PolicyExpiryService> _logger;
        private Timer _timer;
        private readonly Logger _nlog; // Changed to instance field

        public PolicyExpiryService(
            IServiceScopeFactory scopeFactory,
            ILogger<PolicyExpiryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _nlog = LogManager.GetCurrentClassLogger(); // Initialize here
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Policy Expiry Notification Service is starting.");
            _nlog.Info("Service starting (NLog)"); // Example NLog usage

            // Check every hour for expired policies
            _timer = new Timer(async _ => await CheckExpiredPoliciesAsync(),
                             null,
                             TimeSpan.Zero,
                             TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private async Task CheckExpiredPoliciesAsync() // Changed to async Task
        {
            using var scope = _scopeFactory.CreateScope();
            var policyBL = scope.ServiceProvider.GetRequiredService<IPolicyBL>();

            try
            {
                _logger.LogInformation("Checking for expired policies...");
                _nlog.Info("Checking expired policies");

                var expiredPolicies = await policyBL.GetExpiredPoliciesAsync();

                foreach (var policy in expiredPolicies)
                {
                    if (IsExpiredToday(policy))
                    {
                        var customerEmail = await policyBL.GetCustomerEmailAsync(policy.CustomerId);

                        if (!string.IsNullOrEmpty(customerEmail))
                        {
                            await SendExpiryNotificationAsync(policy, customerEmail); // Made async
                            _nlog.Info($"Sent expiry notification for policy {policy.PolicyId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expired policies");
                _nlog.Error(ex, "Error in PolicyExpiryService");
            }
        }

        private bool IsExpiredToday(Policy policy)
        {
            var expiryDate = policy.CreatedAt.AddYears(policy.MaturityPeriod);
            return expiryDate.Date == DateTime.UtcNow.Date;
        }

        private async Task SendExpiryNotificationAsync(Policy policy, string customerEmail)
        {
            try
            {
                var email = new EmailModel
                {
                    ToEmail = customerEmail,
                    Subject = "Your Insurance Policy Has Expired",
                    Body = $"<p>Dear Customer,</p>" +
                           $"<p>Your policy #{policy.PolicyId} has expired on {policy.CreatedAt.AddYears(policy.MaturityPeriod):dd-MMM-yyyy}.</p>" +
                           $"<p>Please contact us to renew your policy.</p>" +
                           $"<p>Thank you,<br/>Insurance Team</p>"
                };

                await Task.Run(() => RabbitMQProducer.EnqueueEmail(email)); // Async wrapper
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send policy expiry notification");
                _nlog.Error(ex, "Failed to send notification");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Policy Expiry Notification Service is stopping.");
            _nlog.Info("Service stopping");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            LogManager.Shutdown(); // Proper NLog cleanup
        }
    }
}