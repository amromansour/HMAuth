using AuthApi.Configurations;
using AuthApi.Db;
using AuthApi.Db.Entities;
using AuthApi.Models;
using AuthApi.Services.Contracts;
using AuthApi.Services.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.Services
{
    public class UserServices : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AuthApiDbContext _context;
       

        public UserServices()
        {
        }
        public UserServices(UserManager<AppUser> userManager, IConfiguration configuration, AuthApiDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
           
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
            var jwtSettings = _configuration.GetSection("JwtSettings");
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
            var refreshToken = GenerateRefreshToken(user.Id);
            LoginResponse _LoginResponse = new LoginResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                UserName = user.UserName,
                ExpiresIn = jwtSettings["DurationInMinutes"] != null ? Convert.ToInt32(jwtSettings["DurationInMinutes"]) : 0
            };
            return new SrvResponse().Success(_LoginResponse);
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

        private string GenerateRefreshToken(string userId)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var RefreshTokenDurationInMinutes = Convert.ToDouble(jwtSettings["RefreshTokenDurationInMinutes"]);
                var randomNumber = new byte[32];
                string token;
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                    token = Convert.ToBase64String(randomNumber);
                }

                RefreshToken refreshToken = new RefreshToken
                {
                    Token = token,
                    ExpireIn = DateTime.UtcNow.AddMinutes(RefreshTokenDurationInMinutes),
                    UserId = userId
                };

                // حفظ الـ Refresh Token في قاعدة البيانات
                _context.RefreshTokens.Add(refreshToken);
                var _saveResult = _context.SaveChanges();
                if (_saveResult > 0)
                {
                    return token;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
           
        }

        private SrvResponse ValidateRefreshToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var refreshToken = _context.RefreshTokens.SingleOrDefault(rt => rt.Token == token);
                if (refreshToken == null)
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Invalid refresh token");
                }

                if (refreshToken.ExpireIn < DateTime.UtcNow)
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Refresh token has expired");
                }

                // Get User Info
                var user  = _userManager.FindByIdAsync(refreshToken.UserId).Result;
                // Check if user still exists
                if (user == null)
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "User is not exists");
                }



                _context.RefreshTokens.Remove(refreshToken);
                _context.SaveChanges();

                // جلب الـ Roles
                var roles =  _userManager.GetRolesAsync(user).Result;

                // إنشاء التوكن
                var Newtoken = GenerateJwtToken(user, roles);
                var NewRefreshToken = GenerateRefreshToken(user.Id);
                LoginResponse _LoginResponse = new LoginResponse
                {
                    AccessToken = Newtoken,
                    RefreshToken = NewRefreshToken,
                    UserName = user.UserName,
                    ExpiresIn = jwtSettings["DurationInMinutes"] != null ? Convert.ToInt32(jwtSettings["DurationInMinutes"]) : 0
                };
                return new SrvResponse().Success(_LoginResponse);

            }
            catch (Exception ex)
            {
                var Rx_Message = ex.Message;
                if (ex.InnerException != null) Rx_Message += " | " + ex.InnerException.Message;
                return new SrvResponse().Error(Rx_Message);
            }
        }

    }
}
