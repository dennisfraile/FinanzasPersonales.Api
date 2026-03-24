using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoAbonoToMetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AbonoAutomatico",
                table: "Metas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DiaAbono",
                table: "Metas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrecuenciaAbono",
                table: "Metas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoAbono",
                table: "Metas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximoAbono",
                table: "Metas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoAbono",
                table: "Metas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbonoAutomatico",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "DiaAbono",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "FrecuenciaAbono",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "MontoAbono",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "ProximoAbono",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "UltimoAbono",
                table: "Metas");
        }
    }
}
