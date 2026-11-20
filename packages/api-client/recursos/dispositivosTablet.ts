import type { $Fetch } from 'ofetch';
import type { DispositivoTabletDto, RegistrarDispositivoTabletResultado } from '../tipos/seguridad';

export function crearRecursoDispositivosTablet(fetch: $Fetch) {
  return {
    listar: () => fetch<DispositivoTabletDto[]>('/api/dispositivos-tablet'),

    registrar: (sucursalId: string, nombre: string) =>
      fetch<RegistrarDispositivoTabletResultado>('/api/dispositivos-tablet', { method: 'POST', body: { sucursalId, nombre } }),

    revocar: (dispositivoId: string) => fetch<void>(`/api/dispositivos-tablet/${dispositivoId}/revocar`, { method: 'POST' }),
  };
}
