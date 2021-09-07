using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;

using Confluent.Kafka;

using MediatR;
using MediatR.Pipeline;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.API.Features.Sports
{
    /// <summary>
    /// Command to create a new Sport in the system.
    /// </summary>
    public class CreateSport : IRequest<Sport>
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; init; }

        [Required(AllowEmptyStrings = false)]
        public string Description { get; init; }
    }

    /// <summary>
    /// Handler for the CreateSport command.
    /// </summary>
    public class CreateSportHandler : IRequestHandler<CreateSport, Sport>
    {
        private readonly ILogger<CreateSportHandler> logger;
        private readonly IMongoDatabase database;

        public CreateSportHandler(ILogger<CreateSportHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<Sport> Handle(CreateSport request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Creating a new sport");
            var sportPoco = new SportPersistence
            {
                Description = request.Description,
                Name = request.Name
            };
            await database
                .GetSportsCollection()
                .InsertNewAsync(sportPoco, cancellationToken);
            return sportPoco.ToSport();
        }
    }

    /// <summary>
    /// Handler for publishing that a Sport was created.
    /// </summary>
    public class PublishSportCreated : IRequestPostProcessor<CreateSport, Sport>
    {
        private readonly ILogger<PublishSportCreated> logger;
        private readonly IProducer<string, byte[]> producer;
        private readonly CloudEventFormatter cloudEventFormatter;

        public PublishSportCreated(ILogger<PublishSportCreated> logger, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.cloudEventFormatter = cloudEventFormatter ?? throw new ArgumentNullException(nameof(cloudEventFormatter));
        }
        public async Task Process(CreateSport request, Sport response, CancellationToken cancellationToken)
        {
            logger.LogTrace("Publishing into Sport.Created topic");
            var cloudEvent = new CloudEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Type = "treinapp.sports.v1.created",
                Source = new Uri("http://treinapp.com/api"),
                Data = response
            };

            await producer.ProduceAsync("sport.created", cloudEvent.ToKafkaMessage(ContentMode.Structured, cloudEventFormatter), cancellationToken);
        }
    }
}
