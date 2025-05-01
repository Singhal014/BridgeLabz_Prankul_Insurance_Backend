using RepoLayer.Entity;
using ModelLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface ISchemeBL
    {
        Task<Scheme> CreateSchemeAsync(SchemeModel model);
        Task<List<Scheme>> GetSchemesByPlanIdAsync(int planId);
        Task<Scheme> GetSchemeByIdAsync(int schemeId);

    }
}
