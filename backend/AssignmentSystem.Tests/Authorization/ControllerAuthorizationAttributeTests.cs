using System.Reflection;
using AssignmentSystem.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace AssignmentSystem.Tests.Authorization;

/// <summary>
/// IMPORTANT - SCOPE OF THESE TESTS:
/// These tests verify that the correct [Authorize(Roles = "...")] attribute is
/// physically present on each controller/action. They do NOT send real HTTP
/// requests through the ASP.NET Core pipeline (that would require a
/// WebApplicationFactory integration-test harness with a real or swapped-in
/// database and JWT issuing, which is a heavier piece of infrastructure -
/// planned for Phase 8 hardening once the project is confirmed to build).
///
/// What this DOES prove: "admin can manage users" / "teacher cannot manage
/// users" / "student cannot manage users" are enforced by attributes that are
/// actually on the right classes - a missing or misspelled [Authorize] here
/// would be caught immediately. What this does NOT prove: that ASP.NET Core's
/// authorization middleware correctly evaluates the attribute at runtime (that
/// is framework behavior, not this project's code, and is exactly what the
/// manual Swagger walkthrough in the README's "Testing" section confirms).
/// </summary>
public class ControllerAuthorizationAttributeTests
{
    [Fact]
    public void UsersController_RequiresAdminRole()
    {
        var attribute = typeof(UsersController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }

    [Fact]
    public void TeacherAssignmentsController_RequiresAdminRole()
    {
        var attribute = typeof(TeacherAssignmentsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }

    [Fact]
    public void ClassesController_WriteActions_RequireAdminRole()
    {
        var createMethod = typeof(ClassesController).GetMethod(nameof(ClassesController.Create))!;
        var updateMethod = typeof(ClassesController).GetMethod(nameof(ClassesController.Update))!;
        var deleteMethod = typeof(ClassesController).GetMethod(nameof(ClassesController.Delete))!;

        foreach (var method in new[] { createMethod, updateMethod, deleteMethod })
        {
            var attribute = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(attribute);
            Assert.Equal("Admin", attribute!.Roles);
        }
    }

    [Fact]
    public void ClassesController_ReadActions_RequireOnlyAuthentication_NotSpecificRole()
    {
        // Class-level [Authorize] (no Roles) means "any authenticated user" -
        // students and teachers must both be able to read classes/subjects.
        var classLevelAttribute = typeof(ClassesController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(classLevelAttribute);
        Assert.True(string.IsNullOrEmpty(classLevelAttribute!.Roles));
    }

    [Fact]
    public void AuthController_Login_IsAnonymous()
    {
        var loginMethod = typeof(AuthController).GetMethod(nameof(AuthController.Login))!;

        Assert.NotNull(loginMethod.GetCustomAttribute<AllowAnonymousAttribute>());
    }
}
