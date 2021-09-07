using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.API.Features.Sports;

namespace Treinapp.API.Features.Workouts
{
    /// <summary>
    /// Command to Start a Workout Session.
    /// </summary>
    public class FinishWorkout : IRequest<Workout>
    {
        [Required]
        public Guid SportId { get; set; }

        [Required]
        public Guid WorkoutId { get; set; }

        public DateTimeOffset FinishedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class FinishWorkoutHandler : IRequestHandler<FinishWorkout, Workout>
    {
        private readonly ILogger<StartWorkoutHandler> logger;
        private readonly IMongoDatabase database;

        public FinishWorkoutHandler(ILogger<StartWorkoutHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<Workout> Handle(FinishWorkout request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Finishing a workout");
            var sport = await database
                .GetSportsCollection()
                .FetchAsync(request.SportId, cancellationToken);
            sport = sport.UpdateWorkout(request.WorkoutId, w => w.Finish(request.FinishedAt));
            await database
                .GetSportsCollection()
                .UpdateAsync(sport.ToPersistence(), cancellationToken);
            return sport.Workouts.SingleOrDefault(w => w.Id.Equals(request.WorkoutId));
        }
    }
}
