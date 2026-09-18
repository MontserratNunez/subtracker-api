using SubTracker.Core.Application.Dtos.Account;
using SubTracker.Core.Application.Dtos.User;

namespace SubTracker.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApi
    {
        Task<LoginResponseForApiDto> AuthenticateAsync(LoginApiDto loginDto);
        Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto);
        Task SignOutAsync(string userId);
        Task<UserResponseDto> ConfirmAccountAsync(string userId, string token);
    }
}