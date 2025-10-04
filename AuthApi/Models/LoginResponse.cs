namespace AuthApi.Models
{
    public class LoginResponse
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresInMin { get; set; }

    }
}
