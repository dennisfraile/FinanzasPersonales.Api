using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Services;
using FinanzasPersonales.Tests.Helpers;
using FluentAssertions;

namespace FinanzasPersonales.Tests.Services
{
    public class MetasServiceTests
    {
        private const string TestUserId = "test-user-id-123";

        private Meta CreateTestMeta(decimal montoTotal = 1000m, decimal ahorroActual = 0m, string nombre = "Mi Meta")
        {
            return new Meta
            {
                Metas = nombre,
                MontoTotal = montoTotal,
                AhorroActual = ahorroActual,
                MontoRestante = montoTotal - ahorroActual,
                UserId = TestUserId
            };
        }

        // --- CalcularProgresoAsync Tests ---

        [Fact]
        public async Task CalcularProgresoAsync_WithValidMeta_ShouldReturnProgressInfo()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta(montoTotal: 1000m, ahorroActual: 500m);
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            dynamic result = await service.CalcularProgresoAsync(meta.Id, TestUserId);

            // Assert - use reflection since it returns anonymous type
            var type = result.GetType();
            var porcentaje = (decimal)type.GetProperty("PorcentajeProgreso")!.GetValue(result);
            var estado = (string)type.GetProperty("Estado")!.GetValue(result);

            porcentaje.Should().Be(50.00m);
            estado.Should().Be("En progreso");
        }

        [Fact]
        public async Task CalcularProgresoAsync_WithCompletedMeta_ShouldReturnCompletada()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta(montoTotal: 1000m, ahorroActual: 1000m);
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            dynamic result = await service.CalcularProgresoAsync(meta.Id, TestUserId);

            // Assert
            var type = result.GetType();
            var porcentaje = (decimal)type.GetProperty("PorcentajeProgreso")!.GetValue(result);
            var estado = (string)type.GetProperty("Estado")!.GetValue(result);

            porcentaje.Should().Be(100.00m);
            estado.Should().Be("Completada");
        }

        [Fact]
        public async Task CalcularProgresoAsync_WithAlmostCompleteMeta_ShouldReturnCasiCompletada()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta(montoTotal: 1000m, ahorroActual: 800m);
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            dynamic result = await service.CalcularProgresoAsync(meta.Id, TestUserId);

            // Assert
            var type = result.GetType();
            var estado = (string)type.GetProperty("Estado")!.GetValue(result);
            estado.Should().Be("Casi completada");
        }

        [Fact]
        public async Task CalcularProgresoAsync_WithZeroMontoTotal_ShouldReturnZeroProgress()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta(montoTotal: 0m, ahorroActual: 0m);
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            dynamic result = await service.CalcularProgresoAsync(meta.Id, TestUserId);

            // Assert
            var type = result.GetType();
            var porcentaje = (decimal)type.GetProperty("PorcentajeProgreso")!.GetValue(result);
            porcentaje.Should().Be(0m);
        }

        [Fact]
        public async Task CalcularProgresoAsync_WithNonExistentMeta_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new MetasService(context);

            // Act & Assert
            await service.Invoking(s => s.CalcularProgresoAsync(999, TestUserId))
                .Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Meta no encontrada");
        }

        [Fact]
        public async Task CalcularProgresoAsync_WithWrongUserId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta();
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act & Assert
            await service.Invoking(s => s.CalcularProgresoAsync(meta.Id, "wrong-user-id"))
                .Should().ThrowAsync<KeyNotFoundException>();
        }

        // --- AbonarMetaAsync Tests ---

        [Fact]
        public async Task AbonarMetaAsync_WithValidAmount_ShouldIncreaseAhorroActual()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta(montoTotal: 1000m, ahorroActual: 200m);
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            var result = await service.AbonarMetaAsync(meta.Id, TestUserId, 300m);

            // Assert
            result.Should().BeTrue();
            meta.AhorroActual.Should().Be(500m);
            meta.MontoRestante.Should().Be(500m);
        }

        [Fact]
        public async Task AbonarMetaAsync_ExceedingTotal_ShouldSetMontoRestanteToZero()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta(montoTotal: 1000m, ahorroActual: 900m);
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            var result = await service.AbonarMetaAsync(meta.Id, TestUserId, 200m);

            // Assert
            result.Should().BeTrue();
            meta.AhorroActual.Should().Be(1100m);
            meta.MontoRestante.Should().Be(0m); // Clamped to zero
        }

        [Fact]
        public async Task AbonarMetaAsync_WithZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta();
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act & Assert
            await service.Invoking(s => s.AbonarMetaAsync(meta.Id, TestUserId, 0m))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("El monto debe ser mayor a 0");
        }

        [Fact]
        public async Task AbonarMetaAsync_WithNegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var meta = CreateTestMeta();
            context.Metas.Add(meta);
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act & Assert
            await service.Invoking(s => s.AbonarMetaAsync(meta.Id, TestUserId, -100m))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AbonarMetaAsync_WithNonExistentMeta_ShouldReturnFalse()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new MetasService(context);

            // Act
            var result = await service.AbonarMetaAsync(999, TestUserId, 100m);

            // Assert
            result.Should().BeFalse();
        }

        // --- ObtenerProyeccionesAsync Tests ---

        [Fact]
        public async Task ObtenerProyeccionesAsync_WithMetas_ShouldReturnProjections()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            context.Metas.Add(CreateTestMeta(montoTotal: 1000m, ahorroActual: 500m, nombre: "Meta 1"));
            context.Metas.Add(CreateTestMeta(montoTotal: 2000m, ahorroActual: 0m, nombre: "Meta 2"));
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            var result = await service.ObtenerProyeccionesAsync(TestUserId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObtenerProyeccionesAsync_WithNoMetas_ShouldReturnEmptyList()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new MetasService(context);

            // Act
            var result = await service.ObtenerProyeccionesAsync(TestUserId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerProyeccionesAsync_CompletedMeta_ShouldShowCompletadaStatus()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            context.Metas.Add(CreateTestMeta(montoTotal: 1000m, ahorroActual: 1000m, nombre: "Meta Completada"));
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            var result = await service.ObtenerProyeccionesAsync(TestUserId);

            // Assert
            result.Should().HaveCount(1);
            dynamic item = result[0];
            var type = item.GetType();
            var estado = (string)type.GetProperty("Estado")!.GetValue(item);
            estado.Should().Be("Completada");
        }

        [Fact]
        public async Task ObtenerProyeccionesAsync_ShouldOnlyReturnUserMetas()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            context.Metas.Add(CreateTestMeta(nombre: "Mi Meta"));
            context.Metas.Add(new Meta
            {
                Metas = "Meta de otro usuario",
                MontoTotal = 500m,
                AhorroActual = 100m,
                MontoRestante = 400m,
                UserId = "other-user-id"
            });
            await context.SaveChangesAsync();

            var service = new MetasService(context);

            // Act
            var result = await service.ObtenerProyeccionesAsync(TestUserId);

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
