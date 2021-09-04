using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace Treinapp.API.Features.Workouts
{
    [ApiController]
    [Route(Constants.DefaultRoute)]
    public class WorkoutsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            // TODO: Implement the Endpoint
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> BookNew()
        {
            // TODO: Implement the Endpoint
            return CreatedAtAction(nameof(ListAll), null);
        }

        [HttpPut("begin")]
        public async Task<IActionResult> Begin()
        {
            // TODO: Implement the Endpoint
            return AcceptedAtAction(nameof(ListAll), null);

        }

        [HttpPut("finish")]
        public async Task<IActionResult> Finish()
        {
            // TODO: Implement the Endpoint
            return AcceptedAtAction(nameof(ListAll), null);
        }
    }
}
