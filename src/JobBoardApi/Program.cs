using JobBoardApi.Services;
using StackExchange.Redis;
using Scalar.AspNetCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var redisConfiguration = builder.Configuration.GetSection("Redis")["Configuration"]
    ?? throw new InvalidOperationException("Configuration 'Redis:Configuration' not found.");

var redis = ConnectionMultiplexer.Connect(redisConfiguration);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddControllers();

// OpenAPI document registered as "v1" with a document transformer
// that enriches the generated JSON with API metadata (Balta.io best practice).
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "JobBoardApi",
            Description = "Job board API with Redis caching and view ranking using sorted sets.",
            Version = "v1",
            Contact = new OpenApiContact
            {
                Name = "JobBoardApi Maintainer",
                Url = new Uri("https://github.com/gil-gam/JobBoardApi")
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };

        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Serve the OpenAPI JSON decoupled from the default endpoint.
    app.MapOpenApi("/docs/{documentName}.json");

    // Serve Scalar API reference consuming the generated OpenAPI JSON.
    app.MapScalarApiReference("/scalar", options =>
{
    // Tells Scalar where to find the OpenAPI JSON files
    options.WithOpenApiRoutePattern("/docs/{documentName}.json");
    // Registers the available documents in the UI dropdown
    options.AddDocument("v1", "JobBoard API - v1");
});
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
