using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferenciasGasto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferenciasGasto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    GastoOrigenId = table.Column<int>(type: "integer", nullable: false),
                    GastoDestinoId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CategoriaOrigenId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaDestinoId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferenciasGasto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferenciasGasto_Gastos_GastoDestinoId",
                        column: x => x.GastoDestinoId,
                        principalTable: "Gastos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasGasto_Gastos_GastoOrigenId",
                        column: x => x.GastoOrigenId,
                        principalTable: "Gastos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasGasto_GastoDestinoId",
                table: "TransferenciasGasto",
                column: "GastoDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasGasto_GastoOrigenId",
                table: "TransferenciasGasto",
                column: "GastoOrigenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferenciasGasto");
        }
    }
}
