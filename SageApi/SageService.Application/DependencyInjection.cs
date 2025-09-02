using Microsoft.Extensions.DependencyInjection;
using SageService.Domain.Interfaces;

namespace SageService.Application
{
    /// <summary>
    /// Application-layer DI helpers. Keeps Program.cs clean.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // App services
            services.AddScoped<ICourseInvoiceJobService, CourseInvoiceJobService>();
            return services;
        }
    }
}
