using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace LeaderboardAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add MongoDBConnection service
            services.AddScoped<MongoDBConnection>(mongoDBConnection => new MongoDBConnection(
                Configuration.GetValue<string>("MongoDb:ConnectionString"),
                Configuration.GetValue<string>("MongoDb:Database")));

            // Add RewardsCalculator service
            services.AddScoped<RewardsCalculator>();
            services.AddScoped<LeaderboardController>();

            // Add MVC service
            services.AddControllers();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use MVC
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
