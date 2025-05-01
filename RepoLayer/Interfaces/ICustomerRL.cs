using RepoLayer.Entity;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface ICustomerRL
    {
        Task<Customer> RegisterCustomerAsync(Customer customerEntity);
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<bool> UpdateCustomerAsync(Customer customerEntity);
    }
}