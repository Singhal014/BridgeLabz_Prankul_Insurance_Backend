using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using RepoLayer.Entity;
using System;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeBL _employeeBL;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeBL employeeBL, ILogger<EmployeeController> logger)
        {
            _employeeBL = employeeBL;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] AdminModel model)
        {
            try
            {
                var employee = await _employeeBL.RegisterEmployeeAsync(model);
                var response = new ResponseModel<Employee>
                {
                    Success = true,
                    Message = $"Hello {employee.FullName}, you have been successfully registered as an employee.",
                    Data = employee
                };
                _logger.LogInformation("Employee registered successfully: {Email}", model.Email);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message == "This email is already registered.")
            {
                _logger.LogWarning("Registration failed for email: {Email} - Email already registered", model.Email);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = "This email is already registered.",
                    Data = ex.Message
                };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", model.Email);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = "Registration failed.",
                    Data = ex.Message
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var token = await _employeeBL.LoginEmployeeAsync(model);
                var response = new ResponseModel<string>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = token
                };
                _logger.LogInformation("Employee logged in successfully: {Email}", model.Email);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed for email: {Email}", model.Email);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = "Invalid credentials",
                    Data = ex.Message
                };
                return Unauthorized(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for email: {Email}", model.Email);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = "Login failed",
                    Data = ex.Message
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword model)
        {
            try
            {
                var result = await _employeeBL.ForgotPasswordAsync(model);
                if (result)
                {
                    var response = new ResponseModel<string>
                    {
                        Success = true,
                        Message = "OTP sent successfully to your email",
                        Data = null
                    };
                    _logger.LogInformation("OTP sent successfully for email: {Email}", model.Email);
                    return Ok(response);
                }
                else
                {
                    var response = new ResponseModel<string>
                    {
                        Success = false,
                        Message = "Email not found",
                        Data = null
                    };
                    _logger.LogWarning("Forgot password attempt for non-existent email: {Email}", model.Email);
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed for email: {Email}", model.Email);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = "Failed to process forgot password request",
                    Data = ex.Message
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            try
            {
                var result = await _employeeBL.ResetPasswordAsync(model);
                if (result)
                {
                    var response = new ResponseModel<string>
                    {
                        Success = true,
                        Message = "Password reset successfully",
                        Data = null
                    };
                    _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);
                    return Ok(response);
                }
                else
                {
                    var response = new ResponseModel<string>
                    {
                        Success = false,
                        Message = "Invalid OTP or email",
                        Data = null
                    };
                    _logger.LogWarning("Invalid OTP or email for password reset: {Email}", model.Email);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password failed for email: {Email}", model.Email);
                var errorResponse = new ResponseModel<string>
                {
                    Success = false,
                    Message = "Failed to reset password",
                    Data = ex.Message
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}