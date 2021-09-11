using Bogus;

using MediatR;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Spammer.Features;

namespace Treinapp.Spammer
{
    public class Worker : BackgroundService
    {
        private Random rand = new();
        private readonly ILogger<Worker> _logger;
        private readonly ISender sender;
        private readonly Faker<CreateSportPayload> sportGenerator;
        private readonly Faker<BookWorkoutPayload> bookWorkoutsGenerator;

        public Worker(ILogger<Worker> logger,
            ISender sender,
            Faker<CreateSportPayload> sportGenerator,
            Faker<BookWorkoutPayload> bookWorkoutsGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.sportGenerator = sportGenerator ?? throw new ArgumentNullException(nameof(sportGenerator));
            this.bookWorkoutsGenerator = bookWorkoutsGenerator ?? throw new ArgumentNullException(nameof(bookWorkoutsGenerator));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                    var randPick = rand.Next(0, 2);
                    switch (randPick)
                    {
                        case 0:
                            await sender.Send(new CreateSport { Payload = sportGenerator.Generate() }, stoppingToken);
                            break;
                        case 1:
                            await sender.Send(new BookWorkout { Payload = bookWorkoutsGenerator.Generate() }, stoppingToken);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Worker failed to do a random action");
                }
                await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 60)), stoppingToken);
            }
        }
    }
}
