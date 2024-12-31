using clean_architecture_template.Models;
using Core.Dto;
using Core.ServiceContracts;
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
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, _response.BadRequest(ModelState));
            }

            await userService.Create(user.ToDto());
            var token = jwtService.CreateToken(user.ToDto());
            var responseObject = new { token = token };
            return StatusCode(StatusCodes.Status200OK, _response.Ok(responseObject));
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Read()
        {
            return StatusCode(StatusCodes.Status200OK, _response.Ok());
        }
    }
}
