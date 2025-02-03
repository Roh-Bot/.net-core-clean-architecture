namespace Core.Dto
{
    public class RefreshTokenDto
    {
        public required string Email { get; set; }
        public required string RefreshToken { get; set; }
    }
}
