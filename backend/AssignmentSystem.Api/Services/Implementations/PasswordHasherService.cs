using AssignmentSystem.Api.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AssignmentSystem.Api.Services.Implementations;

public class PasswordHasherService : IPasswordHasherService
{
    // PasswordHasher<TUser> is generic for extensibility only - its default
    // implementation does not actually read any property off the TUser
    // instance, so passing null for that parameter is safe and avoids forcing
    // callers to construct a throwaway User just to hash a string.
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string plainPassword)
    {
        return _hasher.HashPassword(null!, plainPassword);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success
                       or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
