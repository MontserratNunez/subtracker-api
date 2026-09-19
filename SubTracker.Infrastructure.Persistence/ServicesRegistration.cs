using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubTracker.Core.Domain.Interfaces;
using SubTracker.Infrastructure.Persistence.Contexts;
using SubTracker.Infrastructure.Persistence.Repositories;

namespace SubTracker.Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContext<SubTrackerContext>(opt =>
                opt.UseSqlServer(connectionString,
                    m => m.MigrationsAssembly(typeof(SubTrackerContext).Assembly.FullName)), ServiceLifetime.Transient);
            
            #endregion

            #region Repositories IOC
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISubscriptionCategoryRepository, SubscriptionCategoryRepository>();
            #endregion
        }
    }
}
