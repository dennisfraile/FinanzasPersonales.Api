using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotasTransacciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Ingresos",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Gastos",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Gastos");
        }
    }
}
