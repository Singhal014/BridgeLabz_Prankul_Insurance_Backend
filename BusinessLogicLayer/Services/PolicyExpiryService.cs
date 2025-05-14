// PolicyExpiryService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RepoLayer.Context;
using RepoLayer.Entity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Insurance.Services
{
    public class PolicyExpiryService : BackgroundService
    {
        private readonly ILogger<PolicyExpiryService> _logger;
        private readonly IServiceProvider _services;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check 4x daily

        public PolicyExpiryService(ILogger<PolicyExpiryService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Policy Expiry Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var now = DateTime.UtcNow;

                        // Get policies expiring in next 7 days
                        var expiringPolicies = context.Policies
                            .Where(p => p.Status == "Active")
                            .AsEnumerable() // Switch to client-side for date math
                            .Where(p => IsExpiringSoon(p, now))
                            .ToList();

                        foreach (var policy in expiringPolicies)
                        {
                            var customer = context.Customers.Find(policy.CustomerId);
                            if (customer != null)
                            {
                                _logger.LogInformation(
                                    "Policy {PolicyId} expiring on {ExpiryDate} for {CustomerEmail}",
                                    policy.PolicyId,
                                    GetExpiryDate(policy).ToString("yyyy-MM-dd"),
                                    customer.Email);

                                // Implement your notification logic here
                                // await SendExpiryNotification(customer, policy);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking policy expirations");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private bool IsExpiringSoon(Policy policy, DateTime now)
        {
            var expiryDate = GetExpiryDate(policy);
            return expiryDate <= now.AddDays(7) && expiryDate > now;
        }

        private DateTime GetExpiryDate(Policy policy)
        {
            return policy.CreatedAt.AddYears(policy.MaturityPeriod);
        }
    }
}