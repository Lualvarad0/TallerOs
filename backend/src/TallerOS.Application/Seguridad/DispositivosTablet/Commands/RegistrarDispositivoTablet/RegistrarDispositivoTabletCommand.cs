using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.DispositivosTablet.Commands.RegistrarDispositivoTablet;

public sealed record RegistrarDispositivoTabletCommand(Guid SucursalId, string Nombre) : IRequest<RegistrarDispositivoTabletResultado>;

/// <summary>Token en texto plano: solo se devuelve esta vez. Lo que queda guardado es su hash
/// (TokenHash); si se pierde, hay que revocar el dispositivo y registrar uno nuevo.</summary>
public sealed record RegistrarDispositivoTabletResultado(Guid DispositivoId, string Token);
