namespace SubTracker.Core.Domain.Interfaces
{
    public interface IGenericRepository<Entity> where Entity : class        
    {
        Task<Entity?> AddAsync(Entity entity);
        Task<List<Entity>?> AddRangeAsync(List<Entity> entities);
        Task DeleteAsync(int id);
        Task<List<Entity>> GetAllList();
        IQueryable<Entity> GetAllQuery();
        Task<Entity?> GetById(int id);
        Task<Entity?> UpdateAsync(int id, Entity entity);
        Task<List<Entity>> GetAllListWithInclude(List<string> entities);
        IQueryable<Entity> GetAllQueryWithInclude(List<string> entities);
    }
}