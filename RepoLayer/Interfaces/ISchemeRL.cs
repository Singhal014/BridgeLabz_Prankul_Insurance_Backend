using RepoLayer.Entity;
using ModelLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface ISchemeRL
    {
        Task<Scheme> CreateSchemeAsync(SchemeModel model);
        Task<List<Scheme>> GetAllSchemesAsync();
    }
}
