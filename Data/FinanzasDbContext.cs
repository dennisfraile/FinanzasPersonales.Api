using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace FinanzasPersonales.Api.Data
{
    public class FinanzasDbContext : IdentityDbContext<IdentityUser>
    {
        public FinanzasDbContext(DbContextOptions<FinanzasDbContext> options) : base(options)
        {
        }

        // Mapea tus modelos a tablas en la BD
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Meta> Metas { get; set; }
    }
}
