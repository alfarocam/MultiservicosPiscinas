using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Data;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class GastoOperativoController(
        PiscinasYMultiserviciosContext context,
        BitacoraService bitacora) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;
        private readonly BitacoraService _bitacora = bitacora;

        // GET: /GastoOperativo/Index -> Redirige al módulo unificado Gestión de Gastos
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Gastos");
        }

        // GET: /GastoOperativo/Crear -> Redirige al formulario unificado
        public IActionResult Crear()
        {
            return RedirectToAction("Registrar", "Gastos");
        }

        // GET: /GastoOperativo/Detalle/5 -> Redirige al detalle unificado
        public IActionResult Detalle(int id)
        {
            return RedirectToAction("Detalle", "Gastos", new { id });
        }

        // POST: /GastoOperativo/Aprobar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto != null)
            {
                string estadoAnterior = gasto.Estado;

                if (string.IsNullOrEmpty(gasto.ComprobanteRuta))
                {
                    gasto.Estado = "Rechazado";
                    gasto.MotivoRechazo = "Rechazado automáticamente: el gasto no tiene comprobante adjunto.";
                    await _context.SaveChangesAsync();
                    await _bitacora.RegistrarAsync(
                        userClaims: User,
                        accion: "UPDATE",
                        tablaAfectada: "fin.GASTO_OPERATIVO",
                        registroId: gasto.Id,
                        valorAnterior: $"Estado: {estadoAnterior}",
                        valorNuevo: $"Estado: {gasto.Estado} | Motivo: {gasto.MotivoRechazo}"
                    );
                    TempData["MensajeError"] = "El gasto no tiene comprobante, por lo que fue RECHAZADO automáticamente.";
                    return RedirectToAction("Index", "Gastos");
                }

                gasto.Estado = "Aprobado";
                gasto.MotivoRechazo = null;
                await _context.SaveChangesAsync();
                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "UPDATE",
                    tablaAfectada: "fin.GASTO_OPERATIVO",
                    registroId: gasto.Id,
                    valorAnterior: $"Estado: {estadoAnterior}",
                    valorNuevo: $"Estado: {gasto.Estado} | Monto: ₡{gasto.Monto:N2}"
                );
                TempData["MensajeExito"] = "El gasto fue APROBADO exitosamente.";
            }
            return RedirectToAction("Index", "Gastos");
        }

        // POST: /GastoOperativo/Rechazar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto != null)
            {
                if (string.IsNullOrWhiteSpace(motivoRechazo))
                {
                    TempData["MensajeError"] = "Debe escribir una justificación para rechazar el gasto.";
                    return RedirectToAction("Index", "Gastos");
                }

                string estadoAnterior = gasto.Estado;
                string motivoLimpio = motivoRechazo.Trim();

                gasto.Estado = "Rechazado";
                gasto.MotivoRechazo = motivoLimpio;
                await _context.SaveChangesAsync();
                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "UPDATE",
                    tablaAfectada: "fin.GASTO_OPERATIVO",
                    registroId: gasto.Id,
                    valorAnterior: $"Estado: {estadoAnterior}",
                    valorNuevo: $"Estado: {gasto.Estado} | Motivo: {motivoLimpio}"
                );
                TempData["MensajeExito"] = "El gasto fue RECHAZADO.";
            }
            return RedirectToAction("Index", "Gastos");
        }

        // POST: /GastoOperativo/CrearCategoria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCategoria(string nombreCategoria, string? returnUrl = null)
        {
            var (exito, mensaje, nuevaCategoria) =
                await DivisionTerritorialSeeder.RegistrarCategoriaGastoAsync(_context, nombreCategoria);

            if (!exito || nuevaCategoria == null)
            {
                TempData["MensajeError"] = mensaje;
                return RedirigirRetorno(returnUrl);
            }

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "INSERT",
                tablaAfectada: "fin.CATEGORIA_GASTO_OPERATIVO",
                registroId: nuevaCategoria.Id,
                valorNuevo: $"Categoría: {nuevaCategoria.NombreCategoria}"
            );

            TempData["MensajeExito"] = mensaje;
            return RedirigirRetorno(returnUrl);
        }

        private IActionResult RedirigirRetorno(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Gastos");
        }
    }
}
