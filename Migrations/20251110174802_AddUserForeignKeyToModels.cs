using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserForeignKeyToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Metas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Ingresos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Gastos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Metas_UserId",
                table: "Metas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_UserId",
                table: "Ingresos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_UserId",
                table: "Gastos",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_AspNetUsers_UserId",
                table: "Gastos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingresos_AspNetUsers_UserId",
                table: "Ingresos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Metas_AspNetUsers_UserId",
                table: "Metas",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_AspNetUsers_UserId",
                table: "Gastos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_AspNetUsers_UserId",
                table: "Ingresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Metas_AspNetUsers_UserId",
                table: "Metas");

            migrationBuilder.DropIndex(
                name: "IX_Metas_UserId",
                table: "Metas");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_UserId",
                table: "Ingresos");

            migrationBuilder.DropIndex(
                name: "IX_Gastos_UserId",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Gastos");
        }
    }
}
