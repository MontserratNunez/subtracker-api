using SubTracker.Core.Domain.Entities;
using SubTracker.Core.Domain.Interfaces;
using SubTracker.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubTracker.Infrastructure.Persistence.Repositories
{
    public class SubscriptionCategoryRepository : GenericRepository<SubscriptionCategory>, ISubscriptionCategoryRepository
    {
        private readonly SubTrackerContext _dbContext;

        public SubscriptionCategoryRepository(SubTrackerContext context) : base(context)
        {
            _dbContext = context;
        }
    }
}
