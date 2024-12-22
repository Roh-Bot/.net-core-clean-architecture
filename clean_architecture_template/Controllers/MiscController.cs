using Autofac;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using clean_architecture_template.Models;
namespace clean_architecture_template.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MiscController(ILifetimeScope iLifetimeScope, ILogger<MiscController> logger) : ControllerBase
    {
        private readonly Response _response = new();

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCatFacts()
        {
            await using var scope = iLifetimeScope.BeginLifetimeScope() ;
            var catFacts = await scope.Resolve<IMiscService>().GetCatFacts();
            return StatusCode(StatusCodes.Status200OK, _response.Ok(catFacts));
        }

    }
}
