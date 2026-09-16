using Microsoft.EntityFrameworkCore;
using SubTracker.Core.Domain.Interfaces;
using SubTracker.Infrastructure.Persistence.Contexts;

namespace SubTracker.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<Entity> : IGenericRepository<Entity>
        where Entity : class        
    {
        private readonly SubTrackerContext _context;

        public GenericRepository(SubTrackerContext context)
        {
            _context = context;
        }
        public virtual async Task<Entity?> AddAsync(Entity entity)
        {
            await _context.Set<Entity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public virtual async Task<List<Entity>?> AddRangeAsync(List<Entity> entities)
        {
            await _context.Set<Entity>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }
        public virtual async Task<Entity?> UpdateAsync(int id, Entity entity)
        {
            var entry = await _context.Set<Entity>().FindAsync(id);

            if (entry != null)
            {
                _context.Entry(entry).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return entry;
            }

            return null;
        }
        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<Entity>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<Entity>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public virtual async Task<List<Entity>> GetAllList()
        {
            return await _context.Set<Entity>().ToListAsync();
        }

        public virtual async Task<List<Entity>> GetAllListWithInclude(List<string> entities)
        {
            var query = _context.Set<Entity>().AsQueryable();

            foreach(var entity in entities)
            {
                query = query.Include(entity);
            }

            return await query.ToListAsync(); 
        }    
        public virtual async Task<Entity?> GetById(int id)
        {
            return await _context.Set<Entity>().FindAsync(id);
        }
        public virtual IQueryable<Entity> GetAllQuery()
        {
            return _context.Set<Entity>().AsQueryable();
        }
        public virtual IQueryable<Entity> GetAllQueryWithInclude(List<string> entities)
        {
            var query = _context.Set<Entity>().AsQueryable();

            foreach (var entity in entities)
            {
                query = query.Include(entity);
            }

            return query;
        }
    }
}