using JobBoardApi.Models;
using JobBoardApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobBoardApi.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v1/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IRedisService _redisService;
    private readonly ILogger<JobsController> _logger;

    private static readonly Dictionary<Guid, JobOffer> InMemoryJobs = new()
    {
        [Guid.Parse("11111111-1111-1111-1111-111111111111")] = new JobOffer
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Senior .NET Developer",
            Company = "Tech Corp",
            Location = "Sao Paulo, BR",
            Description = "Work with .NET 10, Redis, and distributed architecture.",
            Salary = 18000m,
            PostedAt = DateTime.UtcNow.AddDays(-2)
        },
        [Guid.Parse("22222222-2222-2222-2222-222222222222")] = new JobOffer
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Data Engineer",
            Company = "DataLab",
            Location = "Remote",
            Description = "Pipelines with Spark, Redis, and Kafka.",
            Salary = 15000m,
            PostedAt = DateTime.UtcNow.AddDays(-5)
        },
        [Guid.Parse("33333333-3333-3333-3333-333333333333")] = new JobOffer
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "DevOps Engineer",
            Company = "CloudOps",
            Location = "Belo Horizonte, BR",
            Description = "Kubernetes, Terraform, and CI/CD.",
            Salary = 14000m,
            PostedAt = DateTime.UtcNow.AddDays(-1)
        }
    };

    public JobsController(IRedisService redisService, ILogger<JobsController> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobOffer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobOffer>> GetJob(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisService.GetJobCacheKey(id);

        var cached = await _redisService.GetCachedAsync<JobOffer>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT for job {JobId}", id);
            await _redisService.IncrementJobViewAsync(id, cancellationToken);
            return Ok(cached);
        }

        _logger.LogInformation("Cache MISS for job {JobId}", id);

        if (!InMemoryJobs.TryGetValue(id, out var job))
        {
            return NotFound();
        }

        await _redisService.SetCachedAsync(cacheKey, job, TimeSpan.FromMinutes(5), cancellationToken);
        await _redisService.IncrementJobViewAsync(id, cancellationToken);

        return Ok(job);
    }

    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyList<JobRankingItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JobRankingItem>>> GetTopJobs(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0) count = 10;
        if (count > 100) count = 100;

        var ranking = await _redisService.GetTopJobsAsync(count, cancellationToken);
        return Ok(ranking);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobOffer>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<JobOffer>> GetAll()
    {
        return Ok(InMemoryJobs.Values);
    }
}