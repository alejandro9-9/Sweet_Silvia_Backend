using Microsoft.EntityFrameworkCore;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Infrastructure.Repositories;

internal abstract class Repository<TEntity>(ApplicationDbContext context)
    where TEntity : Entity
{
    protected readonly ApplicationDbContext Context = context;

    protected Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Set<TEntity>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    protected void AddEntity(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
    }

    protected void UpdateEntity(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }
}
