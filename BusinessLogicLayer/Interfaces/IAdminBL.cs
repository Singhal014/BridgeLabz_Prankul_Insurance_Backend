using RepoLayer.Entity;
using System.Threading.Tasks;
using ModelLayer.Models;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAdminBL
    {
        Task<Admin> RegisterAdminAsync(AdminModel model);
        Task<string> LoginAdminAsync(LoginModel model);
        Task<bool> ForgotPasswordAsync(ForgotPassword model);
        Task<bool> ResetPasswordAsync(ResetPasswordModel model);
    }
}
