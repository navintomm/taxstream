using Microsoft.AspNetCore.Identity;
using TaxStream.Application.Abstractions;
using TaxStream.Core.Entities;

namespace TaxStream.Infrastructure.Security;

public class AspNetPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string hash)
        => _hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}
