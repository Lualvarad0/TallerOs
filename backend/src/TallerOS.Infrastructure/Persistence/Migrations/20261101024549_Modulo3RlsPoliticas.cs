using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TallerOS.Infrastructure.Persistence.Migrations
{
    /// <summary>Segunda barrera de aislamiento para el módulo de clientes y vehículos (ver también
    /// Modulo1RlsPoliticas). Dos grupos de tablas, con reglas distintas:
    ///
    /// - `cliente` y `cliente_vehiculo` tienen empresa_id propio: misma política única por tabla
    ///   (aislamiento_empresa) que el resto de tablas de empresa. Que un cliente de la empresa A no
    ///   pueda vincularse al cliente de la empresa B ya lo garantiza la FK compuesta de
    ///   cliente_vehiculo contra (empresa_id, id) de cliente (ver ClienteVehiculoConfiguration), no
    ///   esta migración.
    ///
    /// - `vehiculo`, `vehiculo_placa`, `vehiculo_propietario` y `cuenta_cliente` son catálogos
    ///   globales (sin empresa_id, ver docs/convenciones.md § Seguridad): la empresa que puede verlos no es la
    ///   que los creó, sino la que tiene un cliente_vehiculo vigente (o, para cuenta_cliente, un
    ///   cliente vinculado directamente). Por eso sus políticas van separadas por comando en vez de
    ///   una sola USING/WITH CHECK:
    ///     * SELECT: el EXISTS contra cliente_vehiculo (o cliente, para cuenta_cliente).
    ///     * INSERT en vehiculo: sin condición (WITH CHECK (true)) porque nada puede exigirle una
    ///       relación a una fila que todavía no existe; es la única excepción real de esta migración.
    ///     * INSERT en vehiculo_placa: exige el mismo EXISTS que el SELECT, porque para cuando se
    ///       inserta la placa el cliente_vehiculo de esa misma operación ya existe (RegistrarVehiculo
    ///       los inserta en una sola transacción, vehiculo → cliente_vehiculo → vehiculo_placa).
    ///     * UPDATE/DELETE: sin política en ninguna de las cuatro. Nada en este módulo modifica ni
    ///       elimina un vehículo, una placa o una propiedad ya existente, así que no se habilita
    ///       todavía: tener un vínculo da derecho a consultar, no a editar un dato que la empresa
    ///       comparte con otras. vehiculo_propietario y cuenta_cliente tampoco tienen política de
    ///       INSERT: nada las escribe aún (ver VehiculoPropietario y CuentaCliente en Domain).
    ///
    /// app.empresa_id y app.es_super_admin son las mismas variables de sesión de Modulo1RlsPoliticas
    /// (ver TenantSessionInterceptor); NULLIF(..., '') antes del cast a uuid por la misma razón de
    /// ahí: una sesión sin empresa deja esa variable en '', y castear '' a uuid directamente
    /// lanzaría un error en cada consulta en vez de simplemente no devolver filas.</summary>
    public partial class Modulo3RlsPoliticas : Migration
    {
        private const string EsSuperAdmin = "current_setting('app.es_super_admin', true) = 'true'";
        private const string EmpresaActual = "NULLIF(current_setting('app.empresa_id', true), '')::uuid";

        private static readonly string[] TablasPorEmpresaId = ["cliente", "cliente_vehiculo"];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var tabla in TablasPorEmpresaId)
            {
                migrationBuilder.Sql($"""
                    ALTER TABLE {tabla} ENABLE ROW LEVEL SECURITY;
                    ALTER TABLE {tabla} FORCE ROW LEVEL SECURITY;
                    CREATE POLICY aislamiento_empresa ON {tabla}
                        USING ({EsSuperAdmin} OR empresa_id = {EmpresaActual})
                        WITH CHECK ({EsSuperAdmin} OR empresa_id = {EmpresaActual});
                    """);
            }

            migrationBuilder.Sql($"""
                ALTER TABLE vehiculo ENABLE ROW LEVEL SECURITY;
                ALTER TABLE vehiculo FORCE ROW LEVEL SECURITY;

                CREATE POLICY seleccionar ON vehiculo FOR SELECT
                    USING (
                        {EsSuperAdmin}
                        OR EXISTS (
                            SELECT 1 FROM cliente_vehiculo
                            WHERE cliente_vehiculo.vehiculo_id = vehiculo.id
                              AND cliente_vehiculo.empresa_id = {EmpresaActual}
                        )
                    );

                CREATE POLICY insertar ON vehiculo FOR INSERT
                    WITH CHECK (true);
                """);

            migrationBuilder.Sql($"""
                ALTER TABLE vehiculo_placa ENABLE ROW LEVEL SECURITY;
                ALTER TABLE vehiculo_placa FORCE ROW LEVEL SECURITY;

                CREATE POLICY seleccionar ON vehiculo_placa FOR SELECT
                    USING (
                        {EsSuperAdmin}
                        OR EXISTS (
                            SELECT 1 FROM cliente_vehiculo
                            WHERE cliente_vehiculo.vehiculo_id = vehiculo_placa.vehiculo_id
                              AND cliente_vehiculo.empresa_id = {EmpresaActual}
                        )
                    );

                CREATE POLICY insertar ON vehiculo_placa FOR INSERT
                    WITH CHECK (
                        {EsSuperAdmin}
                        OR EXISTS (
                            SELECT 1 FROM cliente_vehiculo
                            WHERE cliente_vehiculo.vehiculo_id = vehiculo_placa.vehiculo_id
                              AND cliente_vehiculo.empresa_id = {EmpresaActual}
                        )
                    );
                """);

            migrationBuilder.Sql($"""
                ALTER TABLE vehiculo_propietario ENABLE ROW LEVEL SECURITY;
                ALTER TABLE vehiculo_propietario FORCE ROW LEVEL SECURITY;

                CREATE POLICY seleccionar ON vehiculo_propietario FOR SELECT
                    USING ({EsSuperAdmin});
                """);

            migrationBuilder.Sql($"""
                ALTER TABLE cuenta_cliente ENABLE ROW LEVEL SECURITY;
                ALTER TABLE cuenta_cliente FORCE ROW LEVEL SECURITY;

                CREATE POLICY seleccionar ON cuenta_cliente FOR SELECT
                    USING (
                        {EsSuperAdmin}
                        OR EXISTS (
                            SELECT 1 FROM cliente
                            WHERE cliente.cuenta_cliente_id = cuenta_cliente.id
                              AND cliente.empresa_id = {EmpresaActual}
                        )
                    );
                """);
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

            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS seleccionar ON vehiculo;
                DROP POLICY IF EXISTS insertar ON vehiculo;
                ALTER TABLE vehiculo NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE vehiculo DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS seleccionar ON vehiculo_placa;
                DROP POLICY IF EXISTS insertar ON vehiculo_placa;
                ALTER TABLE vehiculo_placa NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE vehiculo_placa DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS seleccionar ON vehiculo_propietario;
                ALTER TABLE vehiculo_propietario NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE vehiculo_propietario DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS seleccionar ON cuenta_cliente;
                ALTER TABLE cuenta_cliente NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE cuenta_cliente DISABLE ROW LEVEL SECURITY;
                """);
        }
    }
}
