using Microsoft.EntityFrameworkCore;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
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
            try
            {
                await _context.Agents.AddAsync(agentEntity);
                await _context.SaveChangesAsync();
                return agentEntity;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while registering the agent.", ex);
            }
        }

        public async Task<Agent> GetAgentByEmailAsync(string email)
        {
            try
            {
                return await _context.Agents.FirstOrDefaultAsync(a => a.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving agent by email: {email}", ex);
            }
        }

        public async Task<Agent> GetAgentByIdAsync(int agentId)
        {
            try
            {
                return await _context.Agents.FirstOrDefaultAsync(a => a.AgentID == agentId);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving agent by ID: {agentId}", ex);
            }
        }

        public async Task<bool> UpdateAgentAsync(Agent agentEntity)
        {
            try
            {
                _context.Agents.Update(agentEntity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the agent.", ex);
            }
        }
        public async Task<List<Agent>> GetAllAgentsAsync()
        {
            try
            {
                return await _context.Agents.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all agents.", ex);
            }
        }


    }
}
