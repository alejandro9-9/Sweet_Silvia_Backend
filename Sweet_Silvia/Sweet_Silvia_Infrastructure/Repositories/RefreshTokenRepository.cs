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
        var currentDate = DateTimeOffset.UtcNow;
        return Context.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId && token.RevokedAt == null && token.ExpiresAt > currentDate)
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var currentDate = DateTimeOffset.UtcNow;
        return Context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null && token.ExpiresAt > currentDate)
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> DeleteInactiveAsync(DateTimeOffset currentDate, CancellationToken cancellationToken = default)
    {
        return Context.RefreshTokens
            .Where(token => token.RevokedAt != null || token.ExpiresAt <= currentDate)
            .ExecuteDeleteAsync(cancellationToken);
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
