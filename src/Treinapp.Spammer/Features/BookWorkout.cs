using Bogus;

using MediatR;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Commons.Domain;

namespace Treinapp.Spammer.Features
{
    public class BookWorkoutPayloadGenerator : Faker<BookWorkoutPayload>
    {
        public BookWorkoutPayloadGenerator(ICollection<Sport> sports)
        {
            RuleFor(p => p.SportId, f => f.PickRandom(sports).Id);
            RuleFor(p => p.BookAt, f => f.Date.FutureOffset());
        }
    }
    public class BookWorkout : IRequest
    {
        public BookWorkoutPayload Payload { get; init; }
    }

    public class BookWorkoutHandler : IRequestHandler<BookWorkout, Unit>
    {
        private readonly ILogger<BookWorkoutHandler> logger;
        private readonly ITreinappApi api;
        private readonly ICollection<Sport> sports;

        public BookWorkoutHandler(ILogger<BookWorkoutHandler> logger,
            ITreinappApi api,
            ICollection<Sport> sports)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.api = api ?? throw new ArgumentNullException(nameof(api));
            this.sports = sports ?? throw new ArgumentNullException(nameof(sports));
        }

        public async Task<Unit> Handle(BookWorkout request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Booking a new Workout");
            var sport = sports.SingleOrDefault(s => s.Id.Equals(request.Payload.SportId));
            try
            {
                var workout = await api.BookNew(request.Payload, cancellationToken);
                var updatedSport = sport.BookWorkout(workout);
                sports.Remove(sport);
                sports.Add(sport);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to book a new Workout");
            }

            return Unit.Value;
        }
    }
}
