using Microsoft.EntityFrameworkCore;
using Sweet_Silvia_Domain.RefreshToken;

namespace Sweet_Silvia_Infrastructure.Repositories;

internal sealed class RefreshTokenRepository(ApplicationDbContext context)
    : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return FindByIdAsync(id, cancellationToken);
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return Context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    public Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Context.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId)
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(RefreshToken token)
    {
        AddEntity(token);
    }

    public void Update(RefreshToken token)
    {
        UpdateEntity(token);
    }
}
