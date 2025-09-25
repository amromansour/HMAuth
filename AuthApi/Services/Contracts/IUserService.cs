using AuthApi.Services.Dto;

namespace AuthApi.Services.Contracts
{
    public interface IUserService
    {
        Task<SrvResponse> UserRegestration(RegisterDto registerDto);
        Task<SrvResponse> Login(LoginDto loginDto);
    }
}
