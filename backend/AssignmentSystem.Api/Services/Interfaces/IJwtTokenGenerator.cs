using AssignmentSystem.Api.Entities;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Returns the signed JWT string and its UTC expiry.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
