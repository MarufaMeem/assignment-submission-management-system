using AssignmentSystem.Api.DTOs.Users;

namespace AssignmentSystem.Api.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserSummaryDto User { get; set; } = null!;
}
