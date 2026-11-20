using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Modulo2CatalogoVehiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipo_vehiculo",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    nombre = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_vehiculo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "marca",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_vehiculo_id = table.Column<short>(type: "smallint", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_marca", x => x.id);
                    table.ForeignKey(
                        name: "fk_marca_tipos_vehiculo_tipo_vehiculo_id",
                        column: x => x.tipo_vehiculo_id,
                        principalTable: "tipo_vehiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "modelo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    marca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modelo", x => x.id);
                    table.ForeignKey(
                        name: "fk_modelo_marca_marca_id",
                        column: x => x.marca_id,
                        principalTable: "marca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permiso",
                columns: new[] { "codigo", "descripcion", "nombre" },
                values: new object[] { "catalogo_vehiculo.gestionar", "Crear marcas y modelos nuevos en el catálogo global cuando falte uno", "Gestionar catálogo de vehículos" });

            migrationBuilder.InsertData(
                table: "tipo_vehiculo",
                columns: new[] { "id", "nombre" },
                values: new object[,]
                {
                    { (short)1, "Auto" },
                    { (short)2, "Camioneta/SUV" },
                    { (short)3, "Moto" },
                    { (short)4, "Camión" },
                    { (short)5, "Otro" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_marca_nombre_trgm",
                table: "marca",
                column: "nombre")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_marca_tipo_vehiculo_id_nombre",
                table: "marca",
                columns: new[] { "tipo_vehiculo_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_modelo_marca_id_nombre",
                table: "modelo",
                columns: new[] { "marca_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_modelo_nombre_trgm",
                table: "modelo",
                column: "nombre")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_tipo_vehiculo_nombre",
                table: "tipo_vehiculo",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modelo");

            migrationBuilder.DropTable(
                name: "marca");

            migrationBuilder.DropTable(
                name: "tipo_vehiculo");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "catalogo_vehiculo.gestionar");
        }
    }
}
