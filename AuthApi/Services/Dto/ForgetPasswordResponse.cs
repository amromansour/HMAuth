namespace AuthApi.Services.Dto
{
    public class ForgetPasswordResponse
    {
        public string resetToken { get; set; }
        public string OTP { get; set; }
    }
}
