namespace TallerOS.Application.Comun.Cqrs;

/// <summary>Marca un Command o Query. No hay distinción de tipos entre los dos: la convención de
/// nombres (Crear/Actualizar/... vs. Obtener/Listar) ya lo deja claro.</summary>
public interface IRequest<TRespuesta>;
