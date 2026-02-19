using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Tests.TestInfrastructure;

public static class DbContextFactory
{
    public static ApplicationDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .EnableSensitiveDataLogging()
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();
        
        return db;
    }
}
