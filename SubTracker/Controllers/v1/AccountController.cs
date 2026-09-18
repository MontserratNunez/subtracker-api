using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SubTracker.Core.Application.Dtos.Account;
using SubTracker.Core.Application.Dtos.User;
using SubTracker.Core.Application.Interfaces;
using SubTracker.Infrastructure.Identity.Entities;
using System.Security.Claims;


namespace SubTracker.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AccountController : BaseApiController
    {
        private readonly IAccountServiceForWebApi _accountServiceForWebApi;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(IAccountServiceForWebApi accountServiceForWebApi, UserManager<AppUser> userManager)
        {
            _accountServiceForWebApi = accountServiceForWebApi;
            _userManager = userManager;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginApiDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var response = await _accountServiceForWebApi.AuthenticateAsync(dto);

                if (response.HasError)
                    return BadRequest(response.Errors);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };

                Response.Cookies.Append("accessToken", response.AccessToken, cookieOptions);

                return Ok(new { message = "Sesión iniciada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterApiRequest dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var save = new SaveUserDto
                {
                    Id = "",
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password,
                    UserName = dto.UserName
                };

                var result = await _accountServiceForWebApi.RegisterUser(save);

                if (result == null || result.HasError)
                {
                    return BadRequest(result?.Errors);
                }

                return Created(string.Empty, null);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize]
        [IgnoreAntiforgeryToken]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    await _accountServiceForWebApi.SignOutAsync(userId);
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None
                    };

                    Response.Cookies.Delete("accessToken", cookieOptions);
                    return Ok(new { message = "Sesión cerrada correctamente." });
                }
                return BadRequest(new { message = "No se encontró la identidad del usuario en el token." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

