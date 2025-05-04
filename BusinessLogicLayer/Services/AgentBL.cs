using BusinessLogicLayer.Helper;
using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class AgentBL : IAgentBL
    {
        private readonly IAgentRL _agentRL;
        private readonly ILogger<AgentBL> _logger;
        private readonly IConfiguration _config;
        private readonly IOtpServiceBL _otpService;

        public AgentBL(
            IAgentRL agentRL,
            ILogger<AgentBL> logger,
            IConfiguration config,
            IOtpServiceBL otpService)
        {
            _agentRL = agentRL;
            _logger = logger;
            _config = config;
            _otpService = otpService;
        }

        public async Task<Agent> RegisterAgentAsync(AdminModel model)
        {
            try
            {
                var existingAgent = await _agentRL.GetAgentByEmailAsync(model.Email);
                if (existingAgent != null)
                {
                    _logger.LogWarning("Registration attempt with already registered email: {Email}", model.Email);
                    throw new InvalidOperationException("This email is already registered.");
                }

                var hashedPassword = PasswordHelper.PasswordHash(model.Password);

                var agentEntity = new Agent
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = hashedPassword,
                    Role = "Agent"
                };

                var result = await _agentRL.RegisterAgentAsync(agentEntity);

                RabbitMQProducer.EnqueueUserRegistration(new UserRegistrationModel
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = result.Role
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during agent registration for email: {Email}", model.Email);
                throw new Exception("An unexpected error occurred during registration.");
            }
        }

        public async Task<string> LoginAgentAsync(LoginModel model)
        {
            try
            {
                var agent = await _agentRL.GetAgentByEmailAsync(model.Email);
                if (agent == null || !PasswordHelper.VerifyPassword(model.Password, agent.Password))
                    throw new UnauthorizedAccessException("Invalid credentials");

                var token = JwtHelper.GenerateToken(
                    agent.AgentID,
                    agent.Email,
                    agent.FullName,
                    agent.Role,
                    _config);

                return token;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login attempt failed for email: {Email} - {Message}", model.Email, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", model.Email);
                throw new Exception("An unexpected error occurred during login.");
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPassword model)
        {
            try
            {
                var agent = await _agentRL.GetAgentByEmailAsync(model.Email);
                if (agent == null)
                    return false;

                var otp = await _otpService.GenerateOtpAsync();
                await _otpService.StoreOtpAsync(model.Email, otp);

                RabbitMQProducer.EnqueueEmail(new EmailModel
                {
                    ToEmail = model.Email,
                    Subject = "Password Reset OTP",
                    Body = $"Your OTP for password reset is: {otp}. It will expire in 10 minutes."
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed for email: {Email}", model.Email);
                throw new Exception("An unexpected error occurred during forgot password request.");
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordModel model)
        {
            try
            {
                var isValidOtp = await _otpService.ValidateOtpAsync(model.Email, model.Otp);
                if (!isValidOtp)
                    return false;

                var agent = await _agentRL.GetAgentByEmailAsync(model.Email);
                if (agent == null)
                    return false;

                var hashedPassword = PasswordHelper.PasswordHash(model.NewPassword);
                agent.Password = hashedPassword;

                await _agentRL.UpdateAgentAsync(agent);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password failed for email: {Email}", model.Email);
                throw new Exception("An unexpected error occurred during password reset.");
            }
        }
    }
}
