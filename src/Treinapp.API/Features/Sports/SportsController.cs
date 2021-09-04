using Microsoft.AspNetCore.Mvc;

using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.API.Features.Sports
{
    [ApiController]
    [Route(Constants.DefaultRoute)]
    public class SportsController : ControllerBase
    {
        private CancellationToken _token => HttpContext?.RequestAborted ?? default;

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            // TODO: Implement the Endpoint
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNew()
        {
            // TODO: Implement the Endpoint
            return CreatedAtAction(nameof(ListAll), null);
        }
    }
}
