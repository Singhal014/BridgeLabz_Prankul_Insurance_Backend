using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System.Threading.Tasks;

namespace RepoLayer.Services
{
    public class AgentRL : IAgentRL
    {
        private readonly ApplicationDbContext _context;

        public AgentRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Agent> RegisterAgentAsync(Agent agentEntity)
        {
            await _context.Agents.AddAsync(agentEntity);
            await _context.SaveChangesAsync();
            return agentEntity;
        }

        public async Task<Agent> GetAgentByEmailAsync(string email)
        {
            return await _context.Agents.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Agent> GetAgentByIdAsync(int agentId)  
        {
            return await _context.Agents.FirstOrDefaultAsync(a => a.AgentID == agentId);
        }

        public async Task<bool> UpdateAgentAsync(Agent agentEntity)
        {
            _context.Agents.Update(agentEntity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
