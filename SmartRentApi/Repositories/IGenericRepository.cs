using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRentApi.Repositories
{
    /// <summary>
    /// Generic repository interface for standard CRUD operations.
    /// Provides a reusable contract for all entity repositories.
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Returns an IQueryable for querying entities (supports Include, Where, etc.)
        /// </summary>
        IQueryable<T> GetAll();

        /// <summary>
        /// Retrieve a single entity by primary key(s)
        /// </summary>
        Task<T?> GetByIdAsync(params object[] keyValues);

        /// <summary>
        /// Add a new entity to the repository
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Update an existing entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Delete an entity from the repository
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Persist all changes to the database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
