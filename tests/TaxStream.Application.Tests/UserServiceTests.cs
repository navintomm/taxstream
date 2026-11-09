using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaxStream.Application.Abstractions;
using TaxStream.Application.DTOs.Auth;
using TaxStream.Application.Services;
using TaxStream.Core.Entities;
using TaxStream.Core.Exceptions;
using TaxStream.Core.Interfaces;
using Xunit;

namespace TaxStream.Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _userRepoMock.Object,
            _hasherMock.Object,
            _jwtMock.Object,
            NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Test", Email = "test@test.com", Password = "Password1!"
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_ReturnsUserDto()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), default)).ReturnsAsync((User u, CancellationToken _) => u);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

        var result = await _sut.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Asha Nair", Email = "asha@example.com", Password = "Password1!"
        });

        result.Email.Should().Be("asha@example.com");
        result.FullName.Should().Be("Asha Nair");
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsDomainException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
                     .ReturnsAsync(new User { PasswordHash = "hash" });
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var act = () => _sut.LoginAsync(new LoginRequestDto { Email = "a@b.com", Password = "wrong" });

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Invalid*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", FullName = "Test", PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns("jwt-token");
        _jwtMock.Setup(j => j.GetTokenExpiry()).Returns(DateTimeOffset.UtcNow.AddHours(1));

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "a@b.com", Password = "correct" });

        result.AccessToken.Should().Be("jwt-token");
        result.TokenType.Should().Be("Bearer");
    }
}
