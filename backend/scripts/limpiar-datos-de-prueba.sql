-- Borra los huérfanos que deja RegistrarVehiculoCommandTests (ver su comentario de clase) al
-- correr contra Postgres real: la prueba de la carrera de dos registros simultáneos necesita un
-- COMMIT real (sin eso no hay fila contra la cual el índice único pueda chocar), así que a
-- propósito deja un vehiculo + su marca/modelo de prueba + una vehiculo_placa huérfanos cada vez
-- que corre. No se puede evitar el huérfano borrándolo desde la propia prueba: vehiculo y
-- vehiculo_placa no tienen ninguna política RLS de DELETE (a propósito, ver Modulo3RlsPoliticas),
-- así que con FORCE ROW LEVEL SECURITY ni el rol de la Api ni este mismo rol de prueba pueden
-- borrar esas filas aunque quisieran — solo un rol que de verdad bypassee RLS (el superusuario
-- `postgres` del contenedor) puede. Por eso esta limpieza es un script aparte para correr a mano
-- (o en un hook del entorno de desarrollo), no código dentro de la prueba.
--
-- Seguro de re-correr: solo toca marca/modelo con el nombre exacto que pone SembrarMarcaYModeloAsync
-- ("Marca de prueba <guid>" / "Modelo de prueba <guid>"), nunca marcas reales. Si una fila
-- coincidiera por nombre pero ya tuviera un cliente_vehiculo real vinculado, igual se borraría ese
-- vínculo — no debería pasar nunca (esta prueba corre en una empresa de prueba y nunca llega a
-- vincular cliente con el vehículo que deja huérfano), pero el SELECT de abajo lo muestra antes de
-- confirmar nada, para poder revisar manualmente si alguna vez aparece.
--
-- Uso (desde backend/, con el Postgres de docker-compose corriendo; -f no sirve aquí porque
-- resolvería la ruta DENTRO del contenedor, no en el host, por eso se manda por stdin con `<`):
--   docker exec -i talleros-postgres-1 psql -U postgres -d talleros -v ON_ERROR_STOP=1 < scripts/limpiar-datos-de-prueba.sql

\echo 'Marcas de prueba a borrar (revisar antes de que el COMMIT final confirme):'
select m.id, m.nombre, count(distinct v.id) as vehiculos, count(distinct cv.id) as vinculos_de_cliente
from marca m
left join vehiculo v on v.marca_id = m.id
left join cliente_vehiculo cv on cv.vehiculo_id = v.id
where m.nombre like 'Marca de prueba%' or m.nombre like 'MarcaTest%'
group by m.id, m.nombre
order by m.nombre;

begin;

create temporary table marcas_de_prueba as
select id from marca where nombre like 'Marca de prueba%' or nombre like 'MarcaTest%';

delete from vehiculo_placa
where vehiculo_id in (select id from vehiculo where marca_id in (select id from marcas_de_prueba));

delete from cliente_vehiculo
where vehiculo_id in (select id from vehiculo where marca_id in (select id from marcas_de_prueba));

delete from vehiculo where marca_id in (select id from marcas_de_prueba);

delete from modelo where marca_id in (select id from marcas_de_prueba);

delete from marca where id in (select id from marcas_de_prueba);

commit;

\echo 'Listo.'
