namespace MyPetClinic.Application.DTOs
{
    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class ActivateAccountRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Token kích hoạt là bắt buộc")]
        public string Token { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [System.ComponentModel.DataAnnotations.MinLength(8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ClaimProfileDto
    {
        public string Email { get; set; } = string.Empty;
        public string TempToken { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string? PetName { get; set; }
    }

    public class SkipClaimDto
    {
        public string Email { get; set; } = string.Empty;
        public string TempToken { get; set; } = string.Empty;
    }

    /// <summary>Request DTO để yêu cầu gửi lại mã OTP.</summary>
    public class ResendOtpRequestDto
    {
        public required string Email { get; set; }
        public required string Type { get; set; }
    }

    /// <summary>Request DTO để xác thực mã OTP.</summary>
    public class VerifyOtpRequestDto
    {
        public required string Email { get; set; }
        public required string OtpCode { get; set; }
    }

    /// <summary>Request DTO để yêu cầu đặt lại mật khẩu (gửi OTP).</summary>
    public class ForgotPasswordRequestDto
    {
        public required string Email { get; set; }
    }

    /// <summary>Request DTO để đặt lại mật khẩu bằng mã OTP.</summary>
    public class ResetPasswordRequestDto
    {
        public required string Email { get; set; }
        public required string OtpCode { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
