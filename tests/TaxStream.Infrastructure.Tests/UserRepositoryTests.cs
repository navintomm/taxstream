using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxStream.Core.Entities;
using TaxStream.Infrastructure.Persistence;
using TaxStream.Infrastructure.Persistence.Repositories;
using Xunit;

namespace TaxStream.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new UserRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ThenGetByEmail_ReturnsUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Asha Nair",
            Email = "asha@example.com",
            NormalizedEmail = "ASHA@EXAMPLE.COM",
            PasswordHash = "hashed_password",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _sut.AddAsync(user);
        await _sut.SaveChangesAsync();

        var found = await _sut.GetByEmailAsync("ASHA@EXAMPLE.COM");
        found.Should().NotBeNull();
        found!.FullName.Should().Be("Asha Nair");
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test",
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM",
            PasswordHash = "hash",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _sut.AddAsync(user);
        await _sut.SaveChangesAsync();

        var exists = await _sut.EmailExistsAsync("TEST@TEST.COM");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
    {
        var exists = await _sut.EmailExistsAsync("NOBODY@EXAMPLE.COM");
        exists.Should().BeFalse();
    }

    public void Dispose() => _context.Dispose();
}

