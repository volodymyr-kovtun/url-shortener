namespace UrlShortener.Core;

public interface IStorageProvider<TEntity, in TKey>
{
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task<TEntity?> GetAsync(TKey key, CancellationToken ct = default);
}
