using System.Linq.Expressions;

namespace Sweet_Silvia_Domain.Abstractions;

public interface IRepository<TEntity>
    where TEntity : Entity
{
    IQueryable<TEntity> Query();

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(TEntity entity);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}

