namespace TaxStream.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string fullName);
    DateTimeOffset GetTokenExpiry();
}
