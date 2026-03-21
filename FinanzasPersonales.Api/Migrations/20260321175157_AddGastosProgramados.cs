using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGastosProgramados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GastosProgramados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    CuentaId = table.Column<int>(type: "integer", nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EsMontoVariable = table.Column<bool>(type: "boolean", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GastoRecurrenteId = table.Column<int>(type: "integer", nullable: true),
                    GastoGeneradoId = table.Column<int>(type: "integer", nullable: true),
                    Notas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosProgramados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GastosProgramados_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GastosProgramados_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GastosProgramados_Cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GastosProgramados_GastosRecurrentes_GastoRecurrenteId",
                        column: x => x.GastoRecurrenteId,
                        principalTable: "GastosRecurrentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GastosProgramados_CategoriaId",
                table: "GastosProgramados",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_GastosProgramados_CuentaId",
                table: "GastosProgramados",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_GastosProgramados_GastoRecurrenteId",
                table: "GastosProgramados",
                column: "GastoRecurrenteId");

            migrationBuilder.CreateIndex(
                name: "IX_GastosProgramados_UserId",
                table: "GastosProgramados",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GastosProgramados");
        }
    }
}
