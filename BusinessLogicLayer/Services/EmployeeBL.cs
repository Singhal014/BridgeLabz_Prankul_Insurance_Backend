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

            RabbitMQProducer.EnqueueUserRegistration(new UserRegistrationModel
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = result.Role
            });

            _logger.LogInformation("Employee registered & queued for email: {Email}", model.Email);

            return result;
        }

        public async Task<string> LoginEmployeeAsync(LoginModel model)
        {
            var employee = await _employeeRL.GetEmployeeByEmailAsync(model.Email);
            if (employee == null)
            {
                _logger.LogWarning("Login attempt failed for email: {Email} - Employee not found", model.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (!PasswordHelper.VerifyPassword(model.Password, employee.Password))
            {
                _logger.LogWarning("Login attempt failed for email: {Email} - Invalid password", model.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var token = JwtHelper.GenerateToken(
                employee.EmployeeID,
                employee.Email,
                employee.FullName,
                employee.Role,
                _config);

            _logger.LogInformation("Employee logged in successfully: {Email}", model.Email);

            return token;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPassword model)
        {
            var employee = await _employeeRL.GetEmployeeByEmailAsync(model.Email);
            if (employee == null)
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

            var employee = await _employeeRL.GetEmployeeByEmailAsync(model.Email);
            if (employee == null)
            {
                _logger.LogWarning("Reset password attempt for non-existent email: {Email}", model.Email);
                return false;
            }

            var hashedPassword = PasswordHelper.PasswordHash(model.NewPassword);
            employee.Password = hashedPassword;

            await _employeeRL.UpdateEmployeeAsync(employee);

            _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);

            return true;
        }
    }
}