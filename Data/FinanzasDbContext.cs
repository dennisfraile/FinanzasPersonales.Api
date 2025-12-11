using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace FinanzasPersonales.Api.Data
{
    public class FinanzasDbContext : IdentityDbContext<IdentityUser>
    {
        // AÑADE ESTE MÉTODO COMPLETO:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ¡MUY IMPORTANTE! Debe ir primero para Identity.

            // Le decimos a EF Core cómo manejar la relación Gasto -> Categoria
            modelBuilder.Entity<Gasto>()
                .HasOne(g => g.Categoria) // Un Gasto tiene una Categoria
                .WithMany() // Una Categoria puede tener muchos Gastos (pero no lo definimos en Categoria)
                .HasForeignKey(g => g.CategoriaId) // La llave es CategoriaId
                .OnDelete(DeleteBehavior.Restrict); // ¡LA LÍNEA CLAVE! Cambia CASCADE por RESTRICT.

            // Hacemos lo mismo para Ingreso -> Categoria
            modelBuilder.Entity<Ingreso>()
                .HasOne(i => i.Categoria)
                .WithMany()
                .HasForeignKey(i => i.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict); // ¡LA LÍNEA CLAVE!

            // Configuración para Presupuesto -> Categoria
            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

        }
        public FinanzasDbContext(DbContextOptions<FinanzasDbContext> options) : base(options)
        {
        }

        // Mapea tus modelos a tablas en la BD
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Meta> Metas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<ConfiguracionUsuario> ConfiguracionesUsuario { get; set; }
    }
}
