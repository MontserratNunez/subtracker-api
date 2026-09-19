using SubTracker.Core.Application.Dtos.SubscriptionCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubTracker.Core.Application.Interfaces
{
    public interface ISubscriptionCategoryService
    {
        Task<List<SubscriptionCategoryDto>> GetAllByUserAsync(string userId);
        Task<SubscriptionCategoryDto?> GetByIdAsync(int id, string userId);
        Task<SubscriptionCategoryDto> CreateAsync(SaveSubscriptionCategoryDto dto, string userId);
        Task<SubscriptionCategoryDto?> UpdateAsync(int id, SaveSubscriptionCategoryDto dto, string userId);
        Task<bool> ToggleActiveAsync(int id, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
