namespace TallerOS.Application.Comun;

/// <summary>Un solo mecanismo de hash para los tres secretos del módulo: password_hash de Usuario,
/// pin_hash de Personal y token_hash de DispositivoTablet. No hay razón para tres implementaciones.</summary>
public interface IHashService
{
    /// <summary>Con salt aleatorio: dos llamadas con el mismo valor dan hashes distintos. Sirve para
    /// password_hash y pin_hash, que siempre se verifican contra una fila ya encontrada por otro
    /// medio (email, o la lista de personal de una sucursal), nunca se buscan por su hash.</summary>
    string Hash(string valor);
    bool Verificar(string valor, string hash);

    /// <summary>Sin salt (SHA-256): mismo valor, mismo hash siempre. Es lo único que permite
    /// `WHERE token_hash = ...` para encontrar la fila por su secreto (ej. DispositivoTablet).
    /// No usar para password_hash ni pin_hash.</summary>
    string HashDeterministico(string valor);
}
