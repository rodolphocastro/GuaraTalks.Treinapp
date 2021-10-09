using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common;
using Treinapp.Common.Domain;
using Treinapp.Commons.Domain;
using Treinapp.Reports.Worker.Features.Reports;

namespace Treinapp.Reports.Worker
{
    public class SportsCreatedWorker : KafkaConsumerWorker
    {
        private readonly IMongoDatabase database;
        private readonly ISender sender;

        public SportsCreatedWorker(
            ILogger<SportsCreatedWorker> logger,
            IMongoDatabase database,
            IServiceProvider serviceProvider,
            ISender sender) : base(logger, serviceProvider, new JsonEventFormatter<Sport>())
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        protected override async Task DoScoped(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe(Constants.CloudEvents.SportCreatedTopic);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is Sport createdSport)
                    {
                        _logger.LogTrace("Attempting to update a report with the new sport");
                        Report report = await database
                            .GetReportsCollection()
                            .FetchAsync(DateTimeOffset.UtcNow, cancellationToken);
                        if (report is null)
                        {
                            report = await sender.Send(new CreateReport(), cancellationToken);
                        }

                        _ = await sender.Send(new AppendCreatedSport
                        {
                            AppendTo = report,
                            Append = createdSport
                        }, cancellationToken);
                    }
                }
                // Consumer errors should generally be ignored (or logged) unless fatal.
                catch (ConsumeException e) when (e.Error.IsFatal)
                {
                    _logger.LogError(e, "A fatal consumer error happened");
                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception happened, oh no");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }

            }
            KafkaConsumer.Unsubscribe();
        }
    }
}
