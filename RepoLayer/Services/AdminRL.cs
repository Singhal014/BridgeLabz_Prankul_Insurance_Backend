using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class AdminRL : IAdminRL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminRL> _logger;

        public AdminRL(ApplicationDbContext context, ILogger<AdminRL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Admin> RegisterAdminAsync(Admin adminEntity)
        {
            try
            {
                await _context.Admins.AddAsync(adminEntity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin registered: {Email}", adminEntity.Email);
                return adminEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAdminAsync failed");
                throw;
            }
        }

        public async Task<Admin> GetAdminByEmailAsync(string email)
        {
            try
            {
                return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdminByEmailAsync failed");
                throw;
            }
        }

        public async Task<bool> UpdateAdminAsync(Admin adminEntity)
        {
            try
            {
                _context.Admins.Update(adminEntity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAdminAsync failed");
                throw;
            }
        }
    }
}
