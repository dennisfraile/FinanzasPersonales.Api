using FinanzasPersonales.Api.Hubs;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Services;
using FinanzasPersonales.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FinanzasPersonales.Tests.Services
{
    public class NotificacionServiceTests
    {
        private const string TestUserId = "test-user-id-123";

        private static IHubContext<NotificacionesHub> CreateMockHubContext()
        {
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            var mockHubContext = new Mock<IHubContext<NotificacionesHub>>();
            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            return mockHubContext.Object;
        }

        private async Task<int> SeedNotificacion(
            FinanzasPersonales.Api.Data.FinanzasDbContext context,
            bool leida = false,
            string tipo = "Informativa",
            string titulo = "Test Titulo",
            string mensaje = "Test Mensaje",
            string? userId = null)
        {
            var notificacion = new Notificacion
            {
                UserId = userId ?? TestUserId,
                Tipo = tipo,
                Titulo = titulo,
                Mensaje = mensaje,
                FechaCreacion = DateTime.Now,
                Leida = leida,
                EmailEnviado = false
            };
            context.Notificaciones.Add(notificacion);
            await context.SaveChangesAsync();
            return notificacion.Id;
        }

        // --- CrearNotificacionAsync Tests ---

        [Fact]
        public async Task CrearNotificacionAsync_WithValidData_ShouldReturnNotificacionId()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());

            // Act
            var id = await service.CrearNotificacionAsync(TestUserId, "Informativa", "Titulo Test", "Mensaje Test");

            // Assert
            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CrearNotificacionAsync_ShouldPersistToDatabase()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());

            // Act
            var id = await service.CrearNotificacionAsync(TestUserId, "PresupuestoAlerta", "Alerta", "Tu presupuesto esta al limite");

            // Assert
            var notificacion = await context.Notificaciones.FindAsync(id);
            notificacion.Should().NotBeNull();
            notificacion!.UserId.Should().Be(TestUserId);
            notificacion.Tipo.Should().Be("PresupuestoAlerta");
            notificacion.Titulo.Should().Be("Alerta");
            notificacion.Mensaje.Should().Be("Tu presupuesto esta al limite");
            notificacion.Leida.Should().BeFalse();
            notificacion.EmailEnviado.Should().BeFalse();
        }

        [Fact]
        public async Task CrearNotificacionAsync_MultipleCalls_ShouldCreateDistinctNotifications()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());

            // Act
            var id1 = await service.CrearNotificacionAsync(TestUserId, "Informativa", "Titulo 1", "Mensaje 1");
            var id2 = await service.CrearNotificacionAsync(TestUserId, "Informativa", "Titulo 2", "Mensaje 2");

            // Assert
            id1.Should().NotBe(id2);
            context.Notificaciones.Count().Should().Be(2);
        }

        // --- MarcarComoLeidaAsync Tests ---

        [Fact]
        public async Task MarcarComoLeidaAsync_WithValidNotificacion_ShouldMarkAsRead()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());
            var id = await SeedNotificacion(context, leida: false);

            // Act
            await service.MarcarComoLeidaAsync(id, TestUserId);

            // Assert
            var notificacion = await context.Notificaciones.FindAsync(id);
            notificacion!.Leida.Should().BeTrue();
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_WithNonExistentId_ShouldNotThrow()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());

            // Act & Assert - should not throw, just do nothing
            await service.Invoking(s => s.MarcarComoLeidaAsync(999, TestUserId))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_WithWrongUserId_ShouldNotMarkAsRead()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());
            var id = await SeedNotificacion(context, leida: false);

            // Act
            await service.MarcarComoLeidaAsync(id, "wrong-user-id");

            // Assert
            var notificacion = await context.Notificaciones.FindAsync(id);
            notificacion!.Leida.Should().BeFalse();
        }

        // --- ObtenerNoLeidasAsync Tests ---

        [Fact]
        public async Task ObtenerNoLeidasAsync_ShouldReturnOnlyUnreadNotifications()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());
            await SeedNotificacion(context, leida: false, titulo: "No leida 1");
            await SeedNotificacion(context, leida: false, titulo: "No leida 2");
            await SeedNotificacion(context, leida: true, titulo: "Leida 1");

            // Act
            var result = await service.ObtenerNoLeidasAsync(TestUserId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(n => !n.Leida);
        }

        [Fact]
        public async Task ObtenerNoLeidasAsync_ShouldNotReturnOtherUserNotifications()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());
            await SeedNotificacion(context, leida: false, userId: TestUserId);
            await SeedNotificacion(context, leida: false, userId: "other-user-id");

            // Act
            var result = await service.ObtenerNoLeidasAsync(TestUserId);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObtenerNoLeidasAsync_WithNoNotifications_ShouldReturnEmptyList()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());

            // Act
            var result = await service.ObtenerNoLeidasAsync(TestUserId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerNoLeidasAsync_ShouldReturnOrderedByFechaDescending()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());

            var older = new Notificacion
            {
                UserId = TestUserId,
                Tipo = "Informativa",
                Titulo = "Antigua",
                Mensaje = "Msg",
                FechaCreacion = DateTime.Now.AddDays(-2),
                Leida = false,
                EmailEnviado = false
            };
            var newer = new Notificacion
            {
                UserId = TestUserId,
                Tipo = "Informativa",
                Titulo = "Reciente",
                Mensaje = "Msg",
                FechaCreacion = DateTime.Now,
                Leida = false,
                EmailEnviado = false
            };
            context.Notificaciones.AddRange(older, newer);
            await context.SaveChangesAsync();

            // Act
            var result = await service.ObtenerNoLeidasAsync(TestUserId);

            // Assert
            result.Should().HaveCount(2);
            result[0].Titulo.Should().Be("Reciente");
            result[1].Titulo.Should().Be("Antigua");
        }

        // --- ObtenerTodasAsync Tests ---

        [Fact]
        public async Task ObtenerTodasAsync_WithoutFilter_ShouldReturnAllNotifications()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());
            await SeedNotificacion(context, leida: false);
            await SeedNotificacion(context, leida: true);

            // Act
            var result = await service.ObtenerTodasAsync(TestUserId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObtenerTodasAsync_WithSoloNoLeidasTrue_ShouldReturnOnlyUnread()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());
            await SeedNotificacion(context, leida: false);
            await SeedNotificacion(context, leida: true);

            // Act
            var result = await service.ObtenerTodasAsync(TestUserId, soloNoLeidas: true);

            // Assert
            result.Should().HaveCount(1);
            result.Should().OnlyContain(n => !n.Leida);
        }

        [Fact]
        public async Task ObtenerTodasAsync_ShouldMapDtoFieldsCorrectly()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var service = new NotificacionService(context, CreateMockHubContext());
            await SeedNotificacion(context, tipo: "MetaCercana", titulo: "Meta Titulo", mensaje: "Meta Mensaje");

            // Act
            var result = await service.ObtenerTodasAsync(TestUserId);

            // Assert
            result.Should().HaveCount(1);
            var dto = result[0];
            dto.Tipo.Should().Be("MetaCercana");
            dto.Titulo.Should().Be("Meta Titulo");
            dto.Mensaje.Should().Be("Meta Mensaje");
            dto.Leida.Should().BeFalse();
            dto.EmailEnviado.Should().BeFalse();
        }
    }
}
