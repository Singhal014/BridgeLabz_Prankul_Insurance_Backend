using ModelLayer.Models;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPremiumBL
    {
        
        Task<decimal> CalculatePremiumAsync(PremiumModel model);
    }
}
