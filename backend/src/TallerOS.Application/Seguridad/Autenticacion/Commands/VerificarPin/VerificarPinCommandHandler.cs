using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.VerificarPin;

public sealed class VerificarPinCommandHandler(IAppDbContext db, IHashService hash)
    : IRequestHandler<VerificarPinCommand, VerificarPinResultado>
{
    public async Task<VerificarPinResultado> Handle(VerificarPinCommand request, CancellationToken cancellationToken)
    {
        // Llamada anónima (la tablet aún no tiene empresa de sesión): misma elevación de RLS que
        // el resto de los flujos de autenticación.
        await db.ElevarPrivilegiosSinEmpresaAsync(cancellationToken);

        var tokenHash = hash.HashDeterministico(request.TokenDispositivo);
        var dispositivo = await db.DispositivosTablet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.TokenHash == tokenHash && d.Activo, cancellationToken)
            ?? throw new CredencialesInvalidasException();

        // Candidatos acotados a la sucursal del dispositivo: el PIN no se busca por su hash (es
        // salado, cambia cada vez), se verifica uno por uno contra este conjunto ya chico.
        var candidatos = await db.Personal.IgnoreQueryFilters()
            .Where(p => p.EmpresaId == dispositivo.EmpresaId && p.EliminadoEn == null && p.PinHash != null)
            .Join(
                db.Areas.IgnoreQueryFilters().Where(a => a.SucursalId == dispositivo.SucursalId),
                p => p.AreaId, a => a.Id, (p, _) => p)
            .ToListAsync(cancellationToken);

        var persona = candidatos.FirstOrDefault(p => hash.Verificar(request.Pin, p.PinHash!))
            ?? throw new CredencialesInvalidasException();

        dispositivo.RegistrarUso(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);

        return new VerificarPinResultado(persona.Id, persona.Nombre);
    }
}
