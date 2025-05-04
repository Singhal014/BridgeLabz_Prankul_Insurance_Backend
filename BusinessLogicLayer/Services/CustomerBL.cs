using BusinessLogicLayer.Helper;
using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using RepoLayer.Services;
using System;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class CustomerBL : ICustomerBL
    {
        private readonly ICustomerRL _customerRL;
        private readonly IAgentRL _agentRL;
        private readonly ILogger<CustomerBL> _logger;
        private readonly IConfiguration _config;
        private readonly IOtpServiceBL _otpService;

        public CustomerBL(
            ICustomerRL customerRL,
            IAgentRL agentRL,
            ILogger<CustomerBL> logger,
            IConfiguration config,
            IOtpServiceBL otpService)
        {
            _customerRL = customerRL;
            _agentRL = agentRL;
            _logger = logger;
            _config = config;
            _otpService = otpService;
        }

        public async Task<Customer> RegisterCustomerAsync(CustomerRegistration model)
        {
            try
            {
                var existingCustomer = await _customerRL.GetCustomerByEmailAsync(model.Email);
                if (existingCustomer != null)
                {
                    _logger.LogWarning("Registration attempt with already registered email: {Email}", model.Email);
                    throw new InvalidOperationException("This email is already registered.");
                }

                if (model.AgentID.HasValue)
                {
                    var agentExists = await _agentRL.GetAgentByIdAsync(model.AgentID.Value);
                    if (agentExists == null)
                    {
                        _logger.LogWarning("Invalid AgentID provided during customer registration: {AgentID}", model.AgentID.Value);
                        throw new ArgumentException("Invalid AgentID provided.");
                    }
                }

                var hashed = PasswordHelper.PasswordHash(model.Password);

                var entity = new Customer
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = hashed,
                    Phone = model.Phone,
                    DateOfBirth = model.DateOfBirth,
                    AgentID = model.AgentID,
                    Role = "Customer",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _customerRL.RegisterCustomerAsync(entity);

                try
                {
                    RabbitMQProducer.EnqueueUserRegistration(new UserRegistrationModel
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        Role = result.Role
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ enqueue failed for customer: {Email}", model.Email);
                }

                _logger.LogInformation("Customer registered & queued for email: {Email}", model.Email);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during customer registration for email: {Email}", model.Email);
                throw;  
            }
        }

        public async Task<string> LoginCustomerAsync(LoginModel model)
        {
            try
            {
                var customer = await _customerRL.GetCustomerByEmailAsync(model.Email);
                if (customer == null)
                {
                    _logger.LogWarning("Login attempt failed for email: {Email} - Customer not found", model.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                if (!PasswordHelper.VerifyPassword(model.Password, customer.Password))
                {
                    _logger.LogWarning("Login attempt failed for email: {Email} - Invalid password", model.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                var token = JwtHelper.GenerateToken(
                    customer.CustomerID,
                    customer.Email,
                    customer.FullName,
                    customer.Role,
                    _config);

                _logger.LogInformation("Customer logged in successfully: {Email}", model.Email);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login attempt for email: {Email}", model.Email);
                throw;  
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPassword model)
        {
            try
            {
                var customer = await _customerRL.GetCustomerByEmailAsync(model.Email);
                if (customer == null)
                {
                    _logger.LogWarning("Forgot password attempt for non-existent email: {Email}", model.Email);
                    return false;
                }

                var otp = await _otpService.GenerateOtpAsync();
                await _otpService.StoreOtpAsync(model.Email, otp);

                RabbitMQProducer.EnqueueEmail(new EmailModel
                {
                    ToEmail = model.Email,
                    Subject = "Password Reset OTP",
                    Body = $"Your OTP for password reset is: {otp}. It will expire in 10 minutes."
                });

                _logger.LogInformation("OTP queued for sending to email: {Email}", model.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing forgot password request for email: {Email}", model.Email);
                throw;  
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordModel model)
        {
            try
            {
                var isValidOtp = await _otpService.ValidateOtpAsync(model.Email, model.Otp);
                if (!isValidOtp)
                {
                    _logger.LogWarning("Invalid OTP provided for email: {Email}", model.Email);
                    return false;
                }

                var customer = await _customerRL.GetCustomerByEmailAsync(model.Email);
                if (customer == null)
                {
                    _logger.LogWarning("Reset password attempt for non-existent email: {Email}", model.Email);
                    return false;
                }

                var hashedPassword = PasswordHelper.PasswordHash(model.NewPassword);
                customer.Password = hashedPassword;

                await _customerRL.UpdateCustomerAsync(customer);

                _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resetting the password for email: {Email}", model.Email);
                throw;  
            }
        }
    }
}
