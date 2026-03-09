using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;

namespace FinanzasPersonales.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static FinanzasDbContext Create()
        {
            var options = new DbContextOptionsBuilder<FinanzasDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new FinanzasDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
