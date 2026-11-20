using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.Clientes;

namespace TallerOS.Application.Clientes.Commands.ActualizarCliente;

/// <summary>Tipo e Identificacion no se editan aquí: Cliente.ActualizarDatos no los toca (ver
/// Cliente), porque cambiar de persona natural a empresa, o la cédula/RUC de alguien, no es una
/// "actualización de datos" sino, de hecho, otra persona — eso requeriría dar de baja este cliente
/// y crear uno nuevo, no editar el existente.</summary>
public sealed record ActualizarClienteCommand(
    Guid ClienteId,
    string Nombre,
    string? Telefono,
    string? Email,
    CanalPreferido CanalPreferido) : IRequest<Unit>;
