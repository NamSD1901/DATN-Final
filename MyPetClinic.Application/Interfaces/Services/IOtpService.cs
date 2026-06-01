namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IOtpService
    {
        string GenerateOtp(string key, int expirationMinutes = 5);
        bool ValidateOtp(string key, string otpCode);
    }
}
