using RepoLayer.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface ICommissionRL
    {
        Task<Commission> CalculateCommissionAsync(Commission commission);
        Task<List<Commission>> GetAllCommissionsByAgentIdAsync(int agentId);
        Task<int> PayPendingCommissionsAsync();

    }
}
