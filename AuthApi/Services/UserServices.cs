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
using System.Security.Cryptography;
using System.Text;

namespace AuthApi.Services
{
    public class UserServices : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private AuthApiDbContext _context;
        private IConfigurationSection _jwtSettings;
        private readonly IServiceProvider _serviceProvider;

        public UserServices()
        {
        }
        public UserServices(UserManager<AppUser> userManager, IConfiguration configuration, AuthApiDbContext context, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _jwtSettings = _configuration.GetSection("JwtSettings");
            _serviceProvider = serviceProvider;
        }

        public async Task<SrvResponse> GetUserData(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
                return new SrvResponse().Error(Enums.ResponseCode.NotFound, "User Not Found");
            var roles = await _userManager.GetRolesAsync(user);
            UserDto userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            };
            return new SrvResponse().Success(userDto);
        }
        public async Task<SrvResponse> GetAllUsers(int pageIndex, int pageSize)
        {

            var usersQuery = _userManager.Users
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var users = new List<UserDto>();

            foreach (var u in usersQuery)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                    var roles = await userManager.GetRolesAsync(u);
                    users.Add(new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        UserName = u.UserName,
                        Email = u.Email,
                        Roles = roles
                    });
                }
            }

            return new SrvResponse().Success(users, _context.Users.Count());
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
                ExpiresInMin = _jwtSettings["DurationInMinutes"] != null ? Convert.ToInt32(_jwtSettings["DurationInMinutes"]) : 0
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
                    ExpiresInMin = _jwtSettings["DurationInMinutes"] != null ? Convert.ToInt32(_jwtSettings["DurationInMinutes"]) : 0
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

        public async Task<SrvResponse> ForgetPasswordReq(ForgetPasswordReqDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return new SrvResponse().Error(Enums.ResponseCode.NotFound, "No user associated with this email");

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Generate OTP
                var otpResponse = await GenerateOtPForUser(user.Id);
                if (!otpResponse.IsOk)
                {
                    return new SrvResponse().Error(otpResponse._ResponseCode, otpResponse.Message);
                }

                // Here we can send the OTP to the user's email using your preferred email service.
                // SendEmail(user.Email, "Your OTP Code", $"Your OTP code is: {otpResponse.Data}");

                // For demonstration purposes, we'll just return the OTP in the response.

                ForgetPasswordResponse forgetPasswordResponse = new ForgetPasswordResponse
                {
                    OTP = otpResponse.Data as string,
                    resetToken = resetToken
                };
                return new SrvResponse().Success(forgetPasswordResponse);
            }
            catch (Exception ex)
            {
                var Rx_Message = ex.Message;
                if (ex.InnerException != null) Rx_Message += " | " + ex.InnerException.Message;
                return new SrvResponse().Error(Rx_Message);
            }
        }

        public async Task<SrvResponse> SetNewPassword(SetNewPasswordOtp model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return new SrvResponse().Error(Enums.ResponseCode.NotFound, "No user associated with this email");

                // Validate OTP
                var otpValidationResponse = await ValidateOtpForUser(user.Id, model.Otp);
                if (!otpValidationResponse.IsOk)
                {
                    return new SrvResponse().Error(otpValidationResponse._ResponseCode, otpValidationResponse.Message);
                }

                // Reset Password

                var resetResult = await _userManager.ResetPasswordAsync(user, model.resetToken, model.NewPassword);
                if (!resetResult.Succeeded)
                {
                    var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    return new SrvResponse().Error(errors);
                }

                await transaction.CommitAsync();
                return new SrvResponse().Success("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var Rx_Message = ex.Message;
                if (ex.InnerException != null) Rx_Message += " | " + ex.InnerException.Message;
                return new SrvResponse().Error(Rx_Message);
            }

        }

        public async Task<SrvResponse> GenerateOtPForUser(string userId)
        {

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {


                var otp = GenerateNumericOtp(6);
                var salt = CreateSalt();
                var otpHash = HashOtp(otp, salt);

                // Set OTP expiration time (e.g., 5 minutes from now)
                var expirationTime = DateTime.UtcNow.AddMinutes(5);

                // Save OTP to the database
                var userOtp = new UserOTP
                {
                    UserId = userId,
                    OTPHash = otpHash,
                    Salt = salt,
                    ExpirationTime = expirationTime,
                    IsUsed = false
                };


                //check if there is an existing OTP for this user that is not used and not expired
                var existingOtp =
                    _context.UserOTPs.FirstOrDefault(o =>
                    o.UserId == userId && !o.IsUsed && o.ExpirationTime > DateTime.UtcNow);

                if (existingOtp != null)
                {
                    existingOtp.IsUsed = true; // Mark the existing OTP as used
                    _context.UserOTPs.Update(existingOtp);
                }

                _context.UserOTPs.Add(userOtp);
                _context.SaveChanges();
                await transaction.CommitAsync();

                return new SrvResponse().Success(otp);


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var Rx_Message = ex.Message;
                if (ex.InnerException != null) Rx_Message += " | " + ex.InnerException.Message;
                return new SrvResponse().Error(Rx_Message);
            }
        }

        public async Task<SrvResponse> ValidateOtpForUser(string userId, string otp)
        {
            try
            {

                // Retrieve the OTP record from the database
                var userOtp = _context.UserOTPs
                    .Where(o => o.UserId == userId && !o.IsUsed && o.ExpirationTime > DateTime.UtcNow)
                    .OrderByDescending(o => o.ExpirationTime) // Get the most recent OTP
                    .FirstOrDefault();

                if (userOtp == null)
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "No valid OTP found or OTP has expired.");
                }

                // Hash the provided OTP with the stored salt
                var hashedInputOtp = HashOtp(otp, userOtp.Salt);

                // Compare the hashed input OTP with the stored OTP hash
                if (hashedInputOtp == userOtp.OTPHash)
                {
                    // Set the OTP as used
                    userOtp.IsUsed = true;
                    _context.UserOTPs.Update(userOtp);
                    await _context.SaveChangesAsync();
                    return new SrvResponse().Success();
                }
                else
                {
                    return new SrvResponse().Error(Enums.ResponseCode.Unauthorized, "Invalid OTP.");
                }
            }
            catch (Exception ex)
            {
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
                new Claim(JwtRegisteredClaimNames.Name,user.FullName),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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

        private string GenerateNumericOtp(int digits = 6)
        {
            var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            uint value = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, digits);
            return value.ToString($"D{digits}");
        }

        private string HashOtp(string otp, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(otp + salt);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string CreateSalt(int len = 16)
        {
            var bytes = new byte[len];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }



    }
}
