using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using clean_architecture_template.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace clean_architecture_template.Middlewares;

    public class JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtService jwtService,
        IConfiguration configuration) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        private const string Version = "version";
        private const string InvalidTokenResponse = "Invalid authentication token";

        private readonly ILogger<JwtSecurityTokenHandler> _logger = logger.CreateLogger<JwtSecurityTokenHandler>();
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Request.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), string.Empty));
            }
            var authToken = ExtractTokenFromHeader(Request.HttpContext);
            if (string.IsNullOrEmpty(authToken))
            {
                return AuthenticateResult.Fail(InvalidTokenResponse);
            }

            var claimsPrincipal = ValidateToken(authToken);
            if (claimsPrincipal is null)
            {
                return AuthenticateResult.Fail(InvalidTokenResponse);
            }

            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return AuthenticateResult.Fail(InvalidTokenResponse);
            }
            var tokenVersion = claimsPrincipal.FindFirst(Version)?.Value;
            if (string.IsNullOrEmpty(tokenVersion))
            {
                return AuthenticateResult.Fail(InvalidTokenResponse);
            }

            var currentTokenVersion = jwtService.GetTokenUserVersion(email);

            if (tokenVersion != currentTokenVersion)
            {
                _logger.LogWarning("Invalid token version detected for user: {Email}", email);
                await Response.WriteAsJsonAsync(new Response().Unauthorized("Token is outdated. Please generated a new one."));
                return AuthenticateResult.Fail("Token is outdated");
            }

            var claims = new[] { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, JwtBearerDefaults.AuthenticationScheme);
            return AuthenticateResult.Success(ticket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Response.WriteAsJsonAsync(new Response().Unauthorized(InvalidTokenResponse));
        }

        private static string? ExtractTokenFromHeader(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (authHeader.IsNullOrEmpty() || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader["Bearer ".Length..];
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            var secret = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!);

            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secret),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out _);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }
    }

