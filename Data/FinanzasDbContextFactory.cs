using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinanzasPersonales.Api.Data
{
    /// <summary>
    /// Factory para crear el DbContext en tiempo de diseño (para migraciones).
    /// </summary>
    public class FinanzasDbContextFactory : IDesignTimeDbContextFactory<FinanzasDbContext>
    {
        public FinanzasDbContext CreateDbContext(string[] args)
        {
            // Cargar la configuración desde appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Configurar las opciones del DbContext
            var optionsBuilder = new DbContextOptionsBuilder<FinanzasDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString);

            return new FinanzasDbContext(optionsBuilder.Options);
        }
    }
}
