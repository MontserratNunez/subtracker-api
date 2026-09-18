using SubTracker.Core.Domain.Entities;
using SubTracker.Core.Domain.Interfaces;
using SubTracker.Infrastructure.Persistence.Contexts;

namespace SubTracker.Infrastructure.Persistence.Repositories
{
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        private readonly SubTrackerContext _dbContext;

        public SubscriptionRepository(SubTrackerContext context) : base(context)
        {
            _dbContext = context;
        }
    }
}