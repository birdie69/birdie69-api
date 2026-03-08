using Birdie69.Application;
using Birdie69.Infrastructure;
using Birdie69.Infrastructure.Persistence;
using Birdie69.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// ── Authentication ────────────────────────────────────────────────────────────
// Non-Production (Development / Testing): accept any JWT without B2C validation.
// This lets Swagger work before Azure AD B2C is configured (Sprint 1) and
// allows integration tests to run without a real B2C tenant.
// Production: full Azure AD B2C validation is enforced.
if (!builder.Environment.IsProduction())
{
    // Shared dev signing key — used only to produce a parseable JWT for Swagger.
    // Never used in Production. Value does not need to be secret.
    var devKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes("birdie69-dev-only-key-not-used-in-prod!"));

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Disable all validation — we only need the token to be parseable.
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = devKey   // required so the handler has a key reference
            };

            // Root-cause fix: JwtSecurityTokenHandler (and JsonWebTokenHandler) both
            // throw when parsing non-JWT strings like "dev" — BEFORE SignatureValidator
            // is ever reached. Replace any non-JWT Bearer value with a minimal
            // self-signed dev JWT so the pipeline always gets a parseable token.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var raw = context.Request.Headers.Authorization.ToString();
                    if (raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = raw["Bearer ".Length..].Trim();
                        var handler = new JwtSecurityTokenHandler();
                        if (!handler.CanReadToken(value))
                        {
                            var devToken = new JwtSecurityToken(
                                claims: [new Claim(ClaimTypes.NameIdentifier, "dev-user")],
                                signingCredentials: new SigningCredentials(
                                    devKey, SecurityAlgorithms.HmacSha256));
                            context.Token = handler.WriteToken(devToken);
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        });
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
}

// ── Application + Infrastructure layers ──────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── API services ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "birdie69 API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Development: enter any value (e.g. 'dev') or a valid JWT. Production: enter a valid Azure AD B2C JWT."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Birdie69.Application.Common.Interfaces.ICurrentUser,
    Birdie69.Api.Services.CurrentUserService>();

var app = builder.Build();

// ── Database: auto-migrate on startup (non-Production only) ──────────────────
if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory in integration tests
public partial class Program { }
