namespace Pekspro.RadioStorm.CacheDatabase;

public interface ICachePrefetcher
{
    Task PrefetchAsync(CancellationToken cancellationToken);
}
