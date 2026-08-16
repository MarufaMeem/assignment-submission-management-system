namespace AssignmentSystem.Api.Services.Interfaces;

/// <summary>
/// Every place in this codebase that needs "now" goes through this instead of
/// DateTime.UtcNow directly. Reason: submission-deadline tests (Phase 6) need
/// to simulate "5 minutes before deadline" and "5 minutes after deadline"
/// deterministically - that's impossible to do reliably against the real clock,
/// and flaky time-based tests are worse than no tests. Production uses
/// SystemDateTimeProvider (real UtcNow); tests use a fake that returns a fixed value.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
