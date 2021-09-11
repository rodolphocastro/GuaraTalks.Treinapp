using Bogus;

using MediatR;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Commons.Domain;
using Treinapp.Spammer.Features;

namespace Treinapp.Spammer
{
    public class Worker : BackgroundService
    {
        private Random rand = new();
        private readonly ILogger<Worker> _logger;
        private readonly ISender sender;
        private readonly Faker<CreateSportPayload> sportGenerator;

        public Worker(ILogger<Worker> logger,
            ISender sender,
            Faker<CreateSportPayload> sportGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.sportGenerator = sportGenerator ?? throw new ArgumentNullException(nameof(sportGenerator));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await sender.Send(new CreateSport { Payload = sportGenerator.Generate() });
                await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 60)), stoppingToken);
            }
        }
    }
}
