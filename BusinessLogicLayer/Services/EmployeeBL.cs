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
    public class EmployeeBL : IEmployeeBL
    {
        private readonly IEmployeeRL _employeeRL;
        private readonly ILogger<EmployeeBL> _logger;
        private readonly IConfiguration _config;
        private readonly IOtpServiceBL _otpService;

        public EmployeeBL(
            IEmployeeRL employeeRL,
            ILogger<EmployeeBL> logger,
            IConfiguration config,
            IOtpServiceBL otpService)
        {
            _employeeRL = employeeRL;
            _logger = logger;
            _config = config;
            _otpService = otpService;
        }

        public async Task<Employee> RegisterEmployeeAsync(AdminModel model)
        {
            try
            {
                var existingEmployee = await _employeeRL.GetEmployeeByEmailAsync(model.Email);
                if (existingEmployee != null)
                {
                    _logger.LogWarning("Registration attempt with already registered email: {Email}", model.Email);
                    throw new InvalidOperationException("This email is already registered.");
                }

                var hashed = PasswordHelper.PasswordHash(model.Password);

                var entity = new Employee
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = hashed,
                    Role = "Employee"
                };

                var result = await _employeeRL.RegisterEmployeeAsync(entity);

                
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
                    _logger.LogError(ex, "Failed to enqueue user registration for email: {Email}", model.Email);
                    throw;  
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering employee with email: {Email}", model.Email);
                throw;  
            }
        }

        public async Task<string> LoginEmployeeAsync(LoginModel model)
        {
            try
            {
                var employee = await _employeeRL.GetEmployeeByEmailAsync(model.Email);
                if (employee == null || !PasswordHelper.VerifyPassword(model.Password, employee.Password))
                {
                    _logger.LogWarning("Login attempt failed for email: {Email}", model.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                var token = JwtHelper.GenerateToken(
                    employee.EmployeeID,
                    employee.Email,
                    employee.FullName,
                    employee.Role,
                    _config);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login attempt for email: {Email}", model.Email);
                throw; 
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPassword model)
        {
            try
            {
                var employee = await _employeeRL.GetEmployeeByEmailAsync(model.Email);
                if (employee == null)
                {
                    _logger.LogWarning("Forgot password attempt for non-existent email: {Email}", model.Email);
                    return false;
                }

                var otp = await _otpService.GenerateOtpAsync();
                await _otpService.StoreOtpAsync(model.Email, otp);

                
                try
                {
                    RabbitMQProducer.EnqueueEmail(new EmailModel
                    {
                        ToEmail = model.Email,
                        Subject = "Password Reset OTP",
                        Body = $"Your OTP for password reset is: {otp}. It will expire in 10 minutes."
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enqueue OTP email for employee: {Email}", model.Email);
                    throw;  
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password request for email: {Email}", model.Email);
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

                var employee = await _employeeRL.GetEmployeeByEmailAsync(model.Email);
                if (employee == null)
                {
                    _logger.LogWarning("Reset password attempt for non-existent email: {Email}", model.Email);
                    return false;
                }

                var hashedPassword = PasswordHelper.PasswordHash(model.NewPassword);
                employee.Password = hashedPassword;

                await _employeeRL.UpdateEmployeeAsync(employee);

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
