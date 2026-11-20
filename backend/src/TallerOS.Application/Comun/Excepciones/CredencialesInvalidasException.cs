namespace TallerOS.Application.Comun.Excepciones;

/// <summary>La Api la traduce a 401. Un solo mensaje genérico para no revelar si falló el email,
/// la contraseña, el token de dispositivo o el PIN.</summary>
public sealed class CredencialesInvalidasException() : Exception("Credenciales inválidas.");
