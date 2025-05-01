using RepoLayer.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface IPolicyRL
    {
        Task<Policy> CreatePolicyAsync(Policy policyEntity);
        Task<List<Policy>> GetAllPoliciesAsync();
        Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId); 
        Task<List<Policy>> GetPoliciesByStatusAsync(string status);
    }
}