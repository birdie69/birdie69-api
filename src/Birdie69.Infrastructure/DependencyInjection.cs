using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Interfaces;
using Birdie69.Infrastructure.Cms;
using Birdie69.Infrastructure.Persistence;
using Birdie69.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Birdie69.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database (EF Core + Npgsql) ───────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICoupleRepository, CoupleRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();

        // ── Distributed cache (Redis → memory fallback for local dev) ─────────
        var redisConnStr = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnStr))
            services.AddStackExchangeRedisCache(o => o.Configuration = redisConnStr);
        else
            services.AddDistributedMemoryCache();

        // ── Strapi CMS client (typed HttpClient + scoped service) ─────────────
        services.AddHttpClient<ICmsService, CmsService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["Strapi:BaseUrl"] ?? "http://localhost:1337";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", config["Strapi:ReadToken"] ?? string.Empty);
        });

        return services;
    }
}
