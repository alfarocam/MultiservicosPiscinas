using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace MultiserviciosPiscinas.Services
{
    public class Generales(IConfiguration configuration)
    {
        public void EnviarCorreo(string destinatario, string asunto, string cuerpo)
        {
            //leemos desde appsettings.json
            var emailAccount = configuration["EmailSettings:Account"];
            var emailPassword = configuration["EmailSettings:Password"];

            using var mail = new MailMessage();
            mail.From = new MailAddress(emailAccount);
            mail.To.Add(destinatario);
            mail.Subject = asunto;
            mail.Body = cuerpo;
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential(emailAccount, emailPassword);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

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
    }
}
