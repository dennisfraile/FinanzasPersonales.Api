using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSubcategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCategoriaId",
                table: "Categorias",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_ParentCategoriaId",
                table: "Categorias",
                column: "ParentCategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_ParentCategoriaId",
                table: "Categorias",
                column: "ParentCategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Categorias_ParentCategoriaId",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_ParentCategoriaId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "ParentCategoriaId",
                table: "Categorias");
        }
    }
}
