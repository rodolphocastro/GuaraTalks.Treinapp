using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddHttpContextAccessor();
            services.AddMediatR(GetType().Assembly);
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
                var mongoClient = sp.GetRequiredService<MongoClient>();
                return mongoClient.GetDatabase(Constants.MongoDatabase);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Treinapp.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
