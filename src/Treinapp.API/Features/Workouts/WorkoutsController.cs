using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common;
using Treinapp.Commons.Domain;

namespace Treinapp.API.Features.Workouts
{
    [ApiController]
    [Route(Constants.DefaultRoute)]
    public class WorkoutsController : ControllerBase
    {
        private readonly ISender sender;

        public WorkoutsController(ISender sender)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }

        [HttpGet("{SportId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Workout>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListAll([FromRoute] ListWorkouts query, CancellationToken cancellationToken)
        {
            IEnumerable<Workout> result = await sender.Send(query, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Workout))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BookNew([FromBody] BookWorkout command, CancellationToken cancellationToken)
        {
            Workout result = await sender.Send(command, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }
            return Created("", result);
        }

        [HttpPut("begin")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(Workout))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Begin([FromBody] StartWorkout command, CancellationToken cancellationToken)
        {
            Workout result = await sender.Send(command, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }
            return Accepted(result);
        }

        [HttpPut("finish")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(Workout))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Finish([FromBody] FinishWorkout command, CancellationToken cancellationToken)
        {
            Workout result = await sender.Send(command, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }
            return Accepted(result);
        }
    }
}
