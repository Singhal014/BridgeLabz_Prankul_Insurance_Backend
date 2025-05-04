using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class CustomerRL : ICustomerRL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerRL> _logger;

        public CustomerRL(ApplicationDbContext context, ILogger<CustomerRL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Customer> RegisterCustomerAsync(Customer customerEntity)
        {
            try
            {
                await _context.Customers.AddAsync(customerEntity);
                await _context.SaveChangesAsync();
                return customerEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering customer with email: {Email}", customerEntity.Email);
                throw new Exception("An error occurred while registering the customer.", ex);
            }
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching customer with email: {Email}", email);
                throw new Exception("An error occurred while fetching the customer.", ex); // Propagate the exception
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customerEntity)
        {
            try
            {
                if (customerEntity == null)
                {
                    throw new ArgumentNullException(nameof(customerEntity), "Customer entity cannot be null.");
                }

                _context.Customers.Update(customerEntity);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID: {CustomerID}", customerEntity.CustomerID);
                throw new Exception("An error occurred while updating the customer.", ex); // Propagate the exception
            }
        }
    }
}
