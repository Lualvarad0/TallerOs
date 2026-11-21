using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <summary>Segunda barrera de aislamiento para el módulo de órdenes de trabajo y Kanban (ver
    /// también Modulo1RlsPoliticas). A diferencia de Modulo3RlsPoliticas, las nueve tablas de este
    /// módulo tienen todas su propio empresa_id (regla número uno de Seguridad en
    /// docs/convenciones.md: historial_estado_ot, recepcion, danio_previo, subtarea y adjunto lo
    /// llevan aunque cuelguen de una orden_trabajo, en vez de depender de un EXISTS contra ella), así
    /// que todas usan la misma política única aislamiento_empresa, sin excepciones por comando.</summary>
    public partial class Modulo4RlsPoliticas : Migration
    {
        private static readonly string[] TablasPorEmpresaId =
        [
            "estado_ot", "contador_numero_ot", "tipo_trabajo", "orden_trabajo", "historial_estado_ot",
            "recepcion", "subtarea", "danio_previo", "adjunto",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var tabla in TablasPorEmpresaId)
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
