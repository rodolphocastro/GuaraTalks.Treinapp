using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common;
using Treinapp.Commons.Domain;

namespace Treinapp.Reports.Worker
{
    public class SportsCreatedWorker : BackgroundService
    {
        private readonly ILogger<SportsCreatedWorker> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly CloudEventFormatter cloudEventFormatter = new JsonEventFormatter<Sport>();

        public SportsCreatedWorker(
            ILogger<SportsCreatedWorker> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                        _logger.LogInformation("Received a sport!");
                    }                    
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to parse CloudEvent");
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
