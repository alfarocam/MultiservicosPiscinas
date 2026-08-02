using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiserviciosPiscinas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCoordenadasDireccionCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imagen_ruta",
                schema: "inv",
                table: "PRODUCTO",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitud",
                schema: "cli",
                table: "DIRECCION_CLIENTE",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitud",
                schema: "cli",
                table: "DIRECCION_CLIENTE",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imagen_ruta",
                schema: "inv",
                table: "PRODUCTO");

            migrationBuilder.DropColumn(
                name: "latitud",
                schema: "cli",
                table: "DIRECCION_CLIENTE");

            migrationBuilder.DropColumn(
                name: "longitud",
                schema: "cli",
                table: "DIRECCION_CLIENTE");
        }
    }
}