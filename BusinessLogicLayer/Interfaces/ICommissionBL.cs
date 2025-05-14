using ModelLayer.Models;
using RepoLayer.Entity;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface ICommissionBL
    {
        Task<Commission> CalculateCommissionAsync(CommissionModel model);
        Task<(List<Commission> Commissions, decimal TotalCommission)> GetAllCommissionsByAgentIdAsync(int agentId);
        Task<string> PayPendingCommissionsAsync();

    }
}
