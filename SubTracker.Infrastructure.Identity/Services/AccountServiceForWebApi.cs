using SubTracker.Core.Application.Dtos.User;
using SubTracker.Core.Application.Interfaces;
using SubTracker.Core.Domain.Settings;
using SubTracker.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Text;

namespace SubTracker.Infrastructure.Identity.Services
{
    public class AccountServiceForWebApi : BaseAccountService, IAccountServiceForWebApi
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        public AccountServiceForWebApi(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, IOptions<JwtSettings> jwtSettings)
            : base(userManager, emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }
        public async Task<LoginResponseForApiDto> AuthenticateAsync(LoginApiDto loginDto)
        {
            LoginResponseForApiDto response = new()
            {
                LastName = "",
                Name = "",
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this username: {loginDto.UserName}");
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"This account {loginDto.UserName} is not active, you should check your email");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                if (result.IsLockedOut)
                {
                    response.Errors.Add($"Your account {loginDto.UserName} has been locked due to multiple failed attempts." +
                        $" Please try again in 10 minutes. If you don’t remember your password, you can go through the password " +
                        $"reset process.");
                }
                else
                {
                    response.Errors.Add($"these credentials are invalid for this user: {user.UserName}");
                }
                return response;
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(user);

            response.Name = user.Name;
            response.LastName = user.LastName;
            response.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return response;
        }

        public override async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string? origin, bool? isApi = false)
        {
            RegisterResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"this username: {saveDto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"this email: {saveDto.Email} is already taken.");
                return response;
            }

            AppUser user = new AppUser()
            {
                Name = saveDto.Name,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                ProfileImage = saveDto.ProfileImage,
                EmailConfirmed = true,
                IsActive = true,
                PhoneNumber = saveDto.Phone,
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                response.Id = user.Id;
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.Email = user.Email;
                response.UserName = user.UserName;
            }
            else
            {
                response.HasError = true;
                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Description);
                }
            }

            return response;
        }

        public override async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false)
        {
            return await base.EditUser(saveDto, null, isCreated, isApi);
        }

        public override async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        { 
            return await base.ForgotPasswordAsync(request, isApi);
        }

        #region "private methods"

        private async Task<JwtSecurityToken> GenerateJwtToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var rolesClaims = new List<Claim>();
            foreach (var role in roles)
            {
                rolesClaims.Add(new Claim("roles", role));
            }
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("uid",user.Id ?? "")
            }.Union(userClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }

        #endregion
    }
}
