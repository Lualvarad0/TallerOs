using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Modulo1OrganizacionYSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empresa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permiso",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permiso", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "plan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    limite_usuarios = table.Column<int>(type: "integer", nullable: true),
                    limite_sucursales = table.Column<int>(type: "integer", nullable: true),
                    permite_whatsapp_oficial = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    es_super_admin = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol", x => x.id);
                    table.UniqueConstraint("ak_rol_empresa_id_id", x => new { x.empresa_id, x.id });
                    table.ForeignKey(
                        name: "fk_rol_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sucursal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    direccion = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sucursal", x => x.id);
                    table.UniqueConstraint("ak_sucursales_empresa_id_id", x => new { x.empresa_id, x.id });
                    table.ForeignKey(
                        name: "fk_sucursal_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "suscripcion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vigente_desde = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    vigente_hasta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suscripcion", x => x.id);
                    table.ForeignKey(
                        name: "fk_suscripcion_empresa_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_suscripcion_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rol_permiso",
                columns: table => new
                {
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permiso_codigo = table.Column<string>(type: "character varying(60)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol_permiso", x => new { x.rol_id, x.permiso_codigo });
                    table.ForeignKey(
                        name: "fk_rol_permiso_permiso_permiso_codigo",
                        column: x => x.permiso_codigo,
                        principalTable: "permiso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rol_permiso_rol_rol_id",
                        column: x => x.rol_id,
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_area", x => x.id);
                    table.UniqueConstraint("ak_area_empresa_id_id", x => new { x.empresa_id, x.id });
                    table.ForeignKey(
                        name: "fk_area_sucursales_empresa_id_sucursal_id",
                        columns: x => new { x.empresa_id, x.sucursal_id },
                        principalTable: "sucursal",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dispositivo_tablet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ultimo_uso_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dispositivo_tablet", x => x.id);
                    table.ForeignKey(
                        name: "fk_dispositivo_tablet_sucursal_empresa_id_sucursal_id",
                        columns: x => new { x.empresa_id, x.sucursal_id },
                        principalTable: "sucursal",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_rol",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_rol", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_rol_rol_empresa_id_rol_id",
                        columns: x => new { x.empresa_id, x.rol_id },
                        principalTable: "rol",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_rol_sucursal_empresa_id_sucursal_id",
                        columns: x => new { x.empresa_id, x.sucursal_id },
                        principalTable: "sucursal",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_rol_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    tarifa_hora = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    tiene_acceso = table.Column<bool>(type: "boolean", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pin_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_personal", x => x.id);
                    table.ForeignKey(
                        name: "fk_personal_area_empresa_id_area_id",
                        columns: x => new { x.empresa_id, x.area_id },
                        principalTable: "area",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_personal_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_area_empresa_id_sucursal_id",
                table: "area",
                columns: new[] { "empresa_id", "sucursal_id" });

            migrationBuilder.CreateIndex(
                name: "ix_area_sucursal_id_nombre",
                table: "area",
                columns: new[] { "sucursal_id", "nombre" },
                unique: true,
                filter: "eliminado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_tablet_empresa_id_sucursal_id",
                table: "dispositivo_tablet",
                columns: new[] { "empresa_id", "sucursal_id" });

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_tablet_token_hash",
                table: "dispositivo_tablet",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empresa_ruc",
                table: "empresa",
                column: "ruc",
                unique: true,
                filter: "eliminado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_personal_empresa_id_area_id",
                table: "personal",
                columns: new[] { "empresa_id", "area_id" });

            migrationBuilder.CreateIndex(
                name: "ix_personal_usuario_id",
                table: "personal",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_nombre",
                table: "plan",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rol_empresa_id",
                table: "rol",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_rol_empresa_id_nombre",
                table: "rol",
                columns: new[] { "empresa_id", "nombre" },
                unique: true,
                filter: "eliminado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_rol_permiso_permiso_codigo",
                table: "rol_permiso",
                column: "permiso_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_sucursal_empresa_id",
                table: "sucursal",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_sucursal_empresa_id_nombre",
                table: "sucursal",
                columns: new[] { "empresa_id", "nombre" },
                unique: true,
                filter: "eliminado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_suscripcion_plan_id",
                table: "suscripcion",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ux_suscripcion_empresa_id_activa",
                table: "suscripcion",
                column: "empresa_id",
                unique: true,
                filter: "estado = 'Activa'");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_empresa_id_rol_id",
                table: "usuario_rol",
                columns: new[] { "empresa_id", "rol_id" });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_empresa_id_sucursal_id",
                table: "usuario_rol",
                columns: new[] { "empresa_id", "sucursal_id" });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_empresa_id_usuario_id",
                table: "usuario_rol",
                columns: new[] { "empresa_id", "usuario_id" });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_usuario_id",
                table: "usuario_rol",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ux_usuario_rol_todas_sucursales",
                table: "usuario_rol",
                columns: new[] { "usuario_id", "empresa_id", "rol_id" },
                unique: true,
                filter: "sucursal_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_usuario_rol_una_sucursal",
                table: "usuario_rol",
                columns: new[] { "usuario_id", "empresa_id", "rol_id", "sucursal_id" },
                unique: true,
                filter: "sucursal_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispositivo_tablet");

            migrationBuilder.DropTable(
                name: "personal");

            migrationBuilder.DropTable(
                name: "rol_permiso");

            migrationBuilder.DropTable(
                name: "suscripcion");

            migrationBuilder.DropTable(
                name: "usuario_rol");

            migrationBuilder.DropTable(
                name: "area");

            migrationBuilder.DropTable(
                name: "permiso");

            migrationBuilder.DropTable(
                name: "plan");

            migrationBuilder.DropTable(
                name: "rol");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "sucursal");

            migrationBuilder.DropTable(
                name: "empresa");
        }
    }
}
