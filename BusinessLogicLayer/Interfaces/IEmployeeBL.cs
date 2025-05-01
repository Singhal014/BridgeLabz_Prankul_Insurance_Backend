using ModelLayer.Models;
using RepoLayer.Entity;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IEmployeeBL
    {
        Task<Employee> RegisterEmployeeAsync(AdminModel model);
        Task<string> LoginEmployeeAsync(LoginModel model);
        Task<bool> ForgotPasswordAsync(ForgotPassword model);
        Task<bool> ResetPasswordAsync(ResetPasswordModel model);
    }
}