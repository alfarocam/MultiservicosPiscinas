namespace MultiserviciosPiscinas.Services;

/// <summary>
/// Utilidad para cifrar y verificar contraseñas con BCrypt, con soporte de transición para claves legadas en texto plano.
/// </summary>
public static class PasswordHelper
{
    private const int WorkFactor = 11;

    /// <summary>
    /// Genera un hash BCrypt seguro a partir de una contraseña en texto plano.
    /// </summary>
    public static string Hash(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(plainText));

        return BCrypt.Net.BCrypt.HashPassword(plainText, WorkFactor);
    }

    /// <summary>
    /// Determina si el valor almacenado corresponde a un hash BCrypt válido.
    /// </summary>
    public static bool EsHashBCrypt(string? valor)
    {
        return !string.IsNullOrEmpty(valor)
            && valor.Length >= 59
            && (valor.StartsWith("$2a$", StringComparison.Ordinal)
                || valor.StartsWith("$2b$", StringComparison.Ordinal)
                || valor.StartsWith("$2y$", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifica una contraseña en texto plano contra un hash BCrypt o una clave legada en texto plano.
    /// </summary>
    public static bool Verificar(string plainText, string? storedPassword)
    {
        if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(storedPassword))
            return false;

        if (EsHashBCrypt(storedPassword))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(plainText, storedPassword);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        return string.Equals(storedPassword, plainText, StringComparison.Ordinal);
    }
}
