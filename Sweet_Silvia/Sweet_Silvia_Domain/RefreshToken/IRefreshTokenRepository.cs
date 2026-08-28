namespace Sweet_Silvia_Domain.RefreshToken;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(RefreshToken token);

    void Update(RefreshToken token);
}
