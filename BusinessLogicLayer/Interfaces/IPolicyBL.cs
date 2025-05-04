using ModelLayer.Models;
using RepoLayer.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPolicyBL
    {
        Task<Policy> CreatePolicyAsync(PolicyModel model, int userId);
        Task<List<Policy>> GetAllPoliciesAsync();
        Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId);
        Task<List<Policy>> GetPoliciesByStatusAsync(string status);
        Task<Policy> CancelPolicyAsync(int policyId, int userId);

    }
}