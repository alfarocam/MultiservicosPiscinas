using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiserviciosPiscinas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarStockMinimoProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "stock_minimo",
                schema: "inv",
                table: "PRODUCTO",
                type: "int",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stock_minimo",
                schema: "inv",
                table: "PRODUCTO");
        }
    }
}
