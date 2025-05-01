using ModelLayer.Models;
using RepoLayer.Entity;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface ICustomerBL
    {
        Task<Customer> RegisterCustomerAsync(CustomerRegistration model);
        Task<string> LoginCustomerAsync(LoginModel model);
        Task<bool> ForgotPasswordAsync(ForgotPassword model);
        Task<bool> ResetPasswordAsync(ResetPasswordModel model);
    }
}