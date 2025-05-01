using BusinessLogicLayer.Helper;
using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
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
            // Check if email already exists
            var existingAgent = await _agentRL.GetAgentByEmailAsync(model.Email);
            if (existingAgent != null)
            {
                _logger.LogWarning("Registration attempt with already registered email: {Email}", model.Email);
                throw new InvalidOperationException("This email is already registered.");
            }

            var hashed = PasswordHelper.PasswordHash(model.Password);

            var entity = new Agent
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = hashed,
                Role = "Agent"
            };

            var result = await _agentRL.RegisterAgentAsync(entity);

            RabbitMQProducer.EnqueueUserRegistration(new UserRegistrationModel
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = result.Role
            });

            _logger.LogInformation("Agent registered & queued for email: {Email}", model.Email);

            return result;
        }

        public async Task<string> LoginAgentAsync(LoginModel model)
        {
            var agent = await _agentRL.GetAgentByEmailAsync(model.Email);
            if (agent == null)
            {
                _logger.LogWarning("Login attempt failed for email: {Email} - Agent not found", model.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (!PasswordHelper.VerifyPassword(model.Password, agent.Password))
            {
                _logger.LogWarning("Login attempt failed for email: {Email} - Invalid password", model.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var token = JwtHelper.GenerateToken(
                agent.AgentID,
                agent.Email,
                agent.FullName,
                agent.Role,
                _config);

            _logger.LogInformation("Agent logged in successfully: {Email}", model.Email);

            return token;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPassword model)
        {
            var agent = await _agentRL.GetAgentByEmailAsync(model.Email);
            if (agent == null)
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

        public async Task<bool> ResetPasswordAsync(ResetPasswordModel model)
        {
            var isValidOtp = await _otpService.ValidateOtpAsync(model.Email, model.Otp);
            if (!isValidOtp)
            {
                _logger.LogWarning("Invalid OTP provided for email: {Email}", model.Email);
                return false;
            }

            var agent = await _agentRL.GetAgentByEmailAsync(model.Email);
            if (agent == null)
            {
                _logger.LogWarning("Reset password attempt for non-existent email: {Email}", model.Email);
                return false;
            }

            var hashedPassword = PasswordHelper.PasswordHash(model.NewPassword);
            agent.Password = hashedPassword;

            await _agentRL.UpdateAgentAsync(agent);

            _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);

            return true;
        }
    }
}