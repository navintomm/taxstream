using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaxStream.Application.Abstractions;
using TaxStream.Core.Interfaces;
using TaxStream.Infrastructure.AI;
using TaxStream.Infrastructure.Configuration;
using TaxStream.Infrastructure.Persistence;
using TaxStream.Infrastructure.Persistence.Repositories;
using TaxStream.Infrastructure.Security;
using TaxStream.Infrastructure.TaxCalculation;

namespace TaxStream.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core: Supports SQLite (for zero-setup dev) and SQL Server
        var provider = configuration["DatabaseProvider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=taxstream.db";

        // Defensive guard: if provider is Sqlite but the connection string was an old SQL Server string, fallback to sqlite
        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase) && connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = "Data Source=taxstream.db";
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
            else
            {
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaxRecordRepository, TaxRecordRepository>();
        services.AddScoped<IAiConversationRepository, AiConversationRepository>();

        // Tax Calculation
        services.AddScoped<ITaxCalculationService, SimpleTaxCalculationService>();

        // Security
        services.AddScoped<IPasswordHasher, AspNetPasswordHasher>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // AI Provider
        services.Configure<AzureOpenAIOptions>(configuration.GetSection(AzureOpenAIOptions.SectionName));
        services.AddScoped<IAiProviderService, AzureOpenAiService>();

        return services;
    }
}
