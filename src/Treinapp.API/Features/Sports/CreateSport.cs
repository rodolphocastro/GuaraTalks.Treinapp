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
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common;
using Treinapp.Commons.Domain;

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
        private readonly string requestSource;

        public PublishSportCreated(ILogger<PublishSportCreated> logger, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IHttpContextAccessor context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.cloudEventFormatter = cloudEventFormatter ?? throw new ArgumentNullException(nameof(cloudEventFormatter));
            requestSource = context?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(context));
        }

        public Task Process(CreateSport request, Sport response, CancellationToken cancellationToken)
        {
            if (response is null)
            {
                return Task.CompletedTask;
            }

            var cloudEvent = new CloudEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.SportCreatedType,
                Source = new UriBuilder(requestSource).Uri,
                Data = response
            };

            // Allow the publishing routine to run on its own Task
            Task.Run(() => PublishToKafka(cloudEvent, cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }

        private async Task<DeliveryResult<string, byte[]>> PublishToKafka(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            logger.LogTrace("Publishing a new CloudEvent {cloudEventType}", cloudEvent.Type);
            try
            {
                var result = await producer.ProduceAsync(
                                Constants.CloudEvents.SportCreatedTopic,
                                cloudEvent.ToKafkaMessage(ContentMode.Structured, cloudEventFormatter),
                                cancellationToken);
                logger.LogTrace("CloudEvent published successfully");
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error happened while publishing a CloudEvent to Kafka");
                throw;
            }
        }
    }
}
