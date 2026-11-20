// Formato de placa ecuatoriana (ANT): "ABC-1234" para autos/camionetas/camiones, "AB-1234" para
// motos. Es solo una advertencia, no un bloqueo: la placa puede cambiar de formato con el tiempo
// (la de la ANT no es la identidad del vehículo, el VIN sí) y hay excepciones oficiales
// (diplomáticas, de prueba, históricas) que no siguen este patrón.
const FORMATO = /^[A-Z]{2,3}-\d{3,4}$/;

export function placaTieneFormatoReconocido(placa: string): boolean {
  return FORMATO.test(placa.toUpperCase());
}
