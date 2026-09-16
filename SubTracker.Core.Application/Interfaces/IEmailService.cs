using SubTracker.Core.Application.Dtos.Email;

namespace SubTracker.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}