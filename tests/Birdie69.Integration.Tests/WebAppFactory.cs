using Birdie69.Application.Common.Interfaces;
using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;
using Birdie69.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Testcontainers.PostgreSql;

namespace Birdie69.Integration.Tests;

/// <summary>
/// Spins up a real PostgreSQL container and replaces external dependencies
/// (Redis → in-memory cache, Strapi → configurable mock) for integration tests.
/// </summary>
public sealed class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("birdie69_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    /// <summary>
    /// Override to control what ICmsService returns in a specific test.
    /// Defaults to returning null (question not found).
    /// </summary>
    public Mock<ICmsService> CmsServiceMock { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Default: CMS returns null (no question scheduled)
        CmsServiceMock
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionDto?)null);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL DbContext with the Testcontainer connection
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Replace Redis with an in-memory distributed cache
            services.RemoveAll<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            services.AddDistributedMemoryCache();

            // Replace the real CmsService with the controllable mock
            services.RemoveAll<ICmsService>();
            services.AddScoped<ICmsService>(_ => CmsServiceMock.Object);
        });

        builder.UseEnvironment("Testing");
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
