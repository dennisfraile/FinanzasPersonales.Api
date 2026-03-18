using System.Security.Claims;
using FinanzasPersonales.Api.Controllers;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FinanzasPersonales.Tests.Controllers
{
    public class GastosControllerTests
    {
        private const string TestUserId = "test-user-id-123";

        private GastosController CreateControllerWithUser(IGastosService gastosService)
        {
            var controller = new GastosController(gastosService, new Mock<IDetallesGastoService>().Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            return controller;
        }

        // --- GetGastos Tests ---

        [Fact]
        public async Task GetGastos_ShouldReturnOkWithPaginatedResponse()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            var expectedResponse = new PaginatedResponseDto<GastoDto>
            {
                Items = new List<GastoDto>
                {
                    new GastoDto { Id = 1, Monto = 100m, CategoriaId = 1, Fecha = DateTime.UtcNow }
                },
                PaginaActual = 1,
                TamañoPagina = 50,
                TotalItems = 1,
                TotalPaginas = 1,
                TienePaginaAnterior = false,
                TienePaginaSiguiente = false
            };

            mockService.Setup(s => s.GetGastosAsync(
                    TestUserId, null, null, null, null, null, null,
                    null, "fecha", "desc", 1, 50, null))
                .ReturnsAsync(expectedResponse);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            var result = await controller.GetGastos();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PaginatedResponseDto<GastoDto>>().Subject;
            response.Items.Should().HaveCount(1);
            response.TotalItems.Should().Be(1);
        }

        [Fact]
        public async Task GetGastos_WithNoData_ShouldReturnEmptyList()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            var emptyResponse = new PaginatedResponseDto<GastoDto>
            {
                Items = new List<GastoDto>(),
                PaginaActual = 1,
                TamañoPagina = 50,
                TotalItems = 0,
                TotalPaginas = 0,
                TienePaginaAnterior = false,
                TienePaginaSiguiente = false
            };

            mockService.Setup(s => s.GetGastosAsync(
                    TestUserId, null, null, null, null, null, null,
                    null, "fecha", "desc", 1, 50, null))
                .ReturnsAsync(emptyResponse);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            var result = await controller.GetGastos();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PaginatedResponseDto<GastoDto>>().Subject;
            response.Items.Should().BeEmpty();
            response.TotalItems.Should().Be(0);
        }

        // --- GetGasto (by ID) Tests ---

        [Fact]
        public async Task GetGasto_WithValidId_ShouldReturnOkWithGasto()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            var gastoDto = new GastoDto
            {
                Id = 1,
                Fecha = DateTime.UtcNow,
                CategoriaId = 1,
                Monto = 100m,
            };

            mockService.Setup(s => s.GetGastoAsync(TestUserId, 1))
                .ReturnsAsync(gastoDto);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            var result = await controller.GetGasto(1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetGasto_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            mockService.Setup(s => s.GetGastoAsync(TestUserId, 999))
                .ReturnsAsync((GastoDto?)null);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            var result = await controller.GetGasto(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        // --- PostGasto Tests ---

        [Fact]
        public async Task PostGasto_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            var dto = new CreateGastoDto
            {
                Fecha = DateTime.UtcNow,
                CategoriaId = 1,
                Tipo = "Variable",
                Descripcion = "Almuerzo",
                Monto = 150m
            };

            var createdGasto = new GastoDto
            {
                Id = 1,
                Fecha = dto.Fecha,
                CategoriaId = dto.CategoriaId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto
            };

            mockService.Setup(s => s.CreateGastoAsync(TestUserId, dto))
                .ReturnsAsync(createdGasto);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            var result = await controller.PostGasto(dto);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be("GetGasto");
        }

        [Fact]
        public async Task PostGasto_ShouldCallServiceWithCorrectUserId()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            var dto = new CreateGastoDto
            {
                Fecha = DateTime.UtcNow,
                CategoriaId = 1,
                Tipo = "Fijo",
                Monto = 5000m
            };

            mockService.Setup(s => s.CreateGastoAsync(It.IsAny<string>(), It.IsAny<CreateGastoDto>()))
                .ReturnsAsync(new GastoDto { Id = 1, Fecha = dto.Fecha, CategoriaId = 1, Monto = 5000m });

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            await controller.PostGasto(dto);

            // Assert
            mockService.Verify(s => s.CreateGastoAsync(TestUserId, dto), Times.Once);
        }

        [Fact]
        public async Task PostGasto_WithNoUserId_ShouldReturnBadRequest()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            var controller = new GastosController(mockService.Object, new Mock<IDetallesGastoService>().Object);

            // Set up empty claims (no NameIdentifier)
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var dto = new CreateGastoDto
            {
                Fecha = DateTime.UtcNow,
                CategoriaId = 1,
                Monto = 100m
            };

            // Act
            var result = await controller.PostGasto(dto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- PutGasto Tests ---

        [Fact]
        public async Task PutGasto_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            var controller = CreateControllerWithUser(mockService.Object);

            var dto = new UpdateGastoDto
            {
                Id = 2,
                Fecha = DateTime.UtcNow,
                CategoriaId = 1,
                Monto = 100m
            };

            // Act
            var result = await controller.PutGasto(1, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task PutGasto_WithNonExistentGasto_ShouldReturnNotFound()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            mockService.Setup(s => s.UpdateGastoAsync(TestUserId, 999, It.IsAny<UpdateGastoDto>()))
                .ReturnsAsync(false);

            var controller = CreateControllerWithUser(mockService.Object);

            var dto = new UpdateGastoDto
            {
                Id = 999,
                Fecha = DateTime.UtcNow,
                CategoriaId = 1,
                Monto = 100m
            };

            // Act
            var result = await controller.PutGasto(999, dto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task PutGasto_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            mockService.Setup(s => s.UpdateGastoAsync(TestUserId, 1, It.IsAny<UpdateGastoDto>()))
                .ReturnsAsync(true);

            var controller = CreateControllerWithUser(mockService.Object);

            var dto = new UpdateGastoDto
            {
                Id = 1,
                Fecha = DateTime.UtcNow,
                CategoriaId = 1,
                Monto = 200m
            };

            // Act
            var result = await controller.PutGasto(1, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        // --- DeleteGasto Tests ---

        [Fact]
        public async Task DeleteGasto_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            mockService.Setup(s => s.DeleteGastoAsync(TestUserId, 1))
                .ReturnsAsync(true);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            var result = await controller.DeleteGasto(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteGasto_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            mockService.Setup(s => s.DeleteGastoAsync(TestUserId, 999))
                .ReturnsAsync(false);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            var result = await controller.DeleteGasto(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteGasto_ShouldCallServiceWithCorrectParameters()
        {
            // Arrange
            var mockService = new Mock<IGastosService>();
            mockService.Setup(s => s.DeleteGastoAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            var controller = CreateControllerWithUser(mockService.Object);

            // Act
            await controller.DeleteGasto(42);

            // Assert
            mockService.Verify(s => s.DeleteGastoAsync(TestUserId, 42), Times.Once);
        }
    }
}
