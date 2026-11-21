import type { $Fetch } from 'ofetch';
import { crearRecursoAreas } from './recursos/areas';
import { crearRecursoAuth } from './recursos/auth';
import { crearRecursoClientes } from './recursos/clientes';
import { crearRecursoDispositivosTablet } from './recursos/dispositivosTablet';
import { crearRecursoEmpresas } from './recursos/empresas';
import { crearRecursoEstadosOt } from './recursos/estadosOt';
import { crearRecursoMarcas } from './recursos/marcas';
import { crearRecursoModelos } from './recursos/modelos';
import { crearRecursoOrdenesTrabajo } from './recursos/ordenesTrabajo';
import { crearRecursoPersonal } from './recursos/personal';
import { crearRecursoRecepcion } from './recursos/recepcion';
import { crearRecursoRoles } from './recursos/roles';
import { crearRecursoSubtareas } from './recursos/subtareas';
import { crearRecursoSucursales } from './recursos/sucursales';
import { crearRecursoTiposTrabajo } from './recursos/tiposTrabajo';
import { crearRecursoTiposVehiculo } from './recursos/tiposVehiculo';
import { crearRecursoUsuarios } from './recursos/usuarios';
import { crearRecursoVehiculos } from './recursos/vehiculos';

/**
 * No depende de Nuxt ni de Vue: recibe cualquier instancia de `ofetch` ya configurada (baseURL,
 * token, manejo de 401) y devuelve funciones tipadas por recurso. `apps/taller` crea esa instancia
 * en `plugins/api.client.ts`; `apps/mi-garage` la creará igual cuando exista.
 */
export function crearClienteApi(fetch: $Fetch) {
  return {
    auth: crearRecursoAuth(fetch),
    empresas: crearRecursoEmpresas(fetch),
    sucursales: crearRecursoSucursales(fetch),
    areas: crearRecursoAreas(fetch),
    personal: crearRecursoPersonal(fetch),
    roles: crearRecursoRoles(fetch),
    usuarios: crearRecursoUsuarios(fetch),
    dispositivosTablet: crearRecursoDispositivosTablet(fetch),
    tiposVehiculo: crearRecursoTiposVehiculo(fetch),
    marcas: crearRecursoMarcas(fetch),
    modelos: crearRecursoModelos(fetch),
    clientes: crearRecursoClientes(fetch),
    vehiculos: crearRecursoVehiculos(fetch),
    tiposTrabajo: crearRecursoTiposTrabajo(fetch),
    estadosOt: crearRecursoEstadosOt(fetch),
    recepcion: crearRecursoRecepcion(fetch),
    ordenesTrabajo: crearRecursoOrdenesTrabajo(fetch),
    subtareas: crearRecursoSubtareas(fetch),
  };
}

export type ClienteApi = ReturnType<typeof crearClienteApi>;
