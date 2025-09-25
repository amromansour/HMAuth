using AuthApi.Services.Dto;

namespace AuthApi.Services.Contracts
{
    public interface IRoleServices
    {
        Task<SrvResponse> CreateRole(string roleName);
        Task<SrvResponse> DeleteRole(string roleName);
        Task<SrvResponse> AssignRoleToUser(AddOrRemoveUserToRole _AddOrRemoveUserToRole);
        Task<SrvResponse> RemoveRoleFromUser(AddOrRemoveUserToRole _AddOrRemoveUserToRole);
        Task<SrvResponse> GetUserRoles(string userName);
        Task<SrvResponse> GetAllRoles();
    }
}
