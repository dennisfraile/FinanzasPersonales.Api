using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarImportacionCsv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportacionesCsv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NombreArchivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TotalFilas = table.Column<int>(type: "integer", nullable: false),
                    FilasImportadas = table.Column<int>(type: "integer", nullable: false),
                    FilasDuplicadas = table.Column<int>(type: "integer", nullable: false),
                    FilasError = table.Column<int>(type: "integer", nullable: false),
                    FechaImportacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportacionesCsv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportacionesCsv_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportacionesCsv_UserId",
                table: "ImportacionesCsv",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportacionesCsv");
        }
    }
}
