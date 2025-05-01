using BusinessLogicLayer.Interfaces;
using ModelLayer.Models;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class PlanBL : IPlanBL
    {
        private readonly IPlanRL _planRL;

        public PlanBL(IPlanRL planRL)
        {
            _planRL = planRL;
        }

        public Task<InsurancePlan> CreatePlanAsync(PlanModel model)
        {
            return _planRL.CreatePlanAsync(model);
        }

        public Task<List<InsurancePlan>> GetAllPlansAsync()
        {
            return _planRL.GetAllPlansAsync();
        }
    }
}
