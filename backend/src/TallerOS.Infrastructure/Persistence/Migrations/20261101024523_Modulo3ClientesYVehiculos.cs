using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Modulo3ClientesYVehiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cuenta_cliente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telefono = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    telefono_verificado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_cliente", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuenta_cliente_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehiculo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    marca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modelo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anio = table.Column<short>(type: "smallint", nullable: false),
                    motor = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehiculo", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehiculo_marca_marca_id",
                        column: x => x.marca_id,
                        principalTable: "marca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehiculo_modelo_modelo_id",
                        column: x => x.modelo_id,
                        principalTable: "modelo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    identificacion = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    telefono = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    canal_preferido = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente", x => x.id);
                    table.UniqueConstraint("ak_cliente_empresa_id_id", x => new { x.empresa_id, x.id });
                    table.ForeignKey(
                        name: "fk_cliente_cuentas_cliente_cuenta_cliente_id",
                        column: x => x.cuenta_cliente_id,
                        principalTable: "cuenta_cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cliente_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehiculo_placa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    placa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehiculo_placa", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehiculo_placa_vehiculo_vehiculo_id",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehiculo_propietario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehiculo_propietario", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehiculo_propietario_cuenta_cliente_cuenta_cliente_id",
                        column: x => x.cuenta_cliente_id,
                        principalTable: "cuenta_cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehiculo_propietario_vehiculo_vehiculo_id",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cliente_vehiculo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente_vehiculo", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_vehiculo_cliente_empresa_id_cliente_id",
                        columns: x => new { x.empresa_id, x.cliente_id },
                        principalTable: "cliente",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cliente_vehiculo_vehiculos_vehiculo_id",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_cuenta_cliente_id",
                table: "cliente",
                column: "cuenta_cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_empresa_id",
                table: "cliente",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_empresa_id_identificacion",
                table: "cliente",
                columns: new[] { "empresa_id", "identificacion" },
                unique: true,
                filter: "eliminado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_identificacion_trgm",
                table: "cliente",
                column: "identificacion")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_nombre_trgm",
                table: "cliente",
                column: "nombre")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vehiculo_empresa_id_cliente_id_vehiculo_id",
                table: "cliente_vehiculo",
                columns: new[] { "empresa_id", "cliente_id", "vehiculo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_vehiculo_vehiculo_id_empresa_id",
                table: "cliente_vehiculo",
                columns: new[] { "vehiculo_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_cliente_usuario_id",
                table: "cuenta_cliente",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_marca_id",
                table: "vehiculo",
                column: "marca_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_modelo_id",
                table: "vehiculo",
                column: "modelo_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_vin",
                table: "vehiculo",
                column: "vin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_placa_placa_activa",
                table: "vehiculo_placa",
                column: "placa",
                unique: true,
                filter: "hasta IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_placa_placa_trgm",
                table: "vehiculo_placa",
                column: "placa")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_placa_vehiculo_id",
                table: "vehiculo_placa",
                column: "vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_propietario_cuenta_cliente_id",
                table: "vehiculo_propietario",
                column: "cuenta_cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_propietario_vehiculo_id",
                table: "vehiculo_propietario",
                column: "vehiculo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cliente_vehiculo");

            migrationBuilder.DropTable(
                name: "vehiculo_placa");

            migrationBuilder.DropTable(
                name: "vehiculo_propietario");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "vehiculo");

            migrationBuilder.DropTable(
                name: "cuenta_cliente");
        }
    }
}
