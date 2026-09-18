using SubTracker.Core.Application.Dtos.Account;
using SubTracker.Core.Application.Dtos.User;

namespace SubTracker.Core.Application.Interfaces
{
    public interface IBaseAccountService
    {
        Task<UserResponseDto> ConfirmAccountAsync(string userId, string token);
        Task<UserResponseDto> DeleteAsync(string id);
        Task<EditResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserById(string Id);
        Task<UserDto?> GetUserByUserName(string userName);
        Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string? origin);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}