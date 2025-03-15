using Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.RepositoryContracts;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Core.Services
{
    public class JwtService(IUserRepository userRepo, IConfiguration configuration) : IJwtService
    {
        private const string VersionClaim = "version";
        private TokenValidationResult _tokenValidationResult = new();

        public string GenerateJwtToken(string email)
        {
            // Set token expiration
            var expiration = DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:TokenExpirationMinutes"]));

            var token = CreateToken(expiration, email);

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Generated token is null or empty.");
            }

            return token;
        }

        public string GenerateRefreshToken(string email)
        {
            var expiration = DateTime.Now.AddDays(Convert.ToDouble(configuration["Jwt:RefreshTokenExpirationDays"]));

            var token = CreateToken(expiration, email);
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Generated refresh token is null or empty.");
            }

            return token;
        }

        public async Task<bool> ValidateRefreshToken(string token)
        {
            var tokenValidationParams = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                ValidateLifetime = true
            };

            _tokenValidationResult = await new JwtSecurityTokenHandler().ValidateTokenAsync(token, tokenValidationParams);

            if (_tokenValidationResult is not { IsValid: true })
            {
                return false;
            }

            var email = GetPrincipalEmailFromToken();
            var tokenVersion = GetPrincipalVersionFromToken();
            var userVersion = userRepo.GetTokenUserVersion(email);

            return tokenVersion == userVersion;
        }

        public string GetPrincipalEmailFromToken()
        {
            return (string)_tokenValidationResult.Claims[ClaimTypes.Email];
        }

        public void IncrementUserVersion(string email)
        {
            userRepo.IncrementTokenUserVersion(email);
        }

        public string GetTokenUserVersion(string email)
        {
            return userRepo.GetTokenUserVersion(email);
        }

        private string? CreateToken(DateTime expiration, string email)
        {
            // Define claims
            Claim[] claims = [
                new(JwtRegisteredClaimNames.Sub, email),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Email, email),
                new(VersionClaim, userRepo.GetTokenUserVersion(email))
            ];

            // Get signing key
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Generate token
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                notBefore: DateTime.Now,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string GetPrincipalVersionFromToken()
        {
            return (string)_tokenValidationResult.Claims[VersionClaim];
        }
    }
}
