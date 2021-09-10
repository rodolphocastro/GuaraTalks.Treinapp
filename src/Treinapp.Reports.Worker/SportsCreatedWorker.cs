using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common;
using Treinapp.Commons.Domain;
using Treinapp.Reports.Worker.Features.Reports;

namespace Treinapp.Reports.Worker
{
    public class SportsCreatedWorker : BackgroundService
    {
        private readonly ILogger<SportsCreatedWorker> _logger;
        private readonly IMongoDatabase database;
        private readonly IServiceProvider serviceProvider;
        private readonly CloudEventFormatter cloudEventFormatter = new JsonEventFormatter<Sport>();

        public SportsCreatedWorker(
            ILogger<SportsCreatedWorker> logger,
            IMongoDatabase database,
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Sports.created consumer");
            using var consumer = serviceProvider
                .CreateScope().ServiceProvider
                    .GetRequiredService<IConsumer<string, byte[]>>();

            consumer.Subscribe(Constants.CloudEvents.SportCreatedTopic);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is Sport createdSport)
                    {
                        _logger.LogTrace("Attempting to update a report with the new sport");
                        var report = await database
                            .GetReportsCollection()
                            .FetchAsync(DateTimeOffset.UtcNow, stoppingToken);
                        if (report is null)
                        {
                            report = await database
                                .GetReportsCollection()
                                .InsertNewAsync(new Report(Guid.NewGuid()));
                        }
                        report = report.WithCreatedSport(createdSport);
                        await database
                            .GetReportsCollection()
                            .UpdateAsync(report, stoppingToken);
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
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }

            }
            consumer.Unsubscribe();
        }
    }
}
