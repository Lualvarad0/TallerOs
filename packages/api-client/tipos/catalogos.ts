export interface TipoVehiculoDto {
  id: number;
  nombre: string;
}

export interface MarcaDto {
  id: string;
  tipoVehiculoId: number;
  nombre: string;
}

export interface ModeloDto {
  id: string;
  marcaId: string;
  nombre: string;
}
