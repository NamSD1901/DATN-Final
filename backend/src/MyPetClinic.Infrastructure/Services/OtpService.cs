using Microsoft.Extensions.Caching.Memory;
using MyPetClinic.Application.Interfaces.Services;
using System;

namespace MyPetClinic.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GenerateOtp(string key, int expirationMinutes = 5)
        {
            // Tạo mã OTP 6 số ngẫu nhiên
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            // Lưu vào cache
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationMinutes));

            _cache.Set(key, otp, cacheOptions);

            return otp;
        }

        public bool ValidateOtp(string key, string otpCode)
        {
            if (_cache.TryGetValue(key, out string? cachedOtp))
            {
                if (cachedOtp == otpCode)
                {
                    // Xoá OTP sau khi dùng thành công
                    _cache.Remove(key);
                    return true;
                }
            }
            return false;
        }
    }
}
