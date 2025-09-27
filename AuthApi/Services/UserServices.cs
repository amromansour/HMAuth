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
        private  AuthApiDbContext _context;
        private IConfigurationSection _jwtSettings;

        public UserServices()
        {
        }
        public UserServices(UserManager<AppUser> userManager, IConfiguration configuration, AuthApiDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _jwtSettings = _configuration.GetSection("JwtSettings");
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
            //var jwtSettings = _configuration.GetSection("JwtSettings");
            var RefreshTokenDurationInMinutes = Convert.ToDouble(_jwtSettings["RefreshTokenDurationInMinutes"]);

            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
                return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Invalid email or password");


            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
                return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Invalid email or password");

            // Get User Roles
            var roles = await _userManager.GetRolesAsync(user);


            // Generate Token
            var token = GenerateJwtToken(user, roles);
            // Generate RefreshToken
            var refreshToken = GenerateRefreshToken(user.Id);


            #region save RefreshToken In Db
            RefreshToken refreshTokenItem = new RefreshToken
            {
                Token = refreshToken,
                ExpireIn = DateTime.UtcNow.AddMinutes(RefreshTokenDurationInMinutes),
                UserId = user.Id
            };
            _context.RefreshTokens.Add(refreshTokenItem);
            try
            {
                var _saveResult = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
            

            #endregion

            LoginResponse _LoginResponse = new LoginResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                UserName = user.UserName,
                ExpiresIn = _jwtSettings["DurationInMinutes"] != null ? Convert.ToInt32(_jwtSettings["DurationInMinutes"]) : 0
            };
            return new SrvResponse().Success(_LoginResponse);
        }
        public async Task<SrvResponse> ValidateRefreshToken(string token)
        {

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                #region Validaion
                // Check if the refresh token exists in the database
                var refreshToken = _context.RefreshTokens.SingleOrDefault(rt => rt.Token == token);
                if (refreshToken == null)
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Invalid refresh token");
                }
                // Check if the refresh token has expired
                if (refreshToken.ExpireIn < DateTime.UtcNow)
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Refresh token has expired");
                }

                #endregion

                // Get User Info
                var user = await _userManager.FindByIdAsync(refreshToken.UserId);
                // Check if user still exists
                // We can also check if the user is still active or not 
                if (user == null)
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "User is not exists");
                }

                // Get User Roles
                var roles = await _userManager.GetRolesAsync(user);



                // remove the old refresh token
                _context.RefreshTokens.Remove(refreshToken);

                // Token Generation
                var Newtoken = GenerateJwtToken(user, roles);
                // Refresh Token Generation
                var NewRefreshToken = GenerateRefreshToken(user.Id);

             

                var RefreshTokenDurationInMinutes = Convert.ToDouble(_jwtSettings["RefreshTokenDurationInMinutes"]);
                RefreshToken refreshTokenItem = new RefreshToken
                {
                    Token = NewRefreshToken,
                    ExpireIn = DateTime.UtcNow.AddMinutes(RefreshTokenDurationInMinutes),
                    UserId = user.Id
                };
                _context.RefreshTokens.Add(refreshTokenItem);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();



                LoginResponse _LoginResponse = new LoginResponse
                {
                    AccessToken = Newtoken,
                    RefreshToken = NewRefreshToken,
                    UserName = user.UserName,
                    ExpiresIn = _jwtSettings["DurationInMinutes"] != null ? Convert.ToInt32(_jwtSettings["DurationInMinutes"]) : 0
                };
                return new SrvResponse().Success(_LoginResponse);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var Rx_Message = ex.Message;
                if (ex.InnerException != null) Rx_Message += " | " + ex.InnerException.Message;
                return new SrvResponse().Error(Rx_Message);
            }
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
                var randomNumber = new byte[32];
                string token;
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                    token = Convert.ToBase64String(randomNumber);
                }
                return token;
            }
            catch (Exception ex)
            {
                return null;
            }

        }



    }
}
