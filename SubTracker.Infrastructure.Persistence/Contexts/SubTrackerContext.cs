using Microsoft.EntityFrameworkCore;
using SubTracker.Core.Domain.Entities;
using System.Reflection;

namespace SubTracker.Infrastructure.Persistence.Contexts
{
    public class SubTrackerContext : DbContext
    {
        public SubTrackerContext(DbContextOptions<SubTrackerContext> options) : base(options) { }

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionCategory> SubscriptionCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
