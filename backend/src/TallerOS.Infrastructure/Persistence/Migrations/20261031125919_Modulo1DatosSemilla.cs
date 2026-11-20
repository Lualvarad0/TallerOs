using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Modulo1DatosSemilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las políticas RLS de la migración anterior (FORCE ROW LEVEL SECURITY) bloquean estos
            // INSERT incluso para talleros_migrator, porque son filas de una empresa concreta y la
            // sesión de `dotnet ef database update` no tiene app.empresa_id fijado. Se marca la
            // sesión como super admin solo para esta migración de semilla.
            migrationBuilder.Sql("SELECT set_config('app.es_super_admin', 'true', false);");

            migrationBuilder.InsertData(
                table: "empresa",
                columns: new[] { "id", "creado_en", "eliminado_en", "estado", "nombre", "ruc" },
                values: new object[] { new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Activa", "Taller Demo", "1790012345001" });

            migrationBuilder.InsertData(
                table: "permiso",
                columns: new[] { "codigo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { "area.gestionar", "Crear, editar y desactivar áreas dentro de una sucursal", "Gestionar áreas" },
                    { "dispositivo_tablet.gestionar", "Registrar o revocar tablets compartidas para fichar con PIN", "Gestionar tablets" },
                    { "empresa.configurar", "Editar los datos y el estado de la empresa", "Configurar empresa" },
                    { "personal.gestionar", "Crear, editar y desactivar personal (con o sin acceso)", "Gestionar personal" },
                    { "rol.gestionar", "Crear roles y asignarles permisos", "Gestionar roles" },
                    { "sucursal.gestionar", "Crear, editar y desactivar sucursales", "Gestionar sucursales" },
                    { "usuario_rol.asignar", "Dar o quitar un rol a un usuario, por sucursal o para toda la empresa", "Asignar roles a usuarios" },
                    { "usuario.gestionar", "Crear cuentas de usuario e invitar personal", "Gestionar usuarios" }
                });

            migrationBuilder.InsertData(
                table: "plan",
                columns: new[] { "id", "activo", "limite_sucursales", "limite_usuarios", "nombre", "permite_whatsapp_oficial" },
                values: new object[,]
                {
                    { new Guid("265aad84-c2a0-4e88-b9a8-d069b5c54dc4"), true, 3, 15, "Pro", true },
                    { new Guid("916d3c96-5570-4817-9dcb-b21060472d9b"), true, 1, 3, "Básico", false },
                    { new Guid("b7b0a891-60cc-45ca-9cd2-99a9739dd529"), true, null, null, "Empresa", true }
                });

            migrationBuilder.InsertData(
                table: "usuario",
                columns: new[] { "id", "activo", "creado_en", "email", "es_super_admin", "password_hash" },
                values: new object[] { new Guid("ce894d4e-9e31-492c-8230-8dba05250a94"), true, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "dueno@tallerdemo.test", false, "seed$no-es-un-hash-real$reemplazar-en-paso-3" });

            migrationBuilder.InsertData(
                table: "rol",
                columns: new[] { "id", "eliminado_en", "empresa_id", "nombre" },
                values: new object[,]
                {
                    { new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e"), null, new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Dueño/Gerente" },
                    { new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0"), null, new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Jefe de sucursal" }
                });

            migrationBuilder.InsertData(
                table: "sucursal",
                columns: new[] { "id", "direccion", "eliminado_en", "empresa_id", "nombre" },
                values: new object[] { new Guid("c648727f-1153-4fd1-b06a-3c1d6c1040cc"), "Av. Siempre Viva 123, Quito", null, new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Principal" });

            migrationBuilder.InsertData(
                table: "suscripcion",
                columns: new[] { "id", "empresa_id", "estado", "plan_id", "vigente_desde", "vigente_hasta" },
                values: new object[] { new Guid("a5e5f8f6-a35b-4ffc-ad0d-0fddc519bcf2"), new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Activa", new Guid("265aad84-c2a0-4e88-b9a8-d069b5c54dc4"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null });

            migrationBuilder.InsertData(
                table: "area",
                columns: new[] { "id", "eliminado_en", "empresa_id", "nombre", "sucursal_id" },
                values: new object[,]
                {
                    { new Guid("12ce199f-0fb5-4b45-a0ec-6f6ad26c2a92"), null, new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Mecánica", new Guid("c648727f-1153-4fd1-b06a-3c1d6c1040cc") },
                    { new Guid("b7ae6d86-0fd3-40d9-bc12-c31f6a37011f"), null, new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Administración", new Guid("c648727f-1153-4fd1-b06a-3c1d6c1040cc") }
                });

            migrationBuilder.InsertData(
                table: "dispositivo_tablet",
                columns: new[] { "id", "activo", "creado_en", "empresa_id", "nombre", "sucursal_id", "token_hash", "ultimo_uso_en" },
                values: new object[] { new Guid("2d394826-38a4-45ea-8e53-eb9c4c72239c"), true, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Tablet recepción", new Guid("c648727f-1153-4fd1-b06a-3c1d6c1040cc"), "seed$no-es-un-token-real$reemplazar-en-paso-3", null });

            migrationBuilder.InsertData(
                table: "rol_permiso",
                columns: new[] { "permiso_codigo", "rol_id" },
                values: new object[,]
                {
                    { "area.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "dispositivo_tablet.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "empresa.configurar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "personal.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "rol.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "sucursal.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "usuario_rol.asignar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "usuario.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") },
                    { "area.gestionar", new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0") },
                    { "dispositivo_tablet.gestionar", new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0") },
                    { "personal.gestionar", new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0") }
                });

            migrationBuilder.InsertData(
                table: "usuario_rol",
                columns: new[] { "id", "empresa_id", "rol_id", "sucursal_id", "usuario_id" },
                values: new object[] { new Guid("47a1a500-9a46-4bf0-a2ed-62f8f8947158"), new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e"), null, new Guid("ce894d4e-9e31-492c-8230-8dba05250a94") });

            migrationBuilder.InsertData(
                table: "personal",
                columns: new[] { "id", "area_id", "eliminado_en", "empresa_id", "nombre", "pin_hash", "tarifa_hora", "tiene_acceso", "usuario_id" },
                values: new object[,]
                {
                    { new Guid("b040b030-bef2-4bf6-8a40-a5fce884f306"), new Guid("12ce199f-0fb5-4b45-a0ec-6f6ad26c2a92"), null, new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Pedro Mecánico", "seed$no-es-un-hash-real$reemplazar-en-paso-3", 4.50m, false, null },
                    { new Guid("d0ad8cc9-30b6-4ddf-a583-5ff660b91ce7"), new Guid("b7ae6d86-0fd3-40d9-bc12-c31f6a37011f"), null, new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"), "Ana Dueña", null, null, true, new Guid("ce894d4e-9e31-492c-8230-8dba05250a94") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SELECT set_config('app.es_super_admin', 'true', false);");

            migrationBuilder.DeleteData(
                table: "dispositivo_tablet",
                keyColumn: "id",
                keyValue: new Guid("2d394826-38a4-45ea-8e53-eb9c4c72239c"));

            migrationBuilder.DeleteData(
                table: "personal",
                keyColumn: "id",
                keyValue: new Guid("b040b030-bef2-4bf6-8a40-a5fce884f306"));

            migrationBuilder.DeleteData(
                table: "personal",
                keyColumn: "id",
                keyValue: new Guid("d0ad8cc9-30b6-4ddf-a583-5ff660b91ce7"));

            migrationBuilder.DeleteData(
                table: "plan",
                keyColumn: "id",
                keyValue: new Guid("916d3c96-5570-4817-9dcb-b21060472d9b"));

            migrationBuilder.DeleteData(
                table: "plan",
                keyColumn: "id",
                keyValue: new Guid("b7b0a891-60cc-45ca-9cd2-99a9739dd529"));

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "area.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "dispositivo_tablet.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "empresa.configurar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "personal.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "rol.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "sucursal.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "usuario_rol.asignar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "usuario.gestionar", new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "area.gestionar", new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "dispositivo_tablet.gestionar", new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0") });

            migrationBuilder.DeleteData(
                table: "rol_permiso",
                keyColumns: new[] { "permiso_codigo", "rol_id" },
                keyValues: new object[] { "personal.gestionar", new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0") });

            migrationBuilder.DeleteData(
                table: "suscripcion",
                keyColumn: "id",
                keyValue: new Guid("a5e5f8f6-a35b-4ffc-ad0d-0fddc519bcf2"));

            migrationBuilder.DeleteData(
                table: "usuario_rol",
                keyColumn: "id",
                keyValue: new Guid("47a1a500-9a46-4bf0-a2ed-62f8f8947158"));

            migrationBuilder.DeleteData(
                table: "area",
                keyColumn: "id",
                keyValue: new Guid("12ce199f-0fb5-4b45-a0ec-6f6ad26c2a92"));

            migrationBuilder.DeleteData(
                table: "area",
                keyColumn: "id",
                keyValue: new Guid("b7ae6d86-0fd3-40d9-bc12-c31f6a37011f"));

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "area.gestionar");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "dispositivo_tablet.gestionar");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "empresa.configurar");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "personal.gestionar");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "rol.gestionar");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "sucursal.gestionar");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "usuario_rol.asignar");

            migrationBuilder.DeleteData(
                table: "permiso",
                keyColumn: "codigo",
                keyValue: "usuario.gestionar");

            migrationBuilder.DeleteData(
                table: "plan",
                keyColumn: "id",
                keyValue: new Guid("265aad84-c2a0-4e88-b9a8-d069b5c54dc4"));

            migrationBuilder.DeleteData(
                table: "rol",
                keyColumn: "id",
                keyValue: new Guid("29d9f9bd-6850-4547-8bfc-7a809c61534e"));

            migrationBuilder.DeleteData(
                table: "rol",
                keyColumn: "id",
                keyValue: new Guid("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0"));

            migrationBuilder.DeleteData(
                table: "usuario",
                keyColumn: "id",
                keyValue: new Guid("ce894d4e-9e31-492c-8230-8dba05250a94"));

            migrationBuilder.DeleteData(
                table: "sucursal",
                keyColumn: "id",
                keyValue: new Guid("c648727f-1153-4fd1-b06a-3c1d6c1040cc"));

            migrationBuilder.DeleteData(
                table: "empresa",
                keyColumn: "id",
                keyValue: new Guid("e598ae6b-3823-446a-b9b3-3229deb3d134"));
        }
    }
}
