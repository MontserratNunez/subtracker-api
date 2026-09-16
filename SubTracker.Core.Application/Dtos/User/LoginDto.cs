namespace SubTracker.Core.Application.Dtos.User
{
    public class LoginDto
    {
        public required string Identifier { get; set; }
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
