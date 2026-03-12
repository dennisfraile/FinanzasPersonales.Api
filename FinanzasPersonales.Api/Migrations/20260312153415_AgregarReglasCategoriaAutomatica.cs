using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarReglasCategoriaAutomatica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReglasCategoriaAutomatica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Patron = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoCoincidencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    TipoTransaccion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Prioridad = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasCategoriaAutomatica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasCategoriaAutomatica_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReglasCategoriaAutomatica_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReglasCategoriaAutomatica_CategoriaId",
                table: "ReglasCategoriaAutomatica",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasCategoriaAutomatica_UserId",
                table: "ReglasCategoriaAutomatica",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReglasCategoriaAutomatica");
        }
    }
}
