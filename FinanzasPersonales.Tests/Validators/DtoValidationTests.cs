using System.ComponentModel.DataAnnotations;
using FinanzasPersonales.Api.Dtos;
using FluentAssertions;

namespace FinanzasPersonales.Tests.Validators
{
    public class DtoValidationTests
    {
        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        // --- CreateMetaDto Validation Tests ---

        [Fact]
        public void CreateMetaDto_WithValidData_ShouldHaveNoErrors()
        {
            // Arrange
            var dto = new CreateMetaDto
            {
                Metas = "Ahorro para vacaciones",
                MontoTotal = 5000m,
                AhorroActual = 0m,
                MontoRestante = 5000m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void CreateMetaDto_WithEmptyName_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateMetaDto
            {
                Metas = "",
                MontoTotal = 5000m,
                MontoRestante = 5000m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Metas"));
        }

        [Fact]
        public void CreateMetaDto_WithNameExceeding100Chars_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateMetaDto
            {
                Metas = new string('A', 101),
                MontoTotal = 5000m,
                MontoRestante = 5000m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Metas"));
        }

        [Fact]
        public void CreateMetaDto_WithZeroMontoTotal_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateMetaDto
            {
                Metas = "Meta Test",
                MontoTotal = 0m,
                MontoRestante = 0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("MontoTotal"));
        }

        [Fact]
        public void CreateMetaDto_WithNegativeAhorroActual_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateMetaDto
            {
                Metas = "Meta Test",
                MontoTotal = 1000m,
                AhorroActual = -100m,
                MontoRestante = 1100m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("AhorroActual"));
        }

        // --- CreateGastoDto Validation Tests ---

        [Fact]
        public void CreateGastoDto_WithValidData_ShouldHaveNoErrors()
        {
            // Arrange
            var dto = new CreateGastoDto
            {
                Fecha = DateTime.Now,
                CategoriaId = 1,
                Tipo = "Variable",
                Descripcion = "Almuerzo",
                Monto = 150.50m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void CreateGastoDto_WithZeroMonto_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateGastoDto
            {
                Fecha = DateTime.Now,
                CategoriaId = 1,
                Tipo = "Variable",
                Monto = 0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Monto"));
        }

        [Fact]
        public void CreateGastoDto_WithNegativeMonto_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateGastoDto
            {
                Fecha = DateTime.Now,
                CategoriaId = 1,
                Tipo = "Variable",
                Monto = -50m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Monto"));
        }

        [Fact]
        public void CreateGastoDto_WithTipoExceedingMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateGastoDto
            {
                Fecha = DateTime.Now,
                CategoriaId = 1,
                Tipo = new string('X', 21), // Max is 20
                Monto = 100m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Tipo"));
        }

        [Fact]
        public void CreateGastoDto_WithDescripcionExceedingMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new CreateGastoDto
            {
                Fecha = DateTime.Now,
                CategoriaId = 1,
                Tipo = "Variable",
                Descripcion = new string('X', 501), // Max is 500
                Monto = 100m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Descripcion"));
        }

        // --- ConfiguracionNotificacionesDto Validation Tests ---

        [Fact]
        public void ConfiguracionNotificacionesDto_WithValidData_ShouldHaveNoErrors()
        {
            // Arrange
            var dto = new ConfiguracionNotificacionesDto
            {
                AlertasPresupuesto = true,
                UmbralPresupuesto = 80,
                AlertasMetas = true,
                DiasAntesMeta = 30,
                RecordatorioMetas = true,
                GastosInusuales = true,
                ResumenMensual = false,
                UmbralMeta = 90,
                FactorGastoInusual = 2.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void ConfiguracionNotificacionesDto_WithUmbralPresupuestoBelowMin_ShouldHaveError()
        {
            // Arrange
            var dto = new ConfiguracionNotificacionesDto
            {
                UmbralPresupuesto = 10, // Min is 50
                DiasAntesMeta = 30,
                UmbralMeta = 90,
                FactorGastoInusual = 2.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("UmbralPresupuesto"));
        }

        [Fact]
        public void ConfiguracionNotificacionesDto_WithInvalidEmail_ShouldHaveError()
        {
            // Arrange
            var dto = new ConfiguracionNotificacionesDto
            {
                UmbralPresupuesto = 80,
                DiasAntesMeta = 30,
                UmbralMeta = 90,
                FactorGastoInusual = 2.0m,
                Email = "not-an-email"
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void ConfiguracionNotificacionesDto_WithValidEmail_ShouldHaveNoEmailError()
        {
            // Arrange
            var dto = new ConfiguracionNotificacionesDto
            {
                UmbralPresupuesto = 80,
                DiasAntesMeta = 30,
                UmbralMeta = 90,
                FactorGastoInusual = 2.0m,
                Email = "user@example.com"
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().NotContain(r => r.MemberNames.Contains("Email"));
        }

        // --- UpdateMetaDto Validation Tests ---

        [Fact]
        public void UpdateMetaDto_WithValidData_ShouldHaveNoErrors()
        {
            // Arrange
            var dto = new UpdateMetaDto
            {
                Id = 1,
                Metas = "Meta actualizada",
                MontoTotal = 2000m,
                AhorroActual = 500m,
                MontoRestante = 1500m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void UpdateMetaDto_WithNegativeMontoTotal_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new UpdateMetaDto
            {
                Id = 1,
                Metas = "Meta Test",
                MontoTotal = -100m,
                AhorroActual = 0m,
                MontoRestante = 0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("MontoTotal"));
        }

        // --- UpdateGastoDto Validation Tests ---

        [Fact]
        public void UpdateGastoDto_WithValidData_ShouldHaveNoErrors()
        {
            // Arrange
            var dto = new UpdateGastoDto
            {
                Id = 1,
                Fecha = DateTime.Now,
                CategoriaId = 1,
                Tipo = "Fijo",
                Monto = 250m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void UpdateGastoDto_WithZeroMonto_ShouldHaveValidationError()
        {
            // Arrange
            var dto = new UpdateGastoDto
            {
                Id = 1,
                Fecha = DateTime.Now,
                CategoriaId = 1,
                Monto = 0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            results.Should().Contain(r => r.MemberNames.Contains("Monto"));
        }
    }
}
