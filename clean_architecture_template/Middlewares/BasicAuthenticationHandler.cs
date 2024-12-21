using clean_architecture_template.Models;
using Core.Dto;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace clean_architecture_template.Middlewares
{
    public class BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserService userService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            if (authHeader == string.Empty)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var encodedToken = authHeader["Basic ".Length..].Trim();
            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(encodedToken));
            var credentials = decodedToken.Split(':');

            if (credentials.Length != 2)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }
            var username = credentials[0];
            var password = credentials[1];

            if (!await ValidateUserCredentials(username, password))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, "Basic");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Basic");
            return AuthenticateResult.Success(ticket);
        }
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401; // Unauthorized status code
            Response.ContentType = "application/json";

            var errorResponse = new
            {
                message = "Unauthorized access. Please provide valid credentials.",
                statusCode = 401
            };

            await Response.WriteAsJsonAsync(new Response { Status = -1, Error = "Unauthorized" });
        }

        private async Task<bool> ValidateUserCredentials(string username, string password)
        {
            var userDto = new UserDto()
            {
                Username = username,
                Password = password
            };
            await userService.Read(userDto);
            // Replace with your actual logic to validate the user credentials
            return username == "admin" && password == "password";  // Example credentials
        }
    }
}
