namespace AssignmentSystem.Api.Common;

/// <summary>
/// Base type for exceptions that the ExceptionHandlingMiddleware knows how to
/// translate into a specific HTTP status code + consistent error body.
/// Using typed exceptions (rather than throwing generic Exception or manually
/// returning StatusCode(...) from every service method) keeps services free of
/// HTTP concerns entirely - a service should not know what an "HTTP 403" is,
/// it should just say "this is forbidden" and let the web layer translate it.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

/// <summary>Resource does not exist, OR (deliberately) should appear not to exist
/// to this caller - see the 404-vs-403 decision in the Phase 1 authorization matrix
/// for draft assignments and out-of-class assignments viewed by students.</summary>
public class NotFoundAppException : AppException
{
    public NotFoundAppException(string message) : base(message) { }
}

/// <summary>Caller is authenticated but not allowed to perform this action
/// on this specific resource (role is correct but ownership/relationship isn't).</summary>
public class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message) : base(message) { }
}

/// <summary>Authentication itself failed - bad credentials, unknown user, or an
/// inactive account. Deliberately uses ONE generic message across all three
/// causes (see AuthService) so the response never reveals which case applied -
/// that would let an attacker enumerate valid emails or detect deactivated accounts.</summary>
public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(message) { }
}

/// <summary>Input failed a business validation rule (e.g. marks &gt; maxMarks).
/// Distinct from FluentValidation's request-shape validation, which runs earlier.</summary>
public class ValidationAppException : AppException
{
    public ValidationAppException(string message) : base(message) { }
}

/// <summary>Request conflicts with current state (e.g. submitting to an unpublished
/// assignment, submitting after a hard deadline, duplicate unique constraint).</summary>
public class ConflictAppException : AppException
{
    public ConflictAppException(string message) : base(message) { }
}
