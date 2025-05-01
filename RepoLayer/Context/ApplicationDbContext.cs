using Microsoft.EntityFrameworkCore;
using RepoLayer.Entity;
using RepoLayer.Services;

namespace RepoLayer.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<InsurancePlan> InsurancePlans { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<Scheme> Schemes { get; set; }


    }
}
