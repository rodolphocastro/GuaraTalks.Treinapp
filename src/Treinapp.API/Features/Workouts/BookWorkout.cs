using DnsClient.Internal;

using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.API.Features.Sports;

namespace Treinapp.API.Features.Workouts
{
    /// <summary>
    /// Command to book (create) a Workout.
    /// </summary>
    public class BookWorkout : IRequest<Workout>
    {
        [Required]
        public Guid SportId { get; set; }

        public DateTimeOffset BookAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class BookWorkoutHandler : IRequestHandler<BookWorkout, Workout>
    {
        private readonly ILogger<BookWorkoutHandler> logger;
        private readonly IMongoDatabase database;

        public BookWorkoutHandler(ILogger<BookWorkoutHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<Workout> Handle(BookWorkout request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Booking a new Workout");
            var workout = new Workout(Guid.NewGuid(), request.BookAt);
            var sport = await database
                .GetSportsCollection()
                .FetchAsync(request.SportId, cancellationToken);
            sport = sport.BookWorkout(workout);
            await database.GetSportsCollection().UpdateAsync(sport.ToPersistence(), cancellationToken);
            return workout;
        }
    }
}
