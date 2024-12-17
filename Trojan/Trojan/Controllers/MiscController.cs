using Autofac;
using Microsoft.AspNetCore.Mvc;
using Trojan.Models;

namespace Trojan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MiscController(ILifetimeScope lifetimeScope, IHttpClientFactory httpClient, ILogger<MiscController> logger) : Controller
    {
        [Route("cat-facts")]
        public async Task<IActionResult> CatFacts()
        {
            string body;
            try
            {
                using var client = httpClient.CreateClient();
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri("https://catfact.ninja/fact"),
                    Method = HttpMethod.Get
                };

                var response = await client.SendAsync(request);

                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return ResponseWriter.InternalServerError();
            }

            return ResponseWriter.Ok(body);
        }
    }
}
