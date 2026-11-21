using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo;

/// <summary>Los 8 estados estándar del flujo de OT (ver docs/especificacion.md y docs/diseno/README.md
/// § Mapeo de estados de OT → color) y el contador de numeración, para cuando se crea una empresa
/// nueva. La migración Modulo4OrdenesTrabajoYKanban ya siembra estas mismas filas para la empresa
/// demo con HasData; este helper es el que corre en tiempo de ejecución para cada empresa nueva
/// (CrearEmpresaCommandHandler).</summary>
public static class EstadosOtEstandar
{
    /// <summary>No hace nada si la empresa ya tiene estados (IgnoreQueryFilters porque quien llama
    /// esto normalmente no tiene todavía una empresa de sesión real, ver CrearEmpresaCommandHandler):
    /// llamarlo dos veces para la misma empresa no duplica filas.</summary>
    public static async Task SembrarAsync(IAppDbContext db, Guid empresaId, CancellationToken cancellationToken)
    {
        var yaSembrado = await db.EstadosOt.IgnoreQueryFilters().AnyAsync(e => e.EmpresaId == empresaId, cancellationToken);
        if (yaSembrado)
        {
            return;
        }

        db.EstadosOt.AddRange(
            new EstadoOt(empresaId, "Recibido", 1, esFinal: false, colorToken: "sf3"),
            new EstadoOt(empresaId, "Diagnóstico", 2, esFinal: false, colorToken: "blue"),
            new EstadoOt(empresaId, "Esperando aprobación del cliente", 3, esFinal: false, colorToken: "vio"),
            new EstadoOt(empresaId, "Esperando repuestos", 4, esFinal: false, colorToken: "vio"),
            new EstadoOt(empresaId, "En reparación", 5, esFinal: false, colorToken: "blue"),
            new EstadoOt(empresaId, "Control de calidad", 6, esFinal: false, colorToken: "blue"),
            new EstadoOt(empresaId, "Listo para entregar", 7, esFinal: false, colorToken: "grn"),
            new EstadoOt(empresaId, "Entregado", 8, esFinal: true, colorToken: "grn"));

        db.ContadoresNumeroOt.Add(new ContadorNumeroOt(empresaId));
    }
}
