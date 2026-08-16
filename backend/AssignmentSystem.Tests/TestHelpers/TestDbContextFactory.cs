using AssignmentSystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Tests.TestHelpers;

public static class TestDbContextFactory
{
    /// <summary>
    /// Each call gets a uniquely-named in-memory database, so tests never leak
    /// state into one another even when run in parallel (xUnit's default).
    /// </summary>
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
