namespace TallerOS.Application.Comun.Excepciones;

/// <summary>La Api la traduce a 409. Para reglas que el handler puede chequear antes de escribir
/// (ej. nombre duplicado) en vez de depender del mensaje crudo de una violación de índice único.</summary>
public sealed class ConflictoDeNegocioException(string mensaje) : Exception(mensaje);
