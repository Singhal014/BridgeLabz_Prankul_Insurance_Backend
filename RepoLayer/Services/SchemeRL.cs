using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using ModelLayer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class SchemeRL : ISchemeRL
    {
        private readonly ApplicationDbContext _context;

        public SchemeRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Scheme> CreateSchemeAsync(SchemeModel model)
        {
            var scheme = new Scheme
            {
                PlanId = model.PlanId,
                SchemeName = model.SchemeName,
                Details = model.Details
            };

            _context.Schemes.Add(scheme);
            await _context.SaveChangesAsync();

            return await _context.Schemes
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SchemeId == scheme.SchemeId);
        }


        public async Task<List<Scheme>> GetSchemesByPlanIdAsync(int planId)
        {
            return await _context.Schemes
                .Include(s => s.Plan)                 
                .Where(s => s.PlanId == planId)
                .ToListAsync();
        }
        public async Task<Scheme> GetSchemeByIdAsync(int schemeId)
        {
            return await _context.Schemes
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SchemeId == schemeId);
        }

    }
}
