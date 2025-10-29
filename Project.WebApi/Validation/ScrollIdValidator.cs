using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project.WebApi.Validation;

public class ScrollIdValidator(IConfiguration configuration, ILogger<ScrollIdValidator> logger)
{
    #region Jwt

    public string CreateToken(string applicationName, string scrollId)
    {
        Claim[] claims = [
            new(JwtRegisteredClaimNames.Sid, scrollId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.FamilyName, applicationName),
        ];

        // Get signing key
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Generate token
        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: applicationName,
            claims: claims,
            notBefore: DateTime.Now,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public (bool isValid, int? scrollId) ValidateToken(string applicationName, string token)
    {
        var secret = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!);

        var tokenValidationParams = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secret),
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = applicationName,
            ValidateLifetime = false

        };

        try
        {
            var claims = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParams, out _);

            if (claims is null)
            {
                return (false, null);
            }

            var sidClaim = claims.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sid);

            if (sidClaim == null || !int.TryParse(sidClaim.Value, out var scrollId))
            {
                return (false, null);
            }

            return (true, scrollId);
        }
        catch (Exception ex)
        {
            logger.LogError("{ExceptionType} {ExceptionMessage}\n{ExceptionStackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            return (false, null);
        }
    }

    #endregion
}