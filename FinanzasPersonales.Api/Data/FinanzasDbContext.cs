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

            // Configuración para Transferencia -> Cuenta (dos relaciones)
            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.CuentaOrigen)
                .WithMany(c => c.TransferenciasOrigen)
                .HasForeignKey(t => t.CuentaOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.CuentaDestino)
                .WithMany(c => c.TransferenciasDestino)
                .HasForeignKey(t => t.CuentaDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración GastoCompartido -> Categoria
            modelBuilder.Entity<GastoCompartido>()
                .HasOne(g => g.Categoria)
                .WithMany()
                .HasForeignKey(g => g.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración ParticipanteGasto -> GastoCompartido (cascade)
            modelBuilder.Entity<ParticipanteGasto>()
                .HasOne(p => p.GastoCompartido)
                .WithMany(g => g.Participantes)
                .HasForeignKey(p => p.GastoCompartidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración Deuda -> Cuenta
            modelBuilder.Entity<Deuda>()
                .HasOne(d => d.Cuenta)
                .WithMany()
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración PagoDeuda -> Deuda (cascade)
            modelBuilder.Entity<PagoDeuda>()
                .HasOne(p => p.Deuda)
                .WithMany(d => d.Pagos)
                .HasForeignKey(p => p.DeudaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración PlantillaGasto -> Categoria
            modelBuilder.Entity<PlantillaGasto>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración ReglaCategoriaAutomatica -> Categoria
            modelBuilder.Entity<ReglaCategoriaAutomatica>()
                .HasOne(r => r.Categoria)
                .WithMany()
                .HasForeignKey(r => r.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración self-referencing para Subcategorías
            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.ParentCategoria)
                .WithMany(c => c.SubCategorias)
                .HasForeignKey(c => c.ParentCategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración many-to-many para GastoTag
            modelBuilder.Entity<GastoTag>()
                .HasKey(gt => new { gt.GastoId, gt.TagId });

            modelBuilder.Entity<GastoTag>()
                .HasOne(gt => gt.Gasto)
                .WithMany(g => g.GastoTags)
                .HasForeignKey(gt => gt.GastoId);

            modelBuilder.Entity<GastoTag>()
                .HasOne(gt => gt.Tag)
                .WithMany(t => t.GastoTags)
                .HasForeignKey(gt => gt.TagId);

            // Configuración many-to-many para IngresoTag
            modelBuilder.Entity<IngresoTag>()
                .HasKey(it => new { it.IngresoId, it.TagId });

            modelBuilder.Entity<IngresoTag>()
                .HasOne(it => it.Ingreso)
                .WithMany(i => i.IngresoTags)
                .HasForeignKey(it => it.IngresoId);

            modelBuilder.Entity<IngresoTag>()
                .HasOne(it => it.Tag)
                .WithMany(t => t.IngresoTags)
                .HasForeignKey(it => it.TagId);

            // Configuración DetalleGasto -> Gasto (cascade delete)
            modelBuilder.Entity<DetalleGasto>()
                .HasOne(d => d.Gasto)
                .WithMany(g => g.Detalles)
                .HasForeignKey(d => d.GastoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración GastoProgramado -> Categoria
            modelBuilder.Entity<GastoProgramado>()
                .HasOne(gp => gp.Categoria)
                .WithMany()
                .HasForeignKey(gp => gp.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración GastoProgramado -> Cuenta
            modelBuilder.Entity<GastoProgramado>()
                .HasOne(gp => gp.Cuenta)
                .WithMany()
                .HasForeignKey(gp => gp.CuentaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración GastoProgramado -> GastoRecurrente
            modelBuilder.Entity<GastoProgramado>()
                .HasOne(gp => gp.GastoRecurrente)
                .WithMany()
                .HasForeignKey(gp => gp.GastoRecurrenteId)
                .OnDelete(DeleteBehavior.SetNull);

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
        public DbSet<ConfiguracionNotificaciones> ConfiguracionesNotificaciones { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Transferencia> Transferencias { get; set; }
        public DbSet<GastoRecurrente> GastosRecurrentes { get; set; }
        public DbSet<IngresoRecurrente> IngresosRecurrentes { get; set; }
        public DbSet<Adjunto> Adjuntos { get; set; }

        public DbSet<ReglaCategoriaAutomatica> ReglasCategoriaAutomatica { get; set; }
        public DbSet<ImportacionCsv> ImportacionesCsv { get; set; }
        public DbSet<PlantillaGasto> PlantillasGasto { get; set; }
        public DbSet<Deuda> Deudas { get; set; }
        public DbSet<PagoDeuda> PagosDeuda { get; set; }
        public DbSet<GastoCompartido> GastosCompartidos { get; set; }
        public DbSet<ParticipanteGasto> ParticipantesGasto { get; set; }
        public DbSet<TipoCambio> TiposCambio { get; set; }
        public DbSet<ReporteProgramado> ReportesProgramados { get; set; }

        // Detalles de gastos (sub-compras)
        public DbSet<DetalleGasto> DetallesGasto { get; set; }

        // Gastos programados (recibos, cobros con fecha límite)
        public DbSet<GastoProgramado> GastosProgramados { get; set; }

        // Tags y relaciones many-to-many
        public DbSet<Tag> Tags { get; set; }
        public DbSet<GastoTag> GastoTags { get; set; }
        public DbSet<IngresoTag> IngresoTags { get; set; }
    }
}
