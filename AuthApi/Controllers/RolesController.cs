using AuthApi.Db.Entities;
using AuthApi.Models;
using AuthApi.Services;
using AuthApi.Services.Contracts;
using AuthApi.Services.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AuthApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
       IRoleServices _roleServiecs;
        public RolesController(IRoleServices roleServiecs) => _roleServiecs = roleServiecs;

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    return Ok(new ApiResponse()
                    {
                        _ResponseCode = Services.Enums.ResponseCode.BadRequest,
                        Message = "Role name cannot be empty"
                    });
                }
                var RoleCreateResponse = await _roleServiecs.CreateRole(roleName);
                if (RoleCreateResponse.IsOk)
                {
                    return Ok(new ApiResponse() { Message = "Role created successfully" });
                }
                else
                {
                    return Ok(new ApiResponse (){ _ResponseCode = RoleCreateResponse._ResponseCode, Message = RoleCreateResponse.Message});
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse() { Message = ex.Message, _ResponseCode = Services.Enums.ResponseCode.InternalServerError });
            }
        }


        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var rolesResponse = await _roleServiecs.GetAllRoles();
                if (rolesResponse.IsOk)
                {
                    return Ok(new ApiResponse() { Data = rolesResponse});
                }
                else
                {
                    return Ok(new ApiResponse() { _ResponseCode = rolesResponse._ResponseCode, Message = rolesResponse.Message });
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse() { Message = ex.Message, _ResponseCode = Services.Enums.ResponseCode.InternalServerError });
            }
        }


        [HttpPost("AddUserToRole")]
        public async Task<IActionResult> AddUserToRole([FromBody] AddOrRemoveUserToRole dtoModel)
        {
            try
            {
                var rolesResponse = await _roleServiecs.AssignRoleToUser(dtoModel);
                if (rolesResponse.IsOk)
                {
                    return Ok(new ApiResponse() );
                }
                else
                {
                    return Ok(new ApiResponse() { _ResponseCode = rolesResponse._ResponseCode, Message = rolesResponse.Message });
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse() { Message = ex.Message, _ResponseCode = Services.Enums.ResponseCode.InternalServerError });
            }
        }

        [HttpPost("RemoveUserToRole")]
        public async Task<IActionResult> RemoveUserToRole([FromBody] AddOrRemoveUserToRole dtoModel)
        {
            try
            {
                var rolesResponse = await _roleServiecs.RemoveRoleFromUser(dtoModel);
                if (rolesResponse.IsOk)
                {
                    return Ok(new ApiResponse());
                }
                else
                {
                    return Ok(new ApiResponse() { _ResponseCode = rolesResponse._ResponseCode, Message = rolesResponse.Message });
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse() { Message = ex.Message, _ResponseCode = Services.Enums.ResponseCode.InternalServerError });
            }
        }


    }
}
