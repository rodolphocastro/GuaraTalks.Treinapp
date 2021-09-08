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
        private readonly ILogger<FinishWorkoutHandler> logger;
        private readonly IMongoDatabase database;

        public FinishWorkoutHandler(ILogger<FinishWorkoutHandler> logger, IMongoDatabase database)
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

            if (sport is null)
            {
                return null;
            }

            sport = sport.UpdateWorkout(request.WorkoutId, w => w.Finish(request.FinishedAt));
            await database
                .GetSportsCollection()
                .UpdateAsync(sport.ToPersistence(), cancellationToken);
            return sport.Workouts.SingleOrDefault(w => w.Id.Equals(request.WorkoutId));
        }
    }

    /// <summary>
    /// Handler for publishing that a Workout was finished.
    /// </summary>
    public class PublishWorkoutFinished : IRequestPostProcessor<FinishWorkout, Workout>
    {
        private ILogger<PublishWorkoutFinished> logger;
        private IProducer<string, byte[]> producer;
        private CloudEventFormatter cloudEventFormatter;
        private string requestSource;

        public PublishWorkoutFinished(
            ILogger<PublishWorkoutFinished> logger,
            IProducer<string, byte[]> producer,
            CloudEventFormatter cloudEventFormatter,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.cloudEventFormatter = cloudEventFormatter ?? throw new ArgumentNullException(nameof(cloudEventFormatter));
            requestSource = httpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task Process(FinishWorkout request, Workout response, CancellationToken cancellationToken)
        {
            if (response is null)
            {
                return;
            }

            logger.LogTrace("Publishing into Workout.Finished topic");
            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.WorkoutFinishedType,
                Source = new Uri(requestSource),
                Data = response
            };

            await producer.ProduceAsync
                (
                Constants.CloudEvents.WorkoutFinishedTopic,
                cloudEvent.ToKafkaMessage(ContentMode.Structured, cloudEventFormatter),
                cancellationToken);
        }
    }
}
