
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using Treinapp.Common;
using Treinapp.Reports.API.Features.Reports;

namespace Treinapp.Reports.API
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
            services
                .AddHealthChecks()
                .AddMongoDb(Configuration.GetConnectionString(Constants.MongoConnectionKey));
            services.Configure<ForwardedHeadersOptions>(fwh =>
            {
                fwh.ForwardedHeaders = ForwardedHeaders.All;
            });
            services.AddGraphQLServer()
                .AddQueryType<ReportsQuery>()
                .AddMongoDbFiltering()
                .AddMongoDbProjections()
                .AddMongoDbSorting();
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
                return mongoClient.GetDatabase(Constants.MongoReportsDatabase);
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

            if (!Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                endpoints.MapHealthChecks("/health");
            });

            app.UseGraphQLGraphiQL();
        }
    }
}
