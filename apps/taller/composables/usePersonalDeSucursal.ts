import type { PersonalDto } from '@talleros/api-client';

/** Cascada sucursal→área→personal: trae el personal con acceso de todas las áreas de una
 * sucursal. Usado por Recepción (elegir asesor) y el Detalle de OT (elegir responsable de
 * subtarea), que necesitan la misma lista. */
export function usePersonalDeSucursal() {
  const api = useApi();

  async function listar(sucursalId: string): Promise<PersonalDto[]> {
    const areas = await api.areas.listar(sucursalId);
    const porArea = await Promise.all(areas.map((a) => api.personal.listar(a.id)));
    return porArea.flat().filter((p) => p.tieneAcceso);
  }

  return { listar };
}
