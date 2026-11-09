using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxStream.Core.Entities;
using TaxStream.Infrastructure.Persistence;
using TaxStream.Infrastructure.Persistence.Repositories;
using Xunit;

namespace TaxStream.Infrastructure.Tests;

/// <summary>
/// Tests EF Core repository behavior using an in-memory database.
/// For CI environments where SQL Server is not available.
/// </summary>
public class TaxRecordRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaxRecordRepository _sut;

    public TaxRecordRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // fresh DB per test
            .Options;

        _context = new AppDbContext(options);
        _sut = new TaxRecordRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsRecord()
    {
        var userId = Guid.NewGuid();
        var record = new TaxRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaxYear = 2025,
            GrossIncome = 100_000m,
            FilingStatus = "Single",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _sut.AddAsync(record);
        await _sut.SaveChangesAsync();

        var fetched = await _sut.GetByIdAsync(record.Id);
        fetched.Should().NotBeNull();
        fetched!.TaxYear.Should().Be(2025);
        fetched.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByUserId_ReturnsOnlyUserRecords()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        await _sut.AddAsync(new TaxRecord { Id = Guid.NewGuid(), UserId = userId1, TaxYear = 2024, FilingStatus = "Single", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await _sut.AddAsync(new TaxRecord { Id = Guid.NewGuid(), UserId = userId2, TaxYear = 2024, FilingStatus = "Single", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await _sut.SaveChangesAsync();

        var records = await _sut.GetByUserIdAsync(userId1);
        records.Should().HaveCount(1);
        records[0].UserId.Should().Be(userId1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRecord()
    {
        var record = new TaxRecord
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TaxYear = 2023,
            FilingStatus = "Single",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _sut.AddAsync(record);
        await _sut.SaveChangesAsync();

        await _sut.DeleteAsync(record);
        await _sut.SaveChangesAsync();

        var fetched = await _sut.GetByIdAsync(record.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithDeductions_LoadsDeductionsEagerly()
    {
        var record = new TaxRecord
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TaxYear = 2025,
            FilingStatus = "Single",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Deductions = new List<Deduction>
            {
                new() { Id = Guid.NewGuid(), Category = "Retirement", Amount = 10_000m },
                new() { Id = Guid.NewGuid(), Category = "Medical", Amount = 5_000m }
            }
        };

        await _sut.AddAsync(record);
        await _sut.SaveChangesAsync();

        var fetched = await _sut.GetByIdWithDeductionsAsync(record.Id);
        fetched.Should().NotBeNull();
        fetched!.Deductions.Should().HaveCount(2);
    }

    public void Dispose() => _context.Dispose();
}

