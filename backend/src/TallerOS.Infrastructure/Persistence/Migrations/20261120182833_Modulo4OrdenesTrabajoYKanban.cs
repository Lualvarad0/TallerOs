using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Modulo4OrdenesTrabajoYKanban : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ak_personal_empresa_id_id",
                table: "personal",
                columns: new[] { "empresa_id", "id" });

            migrationBuilder.CreateTable(
                name: "adjunto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    entidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    miniatura_url = table.Column<string>(type: "text", nullable: true),
                    autor_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adjunto", x => x.id);
                    table.ForeignKey(
                        name: "fk_adjunto_usuarios_autor_usuario_id",
                        column: x => x.autor_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contador_numero_ot",
                columns: table => new
                {
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ultimo_numero = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contador_numero_ot", x => x.empresa_id);
                });

            migrationBuilder.CreateTable(
                name: "estado_ot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    orden = table.Column<short>(type: "smallint", nullable: false),
                    es_final = table.Column<bool>(type: "boolean", nullable: false),
                    color_token = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_ot", x => x.id);
                    table.UniqueConstraint("ak_estado_ot_empresa_id_id", x => new { x.empresa_id, x.id });
                });

            migrationBuilder.CreateTable(
                name: "tipo_trabajo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tiempo_estandar_min = table.Column<int>(type: "integer", nullable: false),
                    intervalo_km = table.Column<int>(type: "integer", nullable: true),
                    intervalo_meses = table.Column<int>(type: "integer", nullable: true),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_trabajo", x => x.id);
                    table.UniqueConstraint("ak_tipos_trabajo_empresa_id_id", x => new { x.empresa_id, x.id });
                });

            migrationBuilder.CreateTable(
                name: "orden_trabajo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prioridad = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    km_ingreso = table.Column<int>(type: "integer", nullable: false),
                    asesor_personal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ingreso_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    entregado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    eliminado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orden_trabajo", x => x.id);
                    table.UniqueConstraint("ak_ordenes_trabajo_empresa_id_id", x => new { x.empresa_id, x.id });
                    table.ForeignKey(
                        name: "fk_orden_trabajo_cliente_empresa_id_cliente_id",
                        columns: x => new { x.empresa_id, x.cliente_id },
                        principalTable: "cliente",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orden_trabajo_estado_ot_empresa_id_estado_id",
                        columns: x => new { x.empresa_id, x.estado_id },
                        principalTable: "estado_ot",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orden_trabajo_personal_empresa_id_asesor_personal_id",
                        columns: x => new { x.empresa_id, x.asesor_personal_id },
                        principalTable: "personal",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orden_trabajo_sucursales_empresa_id_sucursal_id",
                        columns: x => new { x.empresa_id, x.sucursal_id },
                        principalTable: "sucursal",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orden_trabajo_vehiculos_vehiculo_id",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historial_estado_ot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    de_estado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    a_estado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historial_estado_ot", x => x.id);
                    table.ForeignKey(
                        name: "fk_historial_estado_ot_estado_ot_empresa_id_a_estado_id",
                        columns: x => new { x.empresa_id, x.a_estado_id },
                        principalTable: "estado_ot",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_ot_estado_ot_empresa_id_de_estado_id",
                        columns: x => new { x.empresa_id, x.de_estado_id },
                        principalTable: "estado_ot",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_ot_ordenes_trabajo_empresa_id_ot_id",
                        columns: x => new { x.empresa_id, x.ot_id },
                        principalTable: "orden_trabajo",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_ot_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recepcion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    combustible = table.Column<short>(type: "smallint", nullable: false),
                    objetos = table.Column<string>(type: "text", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    firma_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recepcion", x => x.id);
                    table.UniqueConstraint("ak_recepciones_empresa_id_id", x => new { x.empresa_id, x.id });
                    table.ForeignKey(
                        name: "fk_recepcion_orden_trabajo_empresa_id_ot_id",
                        columns: x => new { x.empresa_id, x.ot_id },
                        principalTable: "orden_trabajo",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subtarea",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_trabajo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    responsable_personal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tiempo_estimado_min = table.Column<int>(type: "integer", nullable: false),
                    orden = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subtarea", x => x.id);
                    table.ForeignKey(
                        name: "fk_subtarea_orden_trabajo_empresa_id_ot_id",
                        columns: x => new { x.empresa_id, x.ot_id },
                        principalTable: "orden_trabajo",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subtarea_personal_empresa_id_responsable_personal_id",
                        columns: x => new { x.empresa_id, x.responsable_personal_id },
                        principalTable: "personal",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subtarea_tipos_trabajo_empresa_id_tipo_trabajo_id",
                        columns: x => new { x.empresa_id, x.tipo_trabajo_id },
                        principalTable: "tipo_trabajo",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "danio_previo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    zona = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    foto_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_danio_previo", x => x.id);
                    table.ForeignKey(
                        name: "fk_danio_previo_recepciones_empresa_id_recepcion_id",
                        columns: x => new { x.empresa_id, x.recepcion_id },
                        principalTable: "recepcion",
                        principalColumns: new[] { "empresa_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "contador_numero_ot",
                columns: new[] { "empresa_id", "ultimo_numero" },
                values: new object[] { new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), 0 });

            migrationBuilder.InsertData(
                table: "estado_ot",
                columns: new[] { "id", "color_token", "empresa_id", "es_final", "nombre", "orden" },
                values: new object[,]
                {
                    { new Guid("00000004-aaaa-4000-8000-000000000001"), "sf3", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), false, "Recibido", (short)1 },
                    { new Guid("00000004-aaaa-4000-8000-000000000002"), "blue", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), false, "Diagnóstico", (short)2 },
                    { new Guid("00000004-aaaa-4000-8000-000000000003"), "vio", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), false, "Esperando aprobación del cliente", (short)3 },
                    { new Guid("00000004-aaaa-4000-8000-000000000004"), "vio", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), false, "Esperando repuestos", (short)4 },
                    { new Guid("00000004-aaaa-4000-8000-000000000005"), "blue", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), false, "En reparación", (short)5 },
                    { new Guid("00000004-aaaa-4000-8000-000000000006"), "blue", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), false, "Control de calidad", (short)6 },
                    { new Guid("00000004-aaaa-4000-8000-000000000007"), "grn", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), false, "Listo para entregar", (short)7 },
                    { new Guid("00000004-aaaa-4000-8000-000000000008"), "grn", new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), true, "Entregado", (short)8 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_adjunto_autor_usuario_id",
                table: "adjunto",
                column: "autor_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_adjunto_empresa_id_entidad_entidad_id",
                table: "adjunto",
                columns: new[] { "empresa_id", "entidad", "entidad_id" });

            migrationBuilder.CreateIndex(
                name: "ix_danio_previo_empresa_id_recepcion_id",
                table: "danio_previo",
                columns: new[] { "empresa_id", "recepcion_id" });

            migrationBuilder.CreateIndex(
                name: "ix_estado_ot_empresa_id_orden",
                table: "estado_ot",
                columns: new[] { "empresa_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_ot_empresa_id_a_estado_id",
                table: "historial_estado_ot",
                columns: new[] { "empresa_id", "a_estado_id" });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_ot_empresa_id_de_estado_id",
                table: "historial_estado_ot",
                columns: new[] { "empresa_id", "de_estado_id" });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_ot_empresa_id_ot_id_fecha",
                table: "historial_estado_ot",
                columns: new[] { "empresa_id", "ot_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_ot_usuario_id",
                table: "historial_estado_ot",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_orden_trabajo_empresa_id_asesor_personal_id",
                table: "orden_trabajo",
                columns: new[] { "empresa_id", "asesor_personal_id" });

            migrationBuilder.CreateIndex(
                name: "ix_orden_trabajo_empresa_id_cliente_id",
                table: "orden_trabajo",
                columns: new[] { "empresa_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_orden_trabajo_empresa_id_estado_id",
                table: "orden_trabajo",
                columns: new[] { "empresa_id", "estado_id" })
                .Annotation("Npgsql:IndexInclude", new[] { "vehiculo_id", "cliente_id", "prioridad", "ingreso_en" });

            migrationBuilder.CreateIndex(
                name: "ix_orden_trabajo_empresa_id_numero",
                table: "orden_trabajo",
                columns: new[] { "empresa_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orden_trabajo_empresa_id_sucursal_id",
                table: "orden_trabajo",
                columns: new[] { "empresa_id", "sucursal_id" });

            migrationBuilder.CreateIndex(
                name: "ix_orden_trabajo_empresa_id_vehiculo_id",
                table: "orden_trabajo",
                columns: new[] { "empresa_id", "vehiculo_id" });

            migrationBuilder.CreateIndex(
                name: "ix_orden_trabajo_vehiculo_id",
                table: "orden_trabajo",
                column: "vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "ix_recepcion_empresa_id_ot_id",
                table: "recepcion",
                columns: new[] { "empresa_id", "ot_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subtarea_empresa_id_ot_id_orden",
                table: "subtarea",
                columns: new[] { "empresa_id", "ot_id", "orden" });

            migrationBuilder.CreateIndex(
                name: "ix_subtarea_empresa_id_responsable_personal_id",
                table: "subtarea",
                columns: new[] { "empresa_id", "responsable_personal_id" });

            migrationBuilder.CreateIndex(
                name: "ix_subtarea_empresa_id_tipo_trabajo_id",
                table: "subtarea",
                columns: new[] { "empresa_id", "tipo_trabajo_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tipo_trabajo_empresa_id_nombre",
                table: "tipo_trabajo",
                columns: new[] { "empresa_id", "nombre" },
                unique: true,
                filter: "eliminado_en IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adjunto");

            migrationBuilder.DropTable(
                name: "contador_numero_ot");

            migrationBuilder.DropTable(
                name: "danio_previo");

            migrationBuilder.DropTable(
                name: "historial_estado_ot");

            migrationBuilder.DropTable(
                name: "subtarea");

            migrationBuilder.DropTable(
                name: "recepcion");

            migrationBuilder.DropTable(
                name: "tipo_trabajo");

            migrationBuilder.DropTable(
                name: "orden_trabajo");

            migrationBuilder.DropTable(
                name: "estado_ot");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_personal_empresa_id_id",
                table: "personal");
        }
    }
}
