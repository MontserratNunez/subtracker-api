using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SubTracker.Core.Application.Dtos.Account;
using SubTracker.Core.Application.Dtos.Email;
using SubTracker.Core.Application.Dtos.User;
using SubTracker.Core.Application.Interfaces;
using SubTracker.Core.Domain.Settings;
using SubTracker.Infrastructure.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SubTracker.Infrastructure.Identity.Services
{
    public class AccountServiceForWebApi : IAccountServiceForWebApi
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IDataProtector _protector;

        public AccountServiceForWebApi(
            UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, 
            IEmailService emailService, 
            IOptions<JwtSettings> jwtSettings, 
            IConfiguration config,
            IDataProtectionProvider provider
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _config = config;
            _protector = provider.CreateProtector("SubTracker.JwtCookieProtector");
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

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

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
            string rawJwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            response.Name = user.Name;
            response.LastName = user.LastName;

            response.AccessToken = _protector.Protect(rawJwt);

            return response;
        }

        public async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto)
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

            var passwordResult = ValidatePasswordStrength(saveDto.Password);
            if (!passwordResult.IsSuccess)
            {
                response.HasError = true;
                response.Errors.Add(passwordResult.Message);
                return response;
            }

            AppUser user = new AppUser()
            {
                Name = saveDto.Name,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                ProfileImage = saveDto.ProfileImage,
                EmailConfirmed = false,
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                string verificationUri = await GetVerificationEmailUri(user);

                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $"Confirma tu cuenta visitando esta URL {verificationUri}",
                    Subject = "Confirmar registro"
                });

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

        public async Task SignOutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }
        }
 
        public virtual async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Message = "There is no acccount registered with this user";
                response.HasError = true;
                return response;
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                response.Message = $"Account confirmed for {user.Email}. You can now use the app";
                response.HasError = false;
                return response;
            }
            else
            {
                response.Message = $"An error occurred while confirming this email {user.Email}";
                response.HasError = true;
                return response;
            }
        }
 

        #region "Private methods"

        private async Task<string> GetVerificationEmailUri(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            string clientUrl = _config["ClientUrl"] ?? throw new InvalidOperationException("ClientUrl is not configured.");
            var route = "confirm-email";

            var completeUrl = new Uri(string.Concat(clientUrl.TrimEnd('/'), "/", route));
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            
            return QueryHelpers.AddQueryString(verificationUri, "token", token);
        }

        private async Task<JwtSecurityToken> GenerateJwtToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim("uid",user.Id ?? ""),
                new Claim("security_stamp", user.SecurityStamp ?? "")
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

        private (bool IsSuccess, string Message) ValidatePasswordStrength(string password)
        {
            var missingCriteria = new List<string>();
            if (password.Length < 8) missingCriteria.Add("al menos 8 caracteres");
            if (!password.Any(char.IsUpper)) missingCriteria.Add("una letra mayúscula");
            if (!password.Any(char.IsLower)) missingCriteria.Add("una letra minúscula");
            if (!password.Any(char.IsDigit)) missingCriteria.Add("un número");
            if (!password.Any(c => !char.IsLetterOrDigit(c))) missingCriteria.Add("un carácter especial");

            if (missingCriteria.Any())
            {
                return (false, $"La nueva contraseña no cumple las reglas. Falta: {string.Join(", ", missingCriteria)}.");
            }
            return (true, string.Empty);
        }

        #endregion
    }
}
