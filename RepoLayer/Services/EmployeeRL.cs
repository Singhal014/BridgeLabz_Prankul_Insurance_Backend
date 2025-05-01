using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class EmployeeRL : IEmployeeRL
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> RegisterEmployeeAsync(Employee employeeEntity)
        {
            await _context.Employees.AddAsync(employeeEntity);
            await _context.SaveChangesAsync();
            return employeeEntity;
        }

        public async Task<Employee> GetEmployeeByEmailAsync(string email)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employeeEntity)
        {
            _context.Employees.Update(employeeEntity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}