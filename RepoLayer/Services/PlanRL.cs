using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using ModelLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class PlanRL : IPlanRL
    {
        private readonly ApplicationDbContext _context;

        public PlanRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InsurancePlan> CreatePlanAsync(PlanModel model)
        {
            var plan = new InsurancePlan
            {
                PlanName = model.PlanName,
                Description = model.Description
            };

            _context.InsurancePlans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<List<InsurancePlan>> GetAllPlansAsync()
        {
            return await _context.InsurancePlans.ToListAsync();
        }
    }
}
