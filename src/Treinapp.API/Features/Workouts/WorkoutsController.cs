using MediatR;

using Microsoft.AspNetCore.Mvc;

using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.API.Features.Workouts
{
    [ApiController]
    [Route(Constants.DefaultRoute)]
    public class WorkoutsController : ControllerBase
    {
        private readonly ISender sender;
        private CancellationToken Token => HttpContext?.RequestAborted ?? default;

        public WorkoutsController(ISender sender)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }

        [HttpGet("{SportId}")]
        public async Task<IActionResult> ListAll([FromRoute] ListWorkouts query)
        {
            var result = await sender.Send(query, Token);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> BookNew([FromBody] BookWorkout command)
        {
            var result = await sender.Send(command, Token);
            return CreatedAtAction(nameof(ListAll), result);
        }

        [HttpPut("begin")]
        public async Task<IActionResult> Begin([FromBody] StartWorkout command)
        {
            var result = await sender.Send(command, Token);
            return AcceptedAtAction(nameof(ListAll), result);

        }

        [HttpPut("finish")]
        public async Task<IActionResult> Finish([FromBody] FinishWorkout command)
        {
            var result = await sender.Send(command, Token);
            return AcceptedAtAction(nameof(ListAll), result);
        }
    }
}
