using Autofac;
using Domain.Entities;
using Domain.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using Trojan.Models;
namespace Trojan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(ILifetimeScope lifetimeScope, ILogger<UserController> logger) : Controller
    {
        private readonly Response _response = new();
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] UserModelDto user)
        {
            if (!ModelState.IsValid)
            {
                return ResponseWriter.BadRequest(ModelState);
            }

            var serviceUser = new User()
            {
                Email = user.Email
            };

            await using var scope = lifetimeScope.BeginLifetimeScope();
            var userService = scope.Resolve<IUserService>();
            userService.Create(serviceUser);

            return StatusCode(StatusCodes.Status200OK, _response);
        }
    }
}
