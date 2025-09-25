using AuthApi.Db.Entities;
using AuthApi.Models;
using AuthApi.Services.Contracts;
using AuthApi.Services.Dto;
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AuthApi.Services
{
    public class RoleServices : IRoleServices
    {
        RoleManager<AppRole> _roleManager;
        UserManager<AppUser> _userManager;
        public RoleServices(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }


     
        public async Task<SrvResponse> CreateRole(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    var errors = "Roles Can not By empty";
                    return new SrvResponse().Error(errors);
                }

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (roleExists)
                {
                    return new SrvResponse()
                    {
                        _ResponseCode = Services.Enums.ResponseCode.Conflict,
                        Message = "Role already exists"
                    };
                }


                var result = await _roleManager.CreateAsync(new AppRole() { Name = roleName});
                if (result.Succeeded)
                {
                    return new SrvResponse().Success();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new SrvResponse().Error(errors);
                }
            }
            catch (Exception ex)
            {
                return ex.GetSrvResponse();
            }

        }

        public async Task<SrvResponse> DeleteRole(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    var errors = "Role Name Can not By empty";
                    return new SrvResponse().Error(errors);
                }
                var Role = await _roleManager.FindByNameAsync(roleName);
                var result = await _roleManager.DeleteAsync(Role);
                if (result.Succeeded)
                {
                    return new SrvResponse().Success();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new SrvResponse().Error(errors);
                }
            }
            catch (Exception ex)
            {
                return ex.GetSrvResponse();
            }
        }

        public async Task<SrvResponse> GetUserRoles(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    var errors = "Username Can not By empty";
                    return new SrvResponse().Error(errors);
                }
                var user = await _userManager.FindByNameAsync(userName);
                IList<string> roles = await _userManager.GetRolesAsync(user);
                return new SrvResponse().Success(roles);
            }
            catch (Exception ex)
            {
                return ex.GetSrvResponse();
            }
        }


        public async Task<SrvResponse> AssignRoleToUser(AddOrRemoveUserToRole model)
        {
            try
            {
                //validation
                //if (string.IsNullOrEmpty(userName))
                //{
                //    var errors = "Username Can not By empty";
                //    return new SrvResponse().Error(errors);
                //}
                //if (string.IsNullOrEmpty(roleName))
                //{
                //    var errors = "Role Name Can not By empty";
                //    return new SrvResponse().Error(errors);
                //}

                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user==null)
                {
                    return new SrvResponse()
                    {
                        _ResponseCode = Services.Enums.ResponseCode.NotFound,
                        Message = "User Not Found"
                    };
                }
                var Role = await _roleManager.FindByNameAsync(model.roleName);
                if (Role == null)
                {
                    return new SrvResponse()
                    {
                        _ResponseCode = Services.Enums.ResponseCode.NotFound,
                        Message = "Role Not Found"
                    };
                }

                var result = await _userManager.AddToRoleAsync(user, model.roleName);
                if (result.Succeeded)
                {
                    return new SrvResponse().Success();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new SrvResponse().Error(errors);
                }
            }
            catch (Exception ex)
            {
                return ex.GetSrvResponse();
            }


        }


        public async Task<SrvResponse> RemoveRoleFromUser(AddOrRemoveUserToRole model)
        {
            try
            {
                //validation
                //if (string.IsNullOrEmpty(userName))
                //{
                //    var errors = "Username Can not By empty";
                //    return new SrvResponse().Error(errors);
                //}
                //if (string.IsNullOrEmpty(roleName))
                //{
                //    var errors = "Role Name Can not By empty";
                //    return new SrvResponse().Error(errors);
                //}
                var user = await _userManager.FindByNameAsync(model.UserName);
                var result = await _userManager.RemoveFromRoleAsync(user, model.roleName);
                if (result.Succeeded)
                {
                    return new SrvResponse().Success();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new SrvResponse().Error(errors);
                }
            }
            catch (Exception ex)
            {
                return ex.GetSrvResponse();
            }
        }

        public async Task<SrvResponse> GetAllRoles()
        {
            try
            {
                var roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return new SrvResponse().Success(roles);
            }
            catch (Exception ex)
            {
                return ex.GetSrvResponse();
            }
        }
    }
}
