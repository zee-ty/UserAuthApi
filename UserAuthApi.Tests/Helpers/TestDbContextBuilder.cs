using Microsoft.EntityFrameworkCore;
using UserAuthApi.Data;

namespace UserAuthApi.Tests.Helpers;

public static class TestDbContextBuilder
{
    public static AppDbContext CreateInMemory(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
