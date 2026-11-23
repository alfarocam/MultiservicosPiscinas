using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace MultiserviciosPiscinas.Services
{
    public class Generales(IConfiguration configuration)
    {
        #region Enviar Correo
        public void EnviarCorreo(string destinatario, string asunto, string cuerpo, string? replyTo = null)
        {
            //leemos desde appsettings.json
            var emailAccount = configuration["EmailSettings:Account"];
            if (string.IsNullOrWhiteSpace(emailAccount))
                throw new InvalidOperationException("EmailSettings:Account no está configurado en appsettings.json");

            var emailPassword = configuration["EmailSettings:Password"];
            if (string.IsNullOrWhiteSpace(emailPassword))
                throw new InvalidOperationException("EmailSettings:Password no está configurado en appsettings.json");

            using var mail = new MailMessage();
            mail.From = new MailAddress(emailAccount);
            mail.To.Add(destinatario);
            mail.Subject = asunto;
            mail.Body = cuerpo;
            mail.IsBodyHtml = true;

            // Si se desea agregar una dirección de respuesta diferente al remitente, se puede hacer así:
            if (!string.IsNullOrEmpty(replyTo))
            {
                mail.ReplyToList.Add(new MailAddress(replyTo));
            }

            using var smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential(emailAccount, emailPassword);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
        #endregion


        #region Normalizar Teléfono
        public static string NormalizarTelefono(string? telefono)
        {
            return (telefono ?? string.Empty)
                .Trim()
                .Replace("-", "")
                .Replace(" ", "");
        }

        public static bool EsTelefonoValido(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(telefono.Trim(), @"^\d{4}-?\d{4}$");
        }

        public static string FormatearTelefono(string? telefono)
        {
            var normalizado = NormalizarTelefono(telefono);
            if (normalizado.Length == 8 && normalizado.All(char.IsDigit))
                return $"{normalizado[..4]}-{normalizado[4..8]}";

            var soloDigitos = new string((telefono ?? string.Empty).Where(char.IsDigit).ToArray());
            if (soloDigitos.Length == 8)
                return $"{soloDigitos[..4]}-{soloDigitos[4..8]}";

            return (telefono ?? string.Empty).Trim();
        }
        #endregion

        #region Generar Contraseña
        public string GenerarContrasena()
        {
            int longitud = 8;
            const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder resultado = new(longitud);

            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[1];
            for (int i = 0; i < longitud; i++)
            {
                rng.GetBytes(bytes);
                int indice = bytes[0] % letras.Length;
                resultado.Append(letras[indice]);
            }
            return resultado.ToString();
        }
        #endregion
    }
}
