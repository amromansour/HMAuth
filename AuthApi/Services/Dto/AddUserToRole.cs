using System.ComponentModel.DataAnnotations;

namespace AuthApi.Services.Dto
{
    public class AddOrRemoveUserToRole
    {
        [Required(ErrorMessage = "Role Name is required")]
        public string roleName{ get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set;}
    }
}
