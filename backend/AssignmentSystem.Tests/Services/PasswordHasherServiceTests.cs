using AssignmentSystem.Api.Services.Implementations;
using Xunit;

namespace AssignmentSystem.Tests.Services;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _sut = new();

    // Business rule protected: "passwords must never be stored as plain text" -
    // confirms hashing actually transforms the input, not a no-op passthrough.
    [Fact]
    public void HashPassword_ReturnsValueDifferentFromPlainInput()
    {
        var hash = _sut.HashPassword("MyP@ssw0rd");

        Assert.NotEqual("MyP@ssw0rd", hash);
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    // Business rule protected: the correct password must verify successfully
    // against its own hash (round-trip correctness).
    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("MyP@ssw0rd");

        Assert.True(_sut.VerifyPassword(hash, "MyP@ssw0rd"));
    }

    // Business rule protected: an incorrect password must never verify against
    // someone else's hash - this is the core guarantee the whole login flow relies on.
    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("MyP@ssw0rd");

        Assert.False(_sut.VerifyPassword(hash, "SomethingElse"));
    }
}
