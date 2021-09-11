using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.Spammer
{
    public class HeartbeatConfiguration
    {
        public int TcpPort { get; set; } = 6666;
    }

    public class HeartbeatService : BackgroundService, IHealthCheckPublisher
    {
        private readonly ILogger<HeartbeatService> logger;
        private readonly TcpListener tcpListener;
        private HealthReport lastReport = null;

        public HeartbeatService(ILogger<HeartbeatService> logger,
            IOptions<HeartbeatConfiguration> cfg)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            tcpListener = new TcpListener(System.Net.IPAddress.Any, cfg.Value.TcpPort);
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

            lastReport = report;
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting HearbeatService");
            await Task.Yield();
            tcpListener.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                await ReactToIncomingChecks(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            tcpListener.Start();
        }

        private async Task ReactToIncomingChecks(CancellationToken stoppingToken)
        {
            var isHealthy = (lastReport?.Status ?? HealthStatus.Unhealthy) == HealthStatus.Healthy;
            if (!isHealthy)
            {
                tcpListener.Stop();
                logger.LogError("Instance is unhealthy, closing tcp socket");
                return;
            }

            tcpListener.Start();
            while (tcpListener.Server.IsBound && tcpListener.Pending())
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                client.Close();
                logger.LogInformation("Successfully processed a health-check");
            }

            logger.LogDebug("Healthcheck executed");
        }
    }
}
