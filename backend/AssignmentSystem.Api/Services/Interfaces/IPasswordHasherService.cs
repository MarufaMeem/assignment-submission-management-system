namespace AssignmentSystem.Api.Services.Interfaces;

/// <summary>
/// Thin seam over ASP.NET Core's built-in PasswordHasher. Exists purely for
/// testability: AuthServiceTests mocks this so login-logic tests don't pay the
/// cost of real PBKDF2 hashing and don't need a real User instance just to hash
/// a string. The hashing algorithm itself is verified separately, once, in
/// PasswordHasherServiceTests against the real implementation (no mocks).
/// </summary>
public interface IPasswordHasherService
{
    string HashPassword(string plainPassword);

    bool VerifyPassword(string hashedPassword, string providedPassword);
}
