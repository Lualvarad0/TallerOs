export interface RolDto {
  id: string;
  nombre: string;
  permisos: string[];
}

export interface PermisoDto {
  codigo: string;
  nombre: string;
  descripcion: string | null;
}

export interface UsuarioRolDto {
  id: string;
  usuarioId: string;
  email: string;
  rolId: string;
  rol: string;
  sucursalId: string | null;
}

export interface DispositivoTabletDto {
  id: string;
  sucursalId: string;
  nombre: string;
  activo: boolean;
  ultimoUsoEn: string | null;
}

/** Token en texto plano: el backend solo lo devuelve una vez, al registrar el dispositivo. */
export interface RegistrarDispositivoTabletResultado {
  dispositivoId: string;
  token: string;
}
