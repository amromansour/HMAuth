using Microsoft.AspNetCore.Identity;

namespace AuthApi.Db.Entities
{
    public class AppUser: IdentityUser
    {
        public string FullName { get; set; }
        
    }
}
