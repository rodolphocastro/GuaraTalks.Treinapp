using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using MongoDB.Driver;

using Treinapp.Common;

namespace Treinapp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AddMongoServices(services);
            AddKafkaServices(services);
            services
                .AddHealthChecks()
                .AddMongoDb(Configuration.GetConnectionString(Constants.MongoConnectionKey))
                .AddKafka(new ProducerConfig
                {
                    BootstrapServers = Configuration.GetConnectionString(Constants.KafkaBootstrapKey),
                });
            services.AddHttpContextAccessor();
            services.AddMediatR(GetType().Assembly);
            services.Configure<ForwardedHeadersOptions>(fwh =>
            {
                fwh.ForwardedHeaders = ForwardedHeaders.All;
            });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Treinapp.API", Version = "v1" });
            });
        }

        /// <summary>
        /// Adds the services requred for working with Kafka.
        /// This means registering IProducer<Null, string>.
        /// </summary>
        /// <param name="services"></param>
        private void AddKafkaServices(IServiceCollection services)
        {
            services.AddSingleton(c =>
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = Configuration.GetConnectionString(Constants.KafkaBootstrapKey),
                };
                return new ProducerBuilder<string, byte[]>(config).Build();
            });
            services.AddSingleton<CloudEventFormatter>(new JsonEventFormatter());
        }

        /// <summary>
        /// Adds the services required for working with MongoDB.
        /// This means registering MongoClient as a Singleton and IMongoDatabase as a Scoped.
        /// </summary>
        /// <param name="services"></param>
        private void AddMongoServices(IServiceCollection services)
        {
            services.AddSingleton(_ =>
            {
                return new MongoClient(Configuration.GetConnectionString(Constants.MongoConnectionKey));
            });
            services.AddScoped(sp =>
            {
                MongoClient mongoClient = sp.GetRequiredService<MongoClient>();
                return mongoClient.GetDatabase(Constants.MongoCrudDatabase);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Treinapp.API v1"));

            if (!Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
