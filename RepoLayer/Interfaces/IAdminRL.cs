using RepoLayer.Entity;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface IAdminRL
    {
        Task<Admin> RegisterAdminAsync(Admin adminEntity);
        Task<Admin> GetAdminByEmailAsync(string email);
        Task<bool> UpdateAdminAsync(Admin adminEntity);
    }
}
