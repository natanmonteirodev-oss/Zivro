namespace Zivro.Domain.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Base repository interface for generic CRUD operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
