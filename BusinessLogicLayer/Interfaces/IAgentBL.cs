using System.Threading.Tasks;
using RepoLayer.Entity;
using ModelLayer.Models;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAgentBL
    {
        Task<Agent> RegisterAgentAsync(AdminModel model);
        Task<string> LoginAgentAsync(LoginModel model);
        Task<bool> ForgotPasswordAsync(ForgotPassword model);
        Task<bool> ResetPasswordAsync(ResetPasswordModel model);
    }
}
