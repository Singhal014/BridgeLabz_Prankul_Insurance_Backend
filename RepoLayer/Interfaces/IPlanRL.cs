using RepoLayer.Entity;
using ModelLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface IPlanRL
    {
        Task<InsurancePlan> CreatePlanAsync(PlanModel model);
        Task<List<InsurancePlan>> GetAllPlansAsync();
    }
}
