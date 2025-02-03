using Core.Dto;

namespace Core.ServiceContracts
{
    public interface IJwtService
    {
        public string GenerateJwtToken(string email);
        public string GenerateRefreshToken(string email);
        public Task<bool> ValidateRefreshToken(string token);
        public string GetPrincipalEmailFromToken();
        public string GetTokenUserVersion(string email);
        public void IncrementUserVersion(string email);
    }
}
