using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class EmployeeRL : IEmployeeRL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeRL> _logger;

        public EmployeeRL(ApplicationDbContext context, ILogger<EmployeeRL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Employee> RegisterEmployeeAsync(Employee employeeEntity)
        {
            try
            {
                _logger.LogInformation("Attempting to register employee with email: {Email}", employeeEntity.Email);

                await _context.Employees.AddAsync(employeeEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Employee registered successfully: {Email}", employeeEntity.Email);

                return employeeEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering employee with email: {Email}", employeeEntity.Email);
                throw;  
            }
        }

        public async Task<Employee> GetEmployeeByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Fetching employee with email: {Email}", email);

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);

                if (employee == null)
                {
                    _logger.LogWarning("Employee not found with email: {Email}", email);
                }

                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching employee with email: {Email}", email);
                throw; 
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employeeEntity)
        {
            try
            {
                _logger.LogInformation("Attempting to update employee with email: {Email}", employeeEntity.Email);

                _context.Employees.Update(employeeEntity);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Employee updated successfully with email: {Email}", employeeEntity.Email);
                }
                else
                {
                    _logger.LogWarning("No changes were made when updating employee with email: {Email}", employeeEntity.Email);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating employee with email: {Email}", employeeEntity.Email);
                throw;  
            }
        }
    }
}
