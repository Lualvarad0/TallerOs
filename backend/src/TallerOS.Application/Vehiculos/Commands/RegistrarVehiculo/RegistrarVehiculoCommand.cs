using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;

/// <summary>Único punto de entrada para vincular un cliente a un vehículo (ver ClienteVehiculo y
/// AppDbContext): a propósito no recibe VehiculoId. Vehiculo es un catálogo global (docs/convenciones.md §
/// Seguridad) sin dato propio contra el cual una política RLS pueda verificar que el vehículo es
/// "el mismo" que el llamante dice tener enfrente; esa verificación solo puede hacerla este comando,
/// resolviendo el vehículo él mismo a partir de lo que de verdad identifica al vehículo físico (VIN
/// o placa), nunca a partir de un id que el cliente de la Api podría inventar.
///
/// RLS (ver Modulo3RlsPoliticas) aísla las LECTURAS: una empresa sin ClienteVehiculo hacia un
/// vehículo no puede verlo. No aísla este INSERT — ninguna política puede, por la razón de arriba
/// (WITH CHECK (true) en vehiculo, sin política alguna en cliente_vehiculo salvo la de empresa_id
/// propio) — así que la única barrera real contra vincular un vehículo ajeno es este contrato: quien
/// llama nunca puede nombrar un vehículo por id, solo por lo que el cliente dice que tiene delante.
///
/// Decisión de negocio, no una verdad técnica: conocer el VIN o la placa activa de un vehículo SÍ
/// basta para vincularlo. No es una prueba imposible de falsificar — alguien podría conocer el VIN
/// de un vehículo ajeno sin tenerlo delante, por ejemplo por un dato filtrado o copiado de un
/// documento — pero es el mismo nivel de prueba que ya exige la operación real del taller (ver
/// docs/especificacion.md § Clientes con varios vehículos: "en recepción, buscar por placa, cédula o
/// nombre"), y este módulo no tiene, hoy, un dato más fuerte que pedir sin inventar uno que la
/// especificación no contempla. Vincular solo concede a esta empresa ver los datos básicos del
/// vehículo (VIN, marca, modelo, año, motor, placa) para operar su propia OT; nunca edición del
/// catálogo global, ni acceso a vehiculo_propietario (Mi Garage) ni a cuenta_cliente salvo que el
/// cliente de esta empresa ya esté vinculado directamente a esa cuenta.
///
/// La búsqueda de un vehículo ya existente no se limita a lo que esta empresa ya conoce: por cada
/// identificador dado (VIN, placa) que no coincida con algo que esta empresa ya ve, el handler busca
/// ese identificador una vez más entre TODAS las empresas (ver RegistrarVehiculoCommandHandler e
/// IAppDbContext.ElevarPrivilegiosSinEmpresaAsync) antes de darlo por nuevo — no solo cuando NINGUNO
/// de los dos aparece localmente: si el VIN ya coincidió localmente pero la placa dada no, la placa
/// igual se busca globalmente, porque podría pertenecer a un vehículo físico distinto de otra
/// empresa (y el chequeo de conflicto de abajo necesita ese dato para detectarlo). Esa búsqueda
/// global solo lee los datos propios del vehículo (nunca quién lo atendió ni sus otros
/// clientes/cuentas), así que reutilizarlo no filtra nada de la otra empresa.
///
/// Vin y Placa son ambos opcionales (un VIN ilegible es real, ver Vehiculo), pero se exige al menos
/// uno: sin ninguno de los dos no hay forma de identificar el vehículo ni de evitar duplicados.</summary>
public sealed record RegistrarVehiculoCommand(
    Guid ClienteId,
    string? Vin,
    string? Placa,
    Guid MarcaId,
    Guid ModeloId,
    short Anio,
    string? Motor,
    RolClienteVehiculo Rol) : IRequest<RegistrarVehiculoResultado>;

/// <summary>EsVehiculoNuevo distingue, para quien llama, si se creó un vehículo o se reutilizó uno
/// que ya existía con ese VIN o esa placa activa — propio (esta empresa ya lo conocía) o de otra
/// empresa, encontrado por la búsqueda global (ver RegistrarVehiculoCommand).
///
/// AdvertenciaVin no es nulo cuando se dio un VIN y su dígito verificador no es válido (ver
/// VinValidator y RegistrarVehiculoCommandValidator): el registro de todas formas se completó, esto
/// solo informa al llamante que el VIN guardado podría estar mal transcrito. Nulo siempre que no se
/// dio VIN o el VIN dado sí es válido.</summary>
public sealed record RegistrarVehiculoResultado(Guid VehiculoId, Guid ClienteVehiculoId, bool EsVehiculoNuevo, string? AdvertenciaVin);
