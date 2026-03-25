namespace Zivro.Domain.Entities;

/// <summary>
/// Base entity class for all domain entities.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Date when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates if the entity is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Equality comparison based on Id.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Hash code based on Id.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
