using AuthApi.Services.Dto;

namespace AuthApi.Services.Contracts
{
    public interface IUserService
    {
        Task<SrvResponse> UserRegestration(RegisterDto registerDto);
        Task<SrvResponse> Login(LoginDto loginDto);
        Task<SrvResponse> ValidateRefreshToken(string token);

        Task<SrvResponse> ForgetPasswordReq(ForgetPasswordReqDto forgetPasswordReqDto);
        Task<SrvResponse> SetNewPassword(SetNewPasswordOtp setNewPasswordOtp);
    }
}
