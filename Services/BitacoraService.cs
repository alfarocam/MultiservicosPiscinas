using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Services
{
    public class BitacoraService(PiscinasYMultiserviciosContext context)
    {
        private readonly PiscinasYMultiserviciosContext _context = context;

        //Registra una accion en la bitacora de auditoria
        public async Task RegistrarAsync(
            ClaimsPrincipal userClaims,
            string accion,
            string tablaAfectada,
            int registroId,
            string valorNuevo,
            string? valorAnterior = null)
        {
            var correo = userClaims.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correo))
                return;

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
                return;

            var registro = new BitacoraAuditoria
            {
                UsuarioId = usuario.Id,
                Accion = accion,
                TablaAfectada = tablaAfectada,
                RegistroId = registroId,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                FechaHora = DateTime.Now
            };

            _context.BitacoraAuditoria.Add(registro);
            await _context.SaveChangesAsync();
        }
    }
}