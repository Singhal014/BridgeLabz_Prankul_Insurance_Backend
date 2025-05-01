using ModelLayer.Models;
using RepoLayer.Entity;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPaymentBL
    {
        Task<Payment> ProcessPaymentAsync(PaymentModel model);
        Task<List<PaymentViewModel>> GetCustomerPaymentHistoryAsync(int customerId);

    }
}
