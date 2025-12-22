using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSistemaCuentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DatosAdicionales",
                table: "Notificaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenciaId",
                table: "Notificaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentaId",
                table: "Metas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentaId",
                table: "Ingresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentaId",
                table: "Gastos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfiguracionesNotificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AlertasPresupuesto = table.Column<bool>(type: "boolean", nullable: false),
                    RecordatorioMetas = table.Column<bool>(type: "boolean", nullable: false),
                    GastosInusuales = table.Column<bool>(type: "boolean", nullable: false),
                    ResumenMensual = table.Column<bool>(type: "boolean", nullable: false),
                    UmbralPresupuesto = table.Column<int>(type: "integer", nullable: false),
                    UmbralMeta = table.Column<int>(type: "integer", nullable: false),
                    FactorGastoInusual = table.Column<decimal>(type: "numeric(3,1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesNotificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesNotificaciones_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    BalanceActual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BalanceInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Icono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cuentas_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transferencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CuentaOrigenId = table.Column<int>(type: "integer", nullable: false),
                    CuentaDestinoId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transferencias_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transferencias_Cuentas_CuentaDestinoId",
                        column: x => x.CuentaDestinoId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transferencias_Cuentas_CuentaOrigenId",
                        column: x => x.CuentaOrigenId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Metas_CuentaId",
                table: "Metas",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_CuentaId",
                table: "Ingresos",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_CuentaId",
                table: "Gastos",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesNotificaciones_UserId",
                table: "ConfiguracionesNotificaciones",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_UserId",
                table: "Cuentas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transferencias_CuentaDestinoId",
                table: "Transferencias",
                column: "CuentaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transferencias_CuentaOrigenId",
                table: "Transferencias",
                column: "CuentaOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_Transferencias_UserId",
                table: "Transferencias",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_Cuentas_CuentaId",
                table: "Gastos",
                column: "CuentaId",
                principalTable: "Cuentas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingresos_Cuentas_CuentaId",
                table: "Ingresos",
                column: "CuentaId",
                principalTable: "Cuentas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Metas_Cuentas_CuentaId",
                table: "Metas",
                column: "CuentaId",
                principalTable: "Cuentas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_Cuentas_CuentaId",
                table: "Gastos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_Cuentas_CuentaId",
                table: "Ingresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Metas_Cuentas_CuentaId",
                table: "Metas");

            migrationBuilder.DropTable(
                name: "ConfiguracionesNotificaciones");

            migrationBuilder.DropTable(
                name: "Transferencias");

            migrationBuilder.DropTable(
                name: "Cuentas");

            migrationBuilder.DropIndex(
                name: "IX_Metas_CuentaId",
                table: "Metas");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_CuentaId",
                table: "Ingresos");

            migrationBuilder.DropIndex(
                name: "IX_Gastos_CuentaId",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "DatosAdicionales",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "ReferenciaId",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "CuentaId",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "CuentaId",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "CuentaId",
                table: "Gastos");
        }
    }
}
