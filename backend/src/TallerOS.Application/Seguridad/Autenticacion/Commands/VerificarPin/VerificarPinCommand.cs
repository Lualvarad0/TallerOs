using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.VerificarPin;

/// <summary>Solo identifica a la persona que está frente a la tablet (TokenDispositivo autoriza al
/// dispositivo, Pin identifica a la persona). No crea ningún registro de jornada/fichaje — eso es
/// de Módulo 5 (Tiempos y jornada), no de este módulo.</summary>
public sealed record VerificarPinCommand(string TokenDispositivo, string Pin) : IRequest<VerificarPinResultado>;

public sealed record VerificarPinResultado(Guid PersonalId, string Nombre);
