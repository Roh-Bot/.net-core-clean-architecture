using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Application;
using Project.WebApi.Authentication;
using Project.WebApi.Models;

namespace Project.WebApi.Controllers
{
    /// <summary>
    /// User controller
    /// </summary>
    /// <param name="userUseCase"></param>
    /// <param name="logger"></param>
    [ApiController]
    [Route("[controller]")]
    public class UserController(UserUseCase userUseCase, JwtAuth auth, ILogger<UserController> logger) : ControllerBase
    {
        private readonly Response _response = new();
        /// <summary>
        /// Creates a user with the given parameters
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Returns success</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, _response.BadRequest(ModelState));
            }

            await userUseCase.Create(user.ToDto());
            var authToken = auth.GenerateJwtToken(user.Email);

            HttpContext.Response.Cookies.Append("accessToken", authToken, new CookieOptions()
            {
                Expires = DateTime.Now.AddMinutes(auth.GetCookieExpiration()),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            var refreshToken = auth.GenerateRefreshToken(user.Email);

            var responseObject = new { authToken = authToken, refreshToken = refreshToken };
            return StatusCode(StatusCodes.Status200OK, _response.Ok(responseObject));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Read()
        {
            return StatusCode(StatusCodes.Status200OK, _response.Ok());
        }

        [Authorize]
        [HttpPost("generate-new-token")]
        public async Task<IActionResult> GenerateNewToken([FromBody] RefreshTokenModel model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, _response.BadRequest(ModelState));

            }

            if (!await auth.ValidateRefreshToken(model.RefreshToken))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, _response.Unauthorized("Invalid refresh token"));
            }

            var email = auth.GetPrincipalEmailFromToken();

            auth.IncrementTokenUserVersion(email);

            var authToken = auth.GenerateJwtToken(email);
            var newRefreshToken = auth.GenerateRefreshToken(email);

            return StatusCode(StatusCodes.Status200OK, _response.Ok(new { authToken = authToken, refreshToken = newRefreshToken }));
        }
    }
}
