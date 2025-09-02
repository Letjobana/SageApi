using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SageService.Domain.Interfaces;
using SageService.Infrastructure.Repositories;
using SageService.Infrastructure.Sage;


namespace SageService.Infrastructure
{
    /// <summary>
    /// Wiring: SQL repository + Sage HTTP client.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddSingleton(new SqlConnectionFactory(connectionString!));
            services.AddScoped<ILmsRepository, LmsRepository>();
            services.AddScoped<ISageClient, SageClient>();
            services.Configure<SageApiOptions>(configuration.GetSection("Sage"));
            return services;
        }
    }
}