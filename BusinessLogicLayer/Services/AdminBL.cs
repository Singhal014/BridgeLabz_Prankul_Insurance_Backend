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
    public class AdminBL : IAdminBL
    {
        private readonly IAdminRL _adminRL;
        private readonly ILogger<AdminBL> _logger;
        private readonly IConfiguration _config;
        private readonly IOtpServiceBL _otpService;

        public AdminBL(
            IAdminRL adminRL,
            ILogger<AdminBL> logger,
            IConfiguration config,
            IOtpServiceBL otpService)
        {
            _adminRL = adminRL;
            _logger = logger;
            _config = config;
            _otpService = otpService;
        }

        public async Task<Admin> RegisterAdminAsync(AdminModel model)
        {
            try
            {
                var existingAdmin = await _adminRL.GetAdminByEmailAsync(model.Email);
                if (existingAdmin != null)
                {
                    _logger.LogWarning("Registration attempt with already registered email: {Email}", model.Email);
                    throw new InvalidOperationException("This email is already registered.");
                }

                var hashed = PasswordHelper.PasswordHash(model.Password);

                var entity = new Admin
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = hashed,
                    Role = "Admin"
                };

                var result = await _adminRL.RegisterAdminAsync(entity);

                RabbitMQProducer.EnqueueUserRegistration(new UserRegistrationModel
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = result.Role
                });

                _logger.LogInformation("Admin registered & queued for email: {Email}", model.Email);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering admin with email: {Email}", model.Email);
                throw;
            }
        }

        public async Task<string> LoginAdminAsync(LoginModel model)
        {
            try
            {
                var admin = await _adminRL.GetAdminByEmailAsync(model.Email);
                if (admin == null)
                {
                    _logger.LogWarning("Login attempt failed for email: {Email} - Admin not found", model.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                if (!PasswordHelper.VerifyPassword(model.Password, admin.Password))
                {
                    _logger.LogWarning("Login attempt failed for email: {Email} - Invalid password", model.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                var token = JwtHelper.GenerateToken(
                    admin.AdminId,
                    admin.Email,
                    admin.FullName,
                    admin.Role,
                    _config);

                _logger.LogInformation("Admin logged in successfully: {Email}", model.Email);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", model.Email);
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPassword model)
        {
            try
            {
                var admin = await _adminRL.GetAdminByEmailAsync(model.Email);
                if (admin == null)
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
                _logger.LogError(ex, "Error occurred while processing forgot password for email: {Email}", model.Email);
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

                var admin = await _adminRL.GetAdminByEmailAsync(model.Email);
                if (admin == null)
                {
                    _logger.LogWarning("Reset password attempt for non-existent email: {Email}", model.Email);
                    return false;
                }

                var hashedPassword = PasswordHelper.PasswordHash(model.NewPassword);
                admin.Password = hashedPassword;

                await _adminRL.UpdateAdminAsync(admin);

                _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for email: {Email}", model.Email);
                throw;
            }
        }
    }
}
