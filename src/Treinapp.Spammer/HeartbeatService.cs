using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.Spammer
{
    public class HeartbeatService : IHealthCheckPublisher
    {
        private readonly ILogger<HeartbeatService> logger;

        public HeartbeatService(ILogger<HeartbeatService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            switch (report.Status)
            {
                case HealthStatus.Healthy:
                    logger.LogInformation("This service is healthy");
                    break;
                case HealthStatus.Unhealthy:
                    logger.LogError("This service is unhealthy");
                    break;
            };

            // TODO: Actually publish the result somewhere
            return Task.CompletedTask;
        }
    }
}
