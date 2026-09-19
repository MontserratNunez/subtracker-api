using Microsoft.EntityFrameworkCore;
using SubTracker.Core.Application.Interfaces;
using SubTracker.Core.Domain.Entities;
using SubTracker.Core.Domain.Interfaces;
using SubTracker.Infrastructure.Persistence.Contexts;

namespace SubTracker.Infrastructure.Persistence.Repositories
{
    public class SubscriptionCategoryRepository : GenericRepository<SubscriptionCategory>, ISubscriptionCategoryRepository
    {
        private readonly SubTrackerContext _context;

        public SubscriptionCategoryRepository(SubTrackerContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionCategory>> GetByUserIdAsync(string userId)
        {
            return await _context.Set<SubscriptionCategory>()
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<SubscriptionCategory?> GetByIdAndUserIdAsync(int id, string userId)
        {
            return await _context.Set<SubscriptionCategory>()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        }
    }
}