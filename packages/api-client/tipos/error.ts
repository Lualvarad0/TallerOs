/** Forma que devuelve ManejoDeExcepcionesMiddleware (backend) en cualquier error. */
export interface ProblemaApi {
  status?: number;
  title?: string;
  detail?: string;
  errores?: { campo: string; mensaje: string }[];
}
