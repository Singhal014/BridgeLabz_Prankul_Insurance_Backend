using BusinessLogicLayer.Interfaces;
using ModelLayer.Models;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class SchemeBL : ISchemeBL
    {
        private readonly ISchemeRL _schemeRL;

        public SchemeBL(ISchemeRL schemeRL)
        {
            _schemeRL = schemeRL;
        }

        public Task<Scheme> CreateSchemeAsync(SchemeModel model)
        {
            return _schemeRL.CreateSchemeAsync(model);
        }

        public Task<List<Scheme>> GetSchemesByPlanIdAsync(int planId)
        {
            return _schemeRL.GetSchemesByPlanIdAsync(planId);
        }
        public Task<Scheme> GetSchemeByIdAsync(int schemeId)
        {
            return _schemeRL.GetSchemeByIdAsync(schemeId);
        }

    }
}
