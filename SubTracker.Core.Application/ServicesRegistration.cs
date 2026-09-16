using SubTracker.Core.Application.Interfaces;
using SubTracker.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SubTracker.Core.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Configurations
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });
            #endregion
            #region Services IOC
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            #endregion
        }
    }
}
