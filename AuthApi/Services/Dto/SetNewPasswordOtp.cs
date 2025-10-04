namespace AuthApi.Services.Dto
{
    public class SetNewPasswordOtp
    {
        public string resetToken { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
        public string Email { get; set; }
    }
}
