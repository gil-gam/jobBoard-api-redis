using JobBoardApi.Models;

namespace JobBoardApi.Services;

public interface IRedisService
{
    /// <summary>
    /// Retrieves a cached value using StringGet and deserializes it into the specified type.
    /// </summary>
    Task<T?> GetCachedAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes the provided value as JSON and stores it in the cache using StringSet, with an optional expiration time.
    /// </summary>
    Task SetCachedAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified key from the cache.
    /// </summary>
    Task RemoveCachedAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the job's score by one in the views sorted set using ZIncrBy.
    /// </summary>
    Task IncrementJobViewAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the ranking of the most viewed jobs using ZRevRange with scores.
    /// </summary>
    Task<IReadOnlyList<JobRankingItem>> GetTopJobsAsync(int count = 10, CancellationToken cancellationToken = default);
}

public record JobRankingItem(Guid JobId, double Views);