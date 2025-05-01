using RepoLayer.Entity;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface IEmployeeRL
    {
        Task<Employee> RegisterEmployeeAsync(Employee employeeEntity);
        Task<Employee> GetEmployeeByEmailAsync(string email);
        Task<bool> UpdateEmployeeAsync(Employee employeeEntity);
    }
}