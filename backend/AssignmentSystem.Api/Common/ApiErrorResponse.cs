namespace AssignmentSystem.Api.Common;

/// <summary>
/// Every error response from this API has this exact shape, regardless of
/// which layer produced it (validation, business rule, or unhandled exception).
/// A consistent shape means the frontend can write one error-handling path
/// instead of guessing the response format per endpoint.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>Machine-readable category, e.g. "NotFound", "Forbidden", "ValidationError".</summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>Human-readable message safe to display to the end user.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional field-level validation errors: { "Title": ["Title is required"] }.</summary>
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>Correlation id for cross-referencing server logs, omitted from client-facing detail otherwise.</summary>
    public string? TraceId { get; set; }
}
