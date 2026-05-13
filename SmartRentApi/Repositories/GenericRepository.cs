using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartRentApi.Data;

namespace SmartRentApi.Repositories
{
    /// <summary>
    /// Generic repository implementation for standard CRUD operations.
    /// Works with ApplicationDbContext to manage any entity type.
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns an IQueryable for building complex queries with Include(), Where(), etc.
        /// </summary>
        public IQueryable<T> GetAll()
        {
            return _context.Set<T>().AsQueryable();
        }

        /// <summary>
        /// Find an entity by its primary key(s)
        /// </summary>
        public async Task<T?> GetByIdAsync(params object[] keyValues)
        {
            return await _context.Set<T>().FindAsync(keyValues);
        }

        /// <summary>
        /// Add a new entity (marks it for insertion)
        /// </summary>
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        /// <summary>
        /// Update an existing entity (marks it for update)
        /// </summary>
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        /// <summary>
        /// Delete an entity (marks it for deletion)
        /// </summary>
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        /// <summary>
        /// Save all pending changes to the database
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
