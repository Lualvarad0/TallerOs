namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Un contador por empresa para el número consecutivo de OT. Vive separado de Empresa (que
/// es de Módulo 1) a propósito. El incremento atómico real (UPDATE ... RETURNING, para que dos
/// recepciones simultáneas nunca obtengan el mismo número) es trabajo del caso de uso en el paso de
/// Backend; EmpresaId es también la clave primaria porque esta fila nace junto con la empresa, uno a
/// uno.</summary>
public class ContadorNumeroOt
{
    public Guid EmpresaId { get; private set; }
    public int UltimoNumero { get; private set; }

    private ContadorNumeroOt() { }

    public ContadorNumeroOt(Guid empresaId)
    {
        EmpresaId = empresaId;
        UltimoNumero = 0;
    }
}
