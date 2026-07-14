//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using MultiserviciosPiscinas.Models;
//using System.Security.Claims;

//namespace MultiserviciosPiscinas.Controllers
//{
//    [Authorize]
//    public class NotificacionesController(PiscinasYMultiserviciosContext context) : Controller
//    {
//        private readonly PiscinasYMultiserviciosContext _context = context;

//        [HttpGet]
//        public async Task<IActionResult> ObtenerNoLeidas()
//        {
//            var usuarioId = await ObtenerUsuarioIdAsync();
//            if (usuarioId == null)
//            {
//                return Json(new { count = 0, notificaciones = new List<object>() });
//            }

//            var notificaciones = await _context.Notificacion
//                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
//                .OrderByDescending(n => n.FechaCreacion)
//                .Select(n => new
//                {
//                    id = n.Id,
//                    mensaje = n.Mensaje,
//                    fecha = n.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
//                })
//                .ToListAsync();

//            return Json(new { count = notificaciones.Count, notificaciones });
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> MarcarLeida(int id)
//        {
//            var usuarioId = await ObtenerUsuarioIdAsync();

//            var notificacion = await _context.Notificacion
//                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == usuarioId);

//            if (notificacion != null)
//            {
//                notificacion.Leida = true;
//                await _context.SaveChangesAsync();
//            }

//            return Ok();
//        }

//        private async Task<int?> ObtenerUsuarioIdAsync()
//        {
//            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
//            if (string.IsNullOrEmpty(correo))
//            {
//                return null;
//            }

//            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);
//            return usuario?.Id;
//        }
//    }
//}