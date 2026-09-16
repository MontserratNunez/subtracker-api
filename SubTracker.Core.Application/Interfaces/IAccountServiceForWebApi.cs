using SubTracker.Core.Application.Dtos.User;

namespace SubTracker.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApi : IBaseAccountService
    {
        Task<LoginResponseForApiDto> AuthenticateAsync(LoginApiDto loginDto);
    }
}