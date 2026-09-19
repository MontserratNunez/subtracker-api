using SubTracker.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubTracker.Core.Domain.Interfaces
{
    public interface ISubscriptionCategoryRepository : IGenericRepository<SubscriptionCategory>
    {
        Task<List<SubscriptionCategory>> GetByUserIdAsync(string userId);
        Task<SubscriptionCategory?> GetByIdAndUserIdAsync(int id, string userId);
    }
}
