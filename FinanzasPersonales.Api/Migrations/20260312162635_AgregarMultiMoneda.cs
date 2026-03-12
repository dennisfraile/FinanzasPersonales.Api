using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMultiMoneda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "Ingresos",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoConvertido",
                table: "Ingresos",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TipoCambioUsado",
                table: "Ingresos",
                type: "numeric(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "Gastos",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoConvertido",
                table: "Gastos",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TipoCambioUsado",
                table: "Gastos",
                type: "numeric(18,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposCambio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonedaOrigen = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MonedaDestino = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Tasa = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fuente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposCambio", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiposCambio");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "MontoConvertido",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "TipoCambioUsado",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "MontoConvertido",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "TipoCambioUsado",
                table: "Gastos");
        }
    }
}
