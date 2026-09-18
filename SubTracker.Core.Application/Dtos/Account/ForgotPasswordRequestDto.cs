namespace SubTracker.Core.Application.Dtos.Account
{
    public class ForgotPasswordRequestDto
    {
        public required string Email { get; set; }
        public required string Origin { get; set; }
    }
}
