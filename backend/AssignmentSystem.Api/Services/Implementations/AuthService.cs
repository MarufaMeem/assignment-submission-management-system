using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Auth;
using AssignmentSystem.Api.DTOs.Users;
using AssignmentSystem.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services.Implementations;

public class AuthService : IAuthService
{
    private const string GenericLoginFailureMessage = "Invalid email or password.";

    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext db,
        IPasswordHasherService passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ILogger<AuthService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, ct);

        // Deliberately the SAME exception + message for all three failure cases below:
        // unknown email, wrong password, and a correctly-authenticated-but-deactivated
        // account. If "inactive" returned a different message/status than "wrong
        // password", an attacker (or a curious ex-employee) could probe emails to
        // discover which accounts exist and which are merely deactivated. This is
        // the business rule "inactive/nonexistent user behavior" from the test list -
        // the correct behavior IS indistinguishability, not just "handle it somehow".
        if (user is null)
        {
            _logger.LogWarning("Login failed: no user found for supplied email.");
            throw new UnauthorizedAppException(GenericLoginFailureMessage);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: account {UserId} is deactivated.", user.Id);
            throw new UnauthorizedAppException(GenericLoginFailureMessage);
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            _logger.LogWarning("Login failed: incorrect password for account {UserId}.", user.Id);
            throw new UnauthorizedAppException(GenericLoginFailureMessage);
        }

        var (token, expiresAtUtc) = _tokenGenerator.GenerateToken(user);

        _logger.LogInformation("User {UserId} ({Role}) logged in successfully.", user.Id, user.Role);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = new UserSummaryDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                ClassId = user.ClassId
            }
        };
    }
}
