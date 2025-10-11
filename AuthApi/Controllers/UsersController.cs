using AuthApi.Models;
using AuthApi.Services.Contracts;
using AuthApi.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        protected IUserService _UserServices;
        public string UserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }

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


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var users = await _UserServices.GetAllUsers(PageIndex,PageSize);
            ApiResponse response = new ApiResponse() { 
            
                _ResponseCode = users._ResponseCode,
                Message = users.Message,
                Data = users.Data,
                AdditionalData = users.AdditionalData
            };
            return Ok(response);   
        }
        public async Task<IActionResult> GetUser()
        {
            var user = await _UserServices.GetUserData(UserId);
            ApiResponse response = new ApiResponse()
            {

                _ResponseCode = user._ResponseCode,
                Message = user.Message,
                Data = user.Data,
                AdditionalData = user.AdditionalData
            };
            return Ok(response);
        }
    }
}
