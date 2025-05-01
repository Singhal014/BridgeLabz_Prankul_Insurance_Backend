using RepoLayer.Entity;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface ICommissionRL
    {
        Task<Commission> CalculateCommissionAsync(Commission commission);
    }
}
