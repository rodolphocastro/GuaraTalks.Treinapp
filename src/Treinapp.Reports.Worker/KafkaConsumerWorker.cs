using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.Reports.Worker
{
    public abstract class KafkaConsumerWorker : BackgroundService
    {
        protected readonly ILogger<KafkaConsumerWorker> _logger;
        protected readonly IServiceProvider serviceProvider;
        protected readonly CloudEventFormatter cloudEventFormatter;
        protected IConsumer<string, byte[]> KafkaConsumer { get; private set; } = null;

        public KafkaConsumerWorker(ILogger<KafkaConsumerWorker> logger,
            IServiceProvider serviceProvider,
            CloudEventFormatter formatter = null)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
            cloudEventFormatter = formatter ?? new JsonEventFormatter();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting {serviceName}", GetType().FullName);
            KafkaConsumer = serviceProvider
                .CreateScope().ServiceProvider
                .GetRequiredService<IConsumer<string, byte[]>>();
            return Task.Run(() => DoScoped(stoppingToken), stoppingToken);  // HACK: This is required so we can have multiple services running in parallel
        }

        protected abstract Task DoScoped(CancellationToken cancellationToken);

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            KafkaConsumer?.Dispose();
            base.Dispose();
        }
    }
}