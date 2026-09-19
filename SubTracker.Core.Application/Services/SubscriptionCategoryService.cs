using SubTracker.Core.Application.Dtos.SubscriptionCategory;
using SubTracker.Core.Application.Interfaces;
using SubTracker.Core.Domain.Entities;
using SubTracker.Core.Domain.Interfaces;

namespace SubTracker.Core.Application.Services
{
    public class SubscriptionCategoryService : ISubscriptionCategoryService
    {
        private readonly ISubscriptionCategoryRepository _categoryRepository;

        public SubscriptionCategoryService(ISubscriptionCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<SubscriptionCategoryDto>> GetAllByUserAsync(string userId)
        {
            var categories = await _categoryRepository.GetByUserIdAsync(userId);
            return categories.Select(c => new SubscriptionCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<SubscriptionCategoryDto?> GetByIdAsync(int id, string userId)
        {
            var category = await _categoryRepository.GetByIdAndUserIdAsync(id, userId);
            if (category == null) return null;

            return new SubscriptionCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive
            };
        }

        public async Task<SubscriptionCategoryDto> CreateAsync(SaveSubscriptionCategoryDto dto, string userId)
        {
            var category = new SubscriptionCategory
            {
                Name = dto.Name,
                UserId = userId,
                IsActive = true,
                IsDeleted = false
            };

            var created = await _categoryRepository.AddAsync(category);

            return new SubscriptionCategoryDto
            {
                Id = created!.Id,
                Name = created.Name,
                IsActive = created.IsActive
            };
        }

        public async Task<SubscriptionCategoryDto?> UpdateAsync(int id, SaveSubscriptionCategoryDto dto, string userId)
        {
            var category = await _categoryRepository.GetByIdAndUserIdAsync(id, userId);
            if (category == null) return null;

            category.Name = dto.Name;

            var updated = await _categoryRepository.UpdateAsync(id, category);

            return new SubscriptionCategoryDto
            {
                Id = updated!.Id,
                Name = updated.Name,
                IsActive = updated.IsActive
            };
        }

        public async Task<bool> ToggleActiveAsync(int id, string userId)
        {
            var category = await _categoryRepository.GetByIdAndUserIdAsync(id, userId);
            if (category == null) return false;

            category.IsActive = !category.IsActive;
            await _categoryRepository.UpdateAsync(id, category);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var category = await _categoryRepository.GetByIdAndUserIdAsync(id, userId);
            if (category == null) return false;

            category.IsDeleted = true;
            await _categoryRepository.UpdateAsync(id, category);
            return true;
        }
    }
}