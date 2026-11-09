using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaxStream.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core tooling (dotnet ef migrations add / update) at design time.
/// Reads the connection string from WebAPI's appsettings.json or fallback.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        string? connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Try standard relative paths from either solution root or Infrastructure folder
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "src", "TaxStream.WebAPI", "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "TaxStream.WebAPI", "appsettings.json"),
                Path.Combine(AppContext.BaseDirectory, "appsettings.json")
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                        cs.TryGetProperty("DefaultConnection", out var dc))
                    {
                        connectionString = dc.GetString();
                        break;
                    }
                }
            }
        }

        connectionString ??= "Server=(localdb)\\mssqllocaldb;Database=TaxStreamDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}

