using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class CustomerRL : ICustomerRL
    {
        private readonly ApplicationDbContext _context;

        public CustomerRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> RegisterCustomerAsync(Customer customerEntity)
        {
            await _context.Customers.AddAsync(customerEntity);
            await _context.SaveChangesAsync();
            return customerEntity;
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> UpdateCustomerAsync(Customer customerEntity)
        {
            _context.Customers.Update(customerEntity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}