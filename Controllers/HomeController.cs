using Microsoft.AspNetCore.Mvc;
using Suitex.Attributes;

// Health Check --> Checagem de saúde
// Saber se a API está online ou offline
// Endpoint que responde 200 se estiver tudo OK
namespace Suitex.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        [ApiKey]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
