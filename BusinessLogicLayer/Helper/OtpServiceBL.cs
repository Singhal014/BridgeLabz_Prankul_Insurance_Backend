using BusinessLogicLayer.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Helper
{
    public class OtpServiceBL : IOtpServiceBL
    {
        private static readonly ConcurrentDictionary<string, (string otp, DateTime expiry)> _otpStorage = new();
        private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(10);

        public async Task<string> GenerateOtpAsync()
        {
            return await Task.Run(() =>
            {
                Random random = new Random();
                return random.Next(100000, 999999).ToString();
            });
        }

        public async Task StoreOtpAsync(string email, string otp)
        {
            await Task.Run(() =>
            {
                _otpStorage[email] = (otp, DateTime.Now.Add(_otpExpiry));
            });
        }

        public async Task<bool> ValidateOtpAsync(string email, string otp)
        {
            return await Task.Run(() =>
            {
                if (!_otpStorage.TryGetValue(email, out var otpData))
                    return false;

                if (DateTime.Now > otpData.expiry)
                {
                    _otpStorage.TryRemove(email, out _);
                    return false;
                }

                return otpData.otp == otp;
            });
        }
    }
}