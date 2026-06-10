using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class ContactoController(PiscinasYMultiserviciosContext _contexto, Generales _generales, IWebHostEnvironment _entornoWeb, IConfiguration _configuracion) : Controller
    {

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enviar(ContactoViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuarioEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var nombreUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            string rutaHtml = Path.Combine(
                _entornoWeb.ContentRootPath,
                "Template",
                "ContactoYSoporte.html");

            if (!System.IO.File.Exists(rutaHtml))
            {
                TempData["Mensaje"] = "Error interno: No se encontró la plantilla de correo.";
                TempData["TipoMensaje"] = "danger";
                return View(model);
            }

            string contenidoHtml = await System.IO.File.ReadAllTextAsync(rutaHtml);

            string htmlFinal = contenidoHtml
                .Replace("{{NOMBRE_USUARIO}}", nombreUsuario)
                .Replace("{{MENSAJE}}", model.Mensaje);

            var emailEmpresa = _configuracion["EmailSettings:Account"];
            _generales.EnviarCorreo(
                emailEmpresa,
                $"[Soporte Web] {model.Asunto}",
                htmlFinal,
                usuarioEmail);

            if (string.IsNullOrEmpty(usuarioEmail))
            {
                TempData["Mensaje"] = "Tu mensaje ha sido enviado, pero no se pudo establecer una dirección de respuesta. Por favor, asegúrate de que tu cuenta de correo esté verificada.";
                TempData["TipoMensaje"] = "warning"; // Alerta amarilla
            }
            else
            {
                TempData["Mensaje"] = "Tu mensaje ha sido enviado correctamente. Nos pondremos en contacto contigo lo antes posible.";
                TempData["TipoMensaje"] = "success"; // Alerta verde
            }

            return RedirectToAction("Index", "Contacto");
        }

        public IActionResult Informacion()
        {
            var contacto = new ContactoEmpresa
            {
                Telefono = _configuracion["DatosContacto:Telefono"] ?? "No disponible",
                Correo = _configuracion["DatosContacto:Correo"] ?? "No disponible",
                Direccion = _configuracion["DatosContacto:Direccion"] ?? "No disponible",
                HorarioAtencion = _configuracion["DatosContacto:HorarioAtencion"] ?? "No disponible"
            };

            return View(contacto);
        }
    }
}