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
    /// Command to Start a Workout session.
    /// </summary>
    public class StartWorkout : IRequest<Workout>
    {
        [Required]
        public Guid SportId { get; set; }

        [Required]
        public Guid WorkoutId { get; set; }

        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class StartWorkoutHandler : IRequestHandler<StartWorkout, Workout>
    {
        private readonly ILogger<StartWorkoutHandler> logger;
        private readonly IMongoDatabase database;

        public StartWorkoutHandler(ILogger<StartWorkoutHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }
        public async Task<Workout> Handle(StartWorkout request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Starting a workout");
            var sport = await database
                .GetSportsCollection()
                .FetchAsync(request.SportId, cancellationToken);
            sport = sport.UpdateWorkout(request.WorkoutId, w => w.Start(request.StartedAt));
            await database
                .GetSportsCollection()
                .UpdateAsync(sport.ToPersistence(), cancellationToken);
            return sport.Workouts.SingleOrDefault(w => w.Id.Equals(request.WorkoutId));
        }
    }
}
