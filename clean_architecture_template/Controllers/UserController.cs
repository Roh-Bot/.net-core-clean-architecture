using clean_architecture_template.Models;
using Core.Dto;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace clean_architecture_template.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
    {
        private readonly Response _response = new();
        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, _response.BadRequest(ModelState));
            }
            await userService.Create(new UserDto()
            {
                Username = user.Username
            });
            return StatusCode(StatusCodes.Status200OK, _response.Ok());
        }
    }
}
