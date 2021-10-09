using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;

using Confluent.Kafka;

using DnsClient.Internal;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.API.Eventing
{
    /// <summary>
    /// Base implementation to a CloudEvent publisher.
    /// </summary>
    public class KafkaPublisherBase
    {
        private readonly ILogger<KafkaPublisherBase> logger;
        private readonly IProducer<string, byte[]> producer;
        private readonly CloudEventFormatter cloudEventFormatter;
        private readonly string topic;

        /// <summary>
        /// Creates a new Publisher
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="producer"></param>
        /// <param name="cloudEventFormatter"></param>
        /// <param name="topic"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public KafkaPublisherBase(ILogger<KafkaPublisherBase> logger, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException($"'{nameof(topic)}' cannot be null or empty.", nameof(topic));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.cloudEventFormatter = cloudEventFormatter ?? throw new ArgumentNullException(nameof(cloudEventFormatter));
            this.topic = topic;
        }

        protected virtual Task<DeliveryResult<string, byte[]>> PublishToKafka(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            logger.LogTrace("Publishing a new CloudEvent {cloudEventType}", cloudEvent.Type);

            try
            {
                var result = producer.ProduceAsync(
                           topic,
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