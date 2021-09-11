using Bogus;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

using Refit;

using System;
using System.Collections.Generic;

using Treinapp.Common;
using Treinapp.Commons.Domain;
using Treinapp.Spammer.Features;

namespace Treinapp.Spammer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services
                        .AddHealthChecks()
                        .AddUrlGroup(new UriBuilder($"{configuration.GetConnectionString(Constants.TreinappApiKey)}/health").Uri);
                    services.AddSingleton<ICollection<Sport>>(new HashSet<Sport>());
                    services.AddSingleton<Faker<CreateSportPayload>, SportBogusGenerator>();
                    services.AddSingleton<Faker<BookWorkoutPayload>, BookWorkoutPayloadGenerator>();
                    services.AddMediatR(typeof(Program).Assembly);
                    services
                        .AddRefitClient<ITreinappApi>()
                        .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration.GetConnectionString(Constants.TreinappApiKey)));
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IHealthCheckPublisher, HeartbeatService>();
                });
    }
}
