namespace AssignmentSystem.Api.Configuration;

/// <summary>
/// Bound from the "Jwt" section of configuration (appsettings.json + environment
/// variable overrides). Never hardcode Secret - it must come from .env locally
/// and a real secrets store in any deployed environment.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
