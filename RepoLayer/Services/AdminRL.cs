using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class AdminRL : IAdminRL
    {
        private readonly ApplicationDbContext _context;

        public AdminRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Admin> RegisterAdminAsync(Admin adminEntity)
        {
            await _context.Admins.AddAsync(adminEntity);
            await _context.SaveChangesAsync();
            return adminEntity;
        }

        public async Task<Admin> GetAdminByEmailAsync(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<bool> UpdateAdminAsync(Admin adminEntity)
        {
            _context.Admins.Update(adminEntity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}