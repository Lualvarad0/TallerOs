using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Application.Seguridad.DispositivosTablet.Commands.RegistrarDispositivoTablet;

public sealed class RegistrarDispositivoTabletCommandHandler(IAppDbContext db, ITenantContext tenant, IHashService hash)
    : IRequestHandler<RegistrarDispositivoTabletCommand, RegistrarDispositivoTabletResultado>
{
    public async Task<RegistrarDispositivoTabletResultado> Handle(RegistrarDispositivoTabletCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var existeSucursal = await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId && s.EliminadoEn == null, cancellationToken);
        if (!existeSucursal)
        {
            throw new EntidadNoEncontradaException("Sucursal", request.SucursalId);
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        // Determinístico, no Hash(): este token se busca por su valor cuando la tablet lo presenta
        // (ver VerificarPinCommand), no solo se verifica contra una fila ya encontrada.
        var dispositivo = new DispositivoTablet(empresaId, request.SucursalId, request.Nombre, hash.HashDeterministico(token));
        db.DispositivosTablet.Add(dispositivo);
        await db.SaveChangesAsync(cancellationToken);

        return new RegistrarDispositivoTabletResultado(dispositivo.Id, token);
    }
}
