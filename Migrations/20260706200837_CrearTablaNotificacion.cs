using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiserviciosPiscinas.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablaNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NOTIFICACION",
                schema: "ops",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    cita_id = table.Column<int>(type: "int", nullable: true),
                    mensaje = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    leida = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICACION", x => x.id);
                    table.ForeignKey(
                        name: "FK_NOTIFICACION_CITA",
                        column: x => x.cita_id,
                        principalSchema: "ops",
                        principalTable: "CITA",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_NOTIFICACION_USUARIO",
                        column: x => x.usuario_id,
                        principalSchema: "seg",
                        principalTable: "USUARIO",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_cita_id",
                schema: "ops",
                table: "NOTIFICACION",
                column: "cita_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_usuario_id",
                schema: "ops",
                table: "NOTIFICACION",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NOTIFICACION",
                schema: "ops");
        }
    }
}
