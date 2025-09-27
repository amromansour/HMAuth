using AuthApi.Models;
using AuthApi.Services.Contracts;
using AuthApi.Services.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public IUserService _userService;
        public AuthController(IUserService userService) {
        
            this._userService = userService;
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var response = await _userService.Login(model);
            if (response.IsOk)
            {
                return Ok(new ApiResponse() { Data = response.Data, Message = "Login successful" });
            }
            else
            {
                return Ok(new ApiResponse() { _ResponseCode = response._ResponseCode, Message = response.Message });
            }
        }


        [HttpPost("RefreshAuth")]
        public async Task<IActionResult> RefreshAuth([FromBody] RefreshTokenDto model)
        {
            if (string.IsNullOrEmpty(model.Token))
                return BadRequest(new ApiResponse() { _ResponseCode = Services.Enums.ResponseCode.BadRequest, Message = "Invalid token" });


            var response = await _userService.ValidateRefreshToken(model.Token);
            if (response.IsOk)
            {
                return Ok(new ApiResponse() { Data = response.Data, Message = "Token is valid" });
            }
            else { return BadRequest(new ApiResponse() { _ResponseCode = response._ResponseCode, Message = response.Message }); 
            }
        }
    }
}
