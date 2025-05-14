using ModelLayer.Models;
using ModelLayer.ViewModels;
using RepoLayer.Entity;
using System.Threading.Tasks;

namespace RepoLayer.Interfaces
{
    public interface IPaymentRL
    {
        Task<Payment> ProcessPaymentAsync(Payment payment);
        Task<List<PaymentViewModel>> GetCustomerPaymentHistoryAsync(long customerId);
        Task<ReceiptViewModel> GenerateReceiptAsync(long paymentId);


    }
}
