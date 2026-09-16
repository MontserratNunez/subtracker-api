using SubTracker.Core.Application.Interfaces;
using SubTracker.Core.Domain.Settings;
using SubTracker.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SubTracker.Infrastructure.Shared
{
    public static class ServicesRegistration
    {
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            #endregion
        }
    }
}
