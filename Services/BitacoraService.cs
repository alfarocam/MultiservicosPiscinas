using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Services
{
    public class BitacoraService(PiscinasYMultiserviciosContext context)
    {
        private readonly PiscinasYMultiserviciosContext _context = context;

        public static string ObtenerNombreModulo(string? tabla) => (tabla ?? string.Empty).Trim() switch
        {
            "inv.PRODUCTO" => "Inventario (productos)",
            "inv.CATEGORIA_PRODUCTO" => "Inventario (categorías)",
            "ops.CITA" => "Agenda (citas)",
            "ops.SERVICIO" => "Servicios técnicos",
            "seg.USUARIO" or "USUARIO" => "Usuarios y roles",
            "seg.ROL" => "Roles y permisos",
            "cli.CLIENTE" => "Clientes",
            "fin.GASTO_OPERATIVO" => "Gastos operativos",
            "fin.CATEGORIA_GASTO_OPERATIVO" => "Gastos operativos (categorías)",
            "act.PISCINA" => "Piscinas",
            "pry.PROYECTO_CONSTRUCCION" => "Proyectos",
            "log.VEHICULO" => "Vehículos de la Empresa",
            "" => "Sistema",
            var t => t
        };

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