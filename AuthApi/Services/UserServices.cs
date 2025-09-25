using AuthApi.Db.Entities;
using AuthApi.Services.Contracts;
using AuthApi.Services.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.Services
{
    public class UserServices : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        public UserServices()
        {
        }
        public UserServices(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<SrvResponse> UserRegestration(RegisterDto registerDto)
        {
            try
            {
                var user = new AppUser
                {
                    FullName = registerDto.FullName,
                    UserName = registerDto.UserName,
                    Email = registerDto.Email
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
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
                var Rx_Message = ex.Message;
                if (ex.InnerException != null) Rx_Message += " | " + ex.InnerException.Message;
                return new SrvResponse().Error(Rx_Message);
            }

        }

        public async Task<SrvResponse> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
                return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Invalid email or password");

            //await _userManager.RemovePasswordAsync(user);
            //await _userManager.AddPasswordAsync(user, "P@ssW0rd");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
                return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Invalid email or password");

            // جلب الـ Roles
            var roles = await _userManager.GetRolesAsync(user);

            // إنشاء التوكن
            var token = GenerateJwtToken(user, roles);
            return new SrvResponse().Success(token);
        }

        private string GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // إضافة الـ Roles كـ Claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
