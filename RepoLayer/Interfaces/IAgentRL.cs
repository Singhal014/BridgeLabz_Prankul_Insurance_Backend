using RepoLayer.Entity;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface IAgentRL
    {
        Task<Agent> RegisterAgentAsync(Agent agentEntity);
        Task<Agent> GetAgentByEmailAsync(string email);
        Task<Agent> GetAgentByIdAsync(int agentId); 
        Task<bool> UpdateAgentAsync(Agent agentEntity);
    }
}
