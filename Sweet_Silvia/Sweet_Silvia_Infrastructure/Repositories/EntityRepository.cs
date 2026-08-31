using Microsoft.EntityFrameworkCore;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Infrastructure.Repositories;

internal sealed class EntityRepository<TEntity>(ApplicationDbContext context)
    : IRepository<TEntity>
    where TEntity : Entity
{
    public IQueryable<TEntity> Query() => context.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Set<TEntity>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public void Add(TEntity entity)
    {
        context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
    }
}

