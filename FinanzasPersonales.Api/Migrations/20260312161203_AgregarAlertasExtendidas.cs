using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAlertasExtendidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlertaBalanceBajo",
                table: "ConfiguracionesNotificaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AlertaPagoRecurrente",
                table: "ConfiguracionesNotificaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DiasAntesPagoRecurrente",
                table: "ConfiguracionesNotificaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UmbralBalanceBajo",
                table: "ConfiguracionesNotificaciones",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertaBalanceBajo",
                table: "ConfiguracionesNotificaciones");

            migrationBuilder.DropColumn(
                name: "AlertaPagoRecurrente",
                table: "ConfiguracionesNotificaciones");

            migrationBuilder.DropColumn(
                name: "DiasAntesPagoRecurrente",
                table: "ConfiguracionesNotificaciones");

            migrationBuilder.DropColumn(
                name: "UmbralBalanceBajo",
                table: "ConfiguracionesNotificaciones");
        }
    }
}
