export interface ClaimsSesion {
  usuarioId: string;
  empresaId: string | null;
  esSuperAdmin: boolean;
  tipoToken: 'preauth' | 'sesion';
  /** Segundos desde epoch (claim estándar "exp"). */
  expiraEn: number | null;
}

/** Decodifica el payload de un JWT sin verificar la firma (eso ya lo hizo el servidor); solo lee
 * los claims para mostrarlos/usarlos en el cliente. Nunca confiar en esto para autorizar nada. */
export function decodificarJwt(token: string): ClaimsSesion | null {
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;
    const json = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    return {
      usuarioId: json.usuario_id,
      empresaId: json.empresa_id ?? null,
      esSuperAdmin: json.es_super_admin === 'true' || json.es_super_admin === true,
      tipoToken: json.tipo_token,
      expiraEn: typeof json.exp === 'number' ? json.exp : null,
    };
  } catch {
    return null;
  }
}

export function tokenExpirado(claims: ClaimsSesion | null): boolean {
  if (!claims?.expiraEn) return true;
  return claims.expiraEn * 1000 <= Date.now();
}
