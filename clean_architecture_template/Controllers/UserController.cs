using clean_architecture_template.Models;
using Core.Dto;
using Core.ServiceContracts;
using Core.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace clean_architecture_template.Controllers
{
    /// <summary>
    /// User controller
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="logger"></param>
    [ApiController]
    [Route("[controller]")]
    public class UserController(IUserService userService, IJwtService jwtService, ILogger<UserController> logger) : ControllerBase
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

            await userService.Create(user.ToDto());
            var authToken = jwtService.GenerateJwtToken(user.Email);
            var refreshToken = jwtService.GenerateRefreshToken(user.Email);

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

            if (!await jwtService.ValidateRefreshToken(model.RefreshToken))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, _response.Unauthorized("Invalid refresh token"));
            }

            var email = jwtService.GetPrincipalEmailFromToken();

            jwtService.IncrementUserVersion(email);

            var authToken = jwtService.GenerateJwtToken(email);
            var newRefreshToken = jwtService.GenerateRefreshToken(email);

            return StatusCode(StatusCodes.Status200OK, _response.Ok(new { authToken = authToken, refreshToken = newRefreshToken }));
        }
    }
}
