using AuthApi.Db.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AuthApi.Db
{
    public class AuthApiDbContext: IdentityDbContext<AppUser, AppRole, string,
                      IdentityUserClaim<string>,
                      IdentityUserRole<string>,
                      IdentityUserLogin<string>,
                      IdentityRoleClaim<string>,
                      IdentityUserToken<string>>
    {
        public AuthApiDbContext(DbContextOptions<AuthApiDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //this To Change the Default Table Names
            builder.Entity<AppUser>(b =>
            {
                b.ToTable("Users");
            });

            builder.Entity<AppRole>(b =>
            {
                b.ToTable("Roles");
            });

            builder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("RoleClaims");
            });

            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("UserTokens");
            });
        }

    }
    
}
