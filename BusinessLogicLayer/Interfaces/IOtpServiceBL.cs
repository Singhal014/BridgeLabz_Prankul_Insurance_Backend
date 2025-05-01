using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public interface IOtpServiceBL
    {
        Task<string> GenerateOtpAsync();
        Task StoreOtpAsync(string email, string otp);
        Task<bool> ValidateOtpAsync(string email, string otp);
    }
}
