using System.Text.Json;
using JobBoardApi.Models;
using StackExchange.Redis;

namespace JobBoardApi.Services;

public class RedisService : IRedisService, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string JobViewsSortedSetKey = "jobs:views:ranking";
    private const string JobCacheKeyPrefix = "jobs:cache:";

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _db = _redis.GetDatabase();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

        public async Task<T?> GetCachedAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var redisValue = await _db.StringGetAsync(key);
        if (!redisValue.HasValue || redisValue.IsNullOrEmpty)
        {
            return default;
        }

        // Correção: Adicionado .ToString() para resolver a ambiguidade CS0121
        return JsonSerializer.Deserialize<T>(redisValue.ToString(), _jsonOptions);
    }

    public async Task SetCachedAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await _db.StringSetAsync(key, json, expiry);
    }

    public async Task RemoveCachedAsync(string key, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task IncrementJobViewAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        // Equivalent to ZINCRBY: increments the member score in the Sorted Set.
        await _db.SortedSetIncrementAsync(
            JobViewsSortedSetKey,
            jobId.ToString(),
            1.0);
    }

    public async Task<IReadOnlyList<JobRankingItem>> GetTopJobsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        // Equivalent to ZREVRANGE with WITHSCORES: from highest to lowest.
        var entries = await _db.SortedSetRangeByRankWithScoresAsync(
            JobViewsSortedSetKey,
            start: 0,
            stop: count - 1,
            order: Order.Descending);

        var result = new List<JobRankingItem>(entries.Length);
        foreach (var entry in entries)
        {
            if (Guid.TryParse(entry.Element.ToString(), out var jobId))
            {
                result.Add(new JobRankingItem(jobId, entry.Score));
            }
        }

        return result;
    }

    public static string GetJobCacheKey(Guid jobId) => $"{JobCacheKeyPrefix}{jobId}";

    public void Dispose()
    {
        if (_redis is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
