using Autofac;
using clean_architecture_template.Models;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
namespace clean_architecture_template.Controllers
{
    /// <summary>
    /// Misc Controller
    /// </summary>
    /// <param name="iLifetimeScope"></param>
    /// <param name="logger"></param>
    //[ApiController]
    //[Route("[controller]")]
    //public class MiscController(ILifetimeScope iLifetimeScope, ILogger<MiscController> logger) : ControllerBase
    //{
    //    private readonly Response _response = new();

    //    /// <summary>
    //    /// To get cat facts from an external api
    //    /// </summary>
    //    /// <returns>Returns JSON response containing cat facts</returns>
    //    [HttpGet("[action]")]
    //    public async Task<IActionResult> GetCatFacts()
    //    {
    //        await using var scope = iLifetimeScope.BeginLifetimeScope();
    //        var catFacts = await scope.Resolve<IMiscService>().GetCatFacts();
    //        return StatusCode(StatusCodes.Status200OK, _response.Ok(catFacts));
    //    }

    //}
}
