using AssignmentSystem.Api.DTOs.Auth;

namespace AssignmentSystem.Api.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Throws UnauthorizedAppException for ANY failure case (unknown email,
    /// wrong password, or inactive account) with an identical message - see
    /// AuthService for why these three cases must not be distinguishable to the caller.
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
}
