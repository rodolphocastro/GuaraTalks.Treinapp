using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;

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
    public class PublishWorkoutStarted : IRequestPostProcessor<StartWorkout, Workout>
    {
        private readonly ILogger<PublishWorkoutStarted> logger;
        private readonly IProducer<string, byte[]> producer;
        private readonly CloudEventFormatter cloudEventFormatter;
        private readonly string requestSource;

        public PublishWorkoutStarted(
            ILogger<PublishWorkoutStarted> logger,
            IProducer<string, byte[]> producer,
            CloudEventFormatter cloudEventFormatter,
            IHttpContextAccessor httpContextAccessor
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.cloudEventFormatter = cloudEventFormatter ?? throw new ArgumentNullException(nameof(cloudEventFormatter));
            requestSource = httpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task Process(StartWorkout request, Workout response, CancellationToken cancellationToken)
        {
            if (response is null)
            {
                return;
            }

            logger.LogTrace("Publishing into Workout.Started topic");
            CloudEvent cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.WorkoutStartedType,
                Source = new UriBuilder(requestSource).Uri,
                Data = response
            };

            await producer.ProduceAsync(
                Constants.CloudEvents.WorkoutStartedTopic,
                cloudEvent.ToKafkaMessage(ContentMode.Structured, cloudEventFormatter),
                cancellationToken);
        }
    }
}
