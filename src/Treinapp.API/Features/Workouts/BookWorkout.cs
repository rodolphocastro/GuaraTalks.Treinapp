using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;

using Confluent.Kafka;

using DnsClient.Internal;

using MediatR;
using MediatR.Pipeline;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.API.Features.Sports;
using Treinapp.Common;
using Treinapp.Commons.Domain;

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

            if (sport is null)
            {
                return null;
            }

            sport = sport.BookWorkout(workout);
            await database.GetSportsCollection().UpdateAsync(sport.ToPersistence(), cancellationToken);
            return workout;
        }
    }

    /// <summary>
    /// Handler for publishing that a Workout was booked.
    /// </summary>
    public class PublishWorkoutBooked : IRequestPostProcessor<BookWorkout, Workout>
    {
        private readonly ILogger<PublishWorkoutBooked> logger;
        private readonly IProducer<string, byte[]> producer;
        private readonly CloudEventFormatter cloudEventFormatter;
        private readonly string requestSource;

        public PublishWorkoutBooked(
            ILogger<PublishWorkoutBooked> logger,
            IProducer<string, byte[]> producer,
            CloudEventFormatter cloudEventFormatter,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.cloudEventFormatter = cloudEventFormatter ?? throw new ArgumentNullException(nameof(cloudEventFormatter));
            requestSource = httpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task Process(BookWorkout request, Workout response, CancellationToken cancellationToken)
        {
            if (response is null)
            {
                return;
            }

            logger.LogTrace("Publishing into Workout.Booked topic");
            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.WorkoutBookedType,
                Source = new Uri(requestSource),
                Data = response
            };

            await producer.ProduceAsync(
                Constants.CloudEvents.WorkoutBookedTopic,
                cloudEvent.ToKafkaMessage(ContentMode.Structured, cloudEventFormatter),
                cancellationToken);
        }
    }
}
