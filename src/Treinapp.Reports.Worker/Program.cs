using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using System;

using Treinapp.Common;

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
                    services.AddSingleton<CloudEventFormatter>(new JsonEventFormatter());

                    services.AddHostedService<SportsCreatedWorker>();
                });
        }
    }    
}
