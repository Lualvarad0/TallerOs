using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <summary>Segunda barrera de aislamiento (la primera es el filtro global de EF Core): si algún
    /// día ese filtro se olvida en una consulta, PostgreSQL igual bloquea las filas de otra empresa.
    ///
    /// app.empresa_id y app.es_super_admin son variables de sesión que fija TenantSessionInterceptor
    /// en cada conexión (ver Infrastructure/Persistence). FORCE ROW LEVEL SECURITY hace que la
    /// política aplique también al propietario de la tabla (talleros_migrator); por eso la API NUNCA
    /// debe conectarse con ese rol, solo con talleros_app (ver infra/postgres/init/02-roles.sh).
    ///
    /// NULLIF(..., '') antes del cast a uuid: si app.empresa_id no está fijada (sesión sin empresa,
    /// ej. antes de iniciar sesión), el interceptor la deja en '' — castear '' a uuid directamente
    /// lanzaría un error en cada consulta en vez de simplemente no devolver filas.</summary>
    public partial class Modulo1RlsPoliticas : Migration
    {
        private static readonly string[] TablasPorEmpresaId =
        [
            "rol", "sucursal", "suscripcion", "area", "dispositivo_tablet", "usuario_rol", "personal",
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var tabla in TablasPorEmpresaId)
            {
                migrationBuilder.Sql($"""
                    ALTER TABLE {tabla} ENABLE ROW LEVEL SECURITY;
                    ALTER TABLE {tabla} FORCE ROW LEVEL SECURITY;
                    CREATE POLICY aislamiento_empresa ON {tabla}
                        USING (
                            current_setting('app.es_super_admin', true) = 'true'
                            OR empresa_id = NULLIF(current_setting('app.empresa_id', true), '')::uuid
                        )
                        WITH CHECK (
                            current_setting('app.es_super_admin', true) = 'true'
                            OR empresa_id = NULLIF(current_setting('app.empresa_id', true), '')::uuid
                        );
                    """);
            }

            // empresa no tiene empresa_id propio: la fila ES la empresa, se compara contra su id.
            migrationBuilder.Sql("""
                ALTER TABLE empresa ENABLE ROW LEVEL SECURITY;
                ALTER TABLE empresa FORCE ROW LEVEL SECURITY;
                CREATE POLICY aislamiento_empresa ON empresa
                    USING (
                        current_setting('app.es_super_admin', true) = 'true'
                        OR id = NULLIF(current_setting('app.empresa_id', true), '')::uuid
                    )
                    WITH CHECK (
                        current_setting('app.es_super_admin', true) = 'true'
                        OR id = NULLIF(current_setting('app.empresa_id', true), '')::uuid
                    );
                """);

            // rol_permiso no tiene empresa_id propio: el aislamiento llega por su rol_id.
            migrationBuilder.Sql("""
                ALTER TABLE rol_permiso ENABLE ROW LEVEL SECURITY;
                ALTER TABLE rol_permiso FORCE ROW LEVEL SECURITY;
                CREATE POLICY aislamiento_empresa ON rol_permiso
                    USING (
                        current_setting('app.es_super_admin', true) = 'true'
                        OR EXISTS (
                            SELECT 1 FROM rol
                            WHERE rol.id = rol_permiso.rol_id
                              AND rol.empresa_id = NULLIF(current_setting('app.empresa_id', true), '')::uuid
                        )
                    )
                    WITH CHECK (
                        current_setting('app.es_super_admin', true) = 'true'
                        OR EXISTS (
                            SELECT 1 FROM rol
                            WHERE rol.id = rol_permiso.rol_id
                              AND rol.empresa_id = NULLIF(current_setting('app.empresa_id', true), '')::uuid
                        )
                    );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var tabla in TablasPorEmpresaId.Append("empresa").Append("rol_permiso"))
            {
                migrationBuilder.Sql($"""
                    DROP POLICY IF EXISTS aislamiento_empresa ON {tabla};
                    ALTER TABLE {tabla} NO FORCE ROW LEVEL SECURITY;
                    ALTER TABLE {tabla} DISABLE ROW LEVEL SECURITY;
                    """);
            }
        }
    }
}
