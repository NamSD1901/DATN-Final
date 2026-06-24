using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto model);
        Task<AuthResult> ResendOtpAsync(string email, string type);
        Task<AuthResult> VerifyOtpAsync(string email, string otpCode);
        Task<AuthResult> LoginAsync(LoginDto model);
        Task<AuthResult> ForgotPasswordAsync(string email);
        Task<AuthResult> ResetPasswordAsync(string email, string otpCode, string newPassword, string confirmPassword);
        Task<AuthResult> ActivateAccountAsync(ActivateAccountRequest request);
        
        // 2FA Profile Claiming
        Task<AuthResult> ClaimProfileAsync(ClaimProfileDto request);
        Task<AuthResult> SkipClaimingAsync(SkipClaimDto request);
    }
}
