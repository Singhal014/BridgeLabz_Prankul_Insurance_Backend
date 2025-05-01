using RepoLayer.Entity;
using ModelLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPlanBL
    {
        Task<InsurancePlan> CreatePlanAsync(PlanModel model);
        Task<List<InsurancePlan>> GetAllPlansAsync();
    }
}
