using AuthApi.Models;
using AuthApi.Services.Contracts;
using AuthApi.Services.Dto;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        protected IUserService _UserServices;

        public UsersController(IUserService userServices)
        {
            _UserServices = userServices;
        }

        [HttpPost("UserRegister")]
        public async Task<IActionResult> UserRegister([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var RegUserResponse = await _UserServices.UserRegestration(model);
            if (RegUserResponse.IsOk)
            {
                return Ok(new ApiResponse() { _ResponseCode = Services.Enums.ResponseCode.OK, Message = "User registered successfully" });
            }
            else
            {
                return Ok(new ApiResponse() { _ResponseCode = RegUserResponse._ResponseCode, Message = RegUserResponse.Message });
            }
        }
    }
}
