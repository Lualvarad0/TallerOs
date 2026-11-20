namespace TallerOS.Application.Organizacion.Empresas;

public sealed record EmpresaDto(Guid Id, string Nombre, string Ruc, string Estado, DateTimeOffset CreadoEn);
