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
            Sport sport = await database
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
    public class PublishWorkoutFinished : KafkaPublisherBase, IRequestPostProcessor<FinishWorkout, Workout>
    {
        private readonly string requestSource;

        public PublishWorkoutFinished(
            ILogger<PublishWorkoutFinished> logger,
            IProducer<string, byte[]> producer,
            CloudEventFormatter cloudEventFormatter,
            IHttpContextAccessor httpContextAccessor) : base(logger, producer, cloudEventFormatter, Constants.CloudEvents.WorkoutFinishedTopic)
        {
            requestSource = httpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task Process(FinishWorkout request, Workout response, CancellationToken cancellationToken)
        {
            if (response is null)
            {
                return Task.CompletedTask;
            }

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.WorkoutFinishedType,
                Source = new UriBuilder(requestSource).Uri,
                Data = response
            };

            Task.Run(() => PublishToKafka(cloudEvent, cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }
    }
}
