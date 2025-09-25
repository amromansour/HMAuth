using AuthApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TestAuthController : ControllerBase
    {
        //All Users Admin and Users
        [Authorize] 
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return Ok(new ApiResponse() { Data = "This is private, only authenticated users can see it." });
        }

        [Authorize(Roles ="User")]
        [HttpGet("privateUser")]
        public IActionResult PrivateUserEndpoint()
        {
            return Ok(new ApiResponse() { Data = "This is private, only authenticated users with User Role can see it." });
            
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("privateAdmin")]
        public IActionResult PrivateAdminEndpoint()
        {
            return Ok(new ApiResponse() { Data = "This is private, only authenticated users With Admin can see it." });

           
        }
    }
}
