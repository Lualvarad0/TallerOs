export interface EmpresaAccesible {
  empresaId: string;
  nombreEmpresa: string;
}

/** Exactamente uno de los dos tokens viene con valor (ver IniciarSesionResultado en el backend). */
export interface IniciarSesionResultado {
  tokenSesion: string | null;
  tokenPreAutenticacion: string | null;
  empresas: EmpresaAccesible[];
}

export interface SeleccionarEmpresaResultado {
  tokenSesion: string;
}

export interface VerificarPinResultado {
  personalId: string;
  nombre: string;
}
