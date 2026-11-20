namespace TallerOS.Application.Comun.Excepciones;

/// <summary>La Api la traduce a 404. No expone detalles del id a propósito en el mensaje por defecto;
/// cada handler puede agregar contexto si hace falta.</summary>
public sealed class EntidadNoEncontradaException(string entidad, object id)
    : Exception($"{entidad} '{id}' no existe (o no pertenece a esta empresa).");
