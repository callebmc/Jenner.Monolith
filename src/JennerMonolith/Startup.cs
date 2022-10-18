using JennerMonolith.Comum;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System;

namespace JennerMonolith
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddMediatR(GetType().Assembly);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JennerMonolith.API", Version = "v1" });
            });


            AddMongoServices(services);
        }


        private void AddMongoServices(IServiceCollection services)
        {
            services.AddSingleton(_ =>
            {
                MongoClientSettings settings = MongoClientSettings.FromConnectionString(Configuration.GetConnectionString(Constants.MongoConnectionString));
                settings.WaitQueueSize = int.MaxValue;
                settings.WaitQueueTimeout = new TimeSpan(0, 4, 0);
                settings.MinConnectionPoolSize = 0;
                settings.MaxConnectionPoolSize = 1000;

                return new MongoClient(settings);
                //return new MongoClient(Configuration.GetConnectionString(Constants.MongoConnectionString));
            });
            services.AddSingleton(sp =>
            {
                MongoClient mongoClient = sp.GetRequiredService<MongoClient>();
                return mongoClient.GetDatabase(Constants.MongoAgendamentoDatabase);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JennerMonolith.API v1"));

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
