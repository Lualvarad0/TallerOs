namespace TallerOS.Application.Comun.Cqrs;

/// <summary>Respuesta de un Command que no necesita devolver nada.</summary>
public readonly record struct Unit
{
    public static readonly Unit Valor = default;
}
