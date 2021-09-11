using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using System;

using Treinapp.Common;
using Treinapp.Spammer;

namespace Treinapp.Reports.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration ?? throw new ArgumentException("Configurations weren't set for this worker, unable to continue");

                    services
                        .AddHealthChecks()
                        .AddMongoDb(configuration.GetConnectionString(Constants.MongoConnectionKey))
                        .AddKafka(new ProducerConfig
                        {
                            BootstrapServers = configuration.GetConnectionString(Constants.KafkaBootstrapKey)
                        });

                    services.AddSingleton(_ =>
                    {
                        return new MongoClient(configuration.GetConnectionString(Constants.MongoConnectionKey));
                    });
                    services.AddScoped(sp =>
                    {
                        var mongoClient = sp.GetRequiredService<MongoClient>();
                        return mongoClient.GetDatabase(Constants.MongoReportsDatabase);
                    });

                    services.AddScoped(c =>
                    {
                        var config = new ConsumerConfig
                        {
                            BootstrapServers = configuration.GetConnectionString(Constants.KafkaBootstrapKey),
                            GroupId = "reports-worker",
                            AutoOffsetReset = AutoOffsetReset.Earliest
                        };
                        return new ConsumerBuilder<string, byte[]>(config).Build();
                    });
                    services.Configure<HeartbeatConfiguration>(configuration.GetSection(nameof(HeartbeatConfiguration)));
                    services.AddSingleton<HeartbeatService>();
                    services.AddSingleton<IHealthCheckPublisher>(sp => sp.GetRequiredService<HeartbeatService>());
                    services.AddHostedService(sp => sp.GetRequiredService<HeartbeatService>());
                    services.AddHostedService<SportsCreatedWorker>();
                    services.AddHostedService<WorkoutBookedWorker>();
                });
        }
    }
}
