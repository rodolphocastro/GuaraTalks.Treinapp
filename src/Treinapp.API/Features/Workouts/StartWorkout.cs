using CloudNative.CloudEvents;

using Confluent.Kafka;

using MediatR;
using MediatR.Pipeline;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.API.Eventing;
using Treinapp.API.Features.Sports;
using Treinapp.Common;
using Treinapp.Commons.Domain;

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
            Sport sport = await database
                .GetSportsCollection()
                .FetchAsync(request.SportId, cancellationToken);

            if (sport is null)
            {
                return null;
            }

            sport = sport.UpdateWorkout(request.WorkoutId, w => w.Start(request.StartedAt));
            await database
                .GetSportsCollection()
                .UpdateAsync(sport.ToPersistence(), cancellationToken);
            return sport.Workouts.SingleOrDefault(w => w.Id.Equals(request.WorkoutId));
        }
    }

    /// <summary>
    /// Handler for publishing that a Workout was started.
    /// </summary>
    public class PublishWorkoutStarted : KafkaPublisherBase, IRequestPostProcessor<StartWorkout, Workout>
    {
        private readonly string requestSource;

        public PublishWorkoutStarted(
            ILogger<PublishWorkoutStarted> logger,
            IProducer<string, byte[]> producer,
            CloudEventFormatter cloudEventFormatter,
            IHttpContextAccessor httpContextAccessor
            ) : base(logger, producer, cloudEventFormatter, Constants.CloudEvents.WorkoutStartedTopic)
        {
            requestSource = httpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task Process(StartWorkout request, Workout response, CancellationToken cancellationToken)
        {
            if (response is null)
            {
                return Task.CompletedTask;
            }

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.WorkoutStartedType,
                Source = new UriBuilder(requestSource).Uri,
                Data = response
            };

            Task.Run(() => PublishToKafka(cloudEvent, cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }
    }
}
