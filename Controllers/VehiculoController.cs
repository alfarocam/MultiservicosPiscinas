using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class VehiculoController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public VehiculoController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculo
                .Include(v => v.Tecnico)
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            // RolId == 2 es el rol de Técnico (mismo criterio que ya usa
            // AgendaController para llenar sus combos de técnicos). Antes se traían
            // todos los usuarios del sistema (admins, clientes, etc.), por eso
            // aparecían en el desplegable personas que no son técnicos.
            var tecnicos = await _context.Usuario
                .Where(u => u.RolId == 2 && u.Activo)
                .OrderBy(u => u.ApellidoPaterno)
                .Select(u => new {
                    u.Id,
                    NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                })
                .ToListAsync();

            ViewBag.Tecnicos = new SelectList(tecnicos, "Id", "NombreCompleto");

            return View(vehiculos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Vehiculo vehiculo)
        {
            // Nota: acá NO se usa ModelState.IsValid a secas. "Vehiculo" es la
            // entidad de Entity Framework tal cual, y tiene propiedades como
            // "Tecnico" (la navegación) y "Estado" que son de tipo no-nulable pero
            // que el formulario nunca envía (Estado se asigna un poco más abajo, y
            // "Tecnico" no se completa nunca acá, solo su Id). Por eso
            // ModelState.IsValid daba "false" SIEMPRE, incluso llenando bien el
            // formulario: ASP.NET Core marca esas propiedades como inválidas por
            // venir en null. La validación real solo debe mirar los campos que el
            // usuario efectivamente llena.
            if (string.IsNullOrWhiteSpace(vehiculo.Placa)
                || string.IsNullOrWhiteSpace(vehiculo.Marca)
                || vehiculo.TecnicoId <= 0)
            {
                TempData["MensajeError"] = "Debe completar Placa, Marca y seleccionar un Técnico válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                bool placaExiste = await _context.Vehiculo
                    .AnyAsync(v => v.Placa.ToLower() == vehiculo.Placa.ToLower());

                if (placaExiste)
                {
                    TempData["MensajeError"] = $"¡Error! La placa '{vehiculo.Placa}' ya está registrada en el sistema.";
                    return RedirectToAction(nameof(Index));
                }

                // registro exitoso
                vehiculo.Estado = "Activo";

                _context.Vehiculo.Add(vehiculo);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"Vehiiculo registrado con exito (ID Interno: #{vehiculo.Id}) y asignado como Activo.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                // Si SQL Server tarda demasiado (por ejemplo, por un bloqueo de otra
                // sesión) o rechaza el guardado, esto evita que la petición quede sin
                // responder: se captura el error y se informa al usuario.
                TempData["MensajeError"] = "No se pudo registrar el vehículo. Ocurrió un error al guardar en la base de datos.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Vehiculo vehiculo)
        {
            try
            {
                var vehiculoExistente = await _context.Vehiculo.FindAsync(vehiculo.Id);

                if (vehiculoExistente == null)
                {
                    TempData["MensajeError"] = "No se encontró el vehiculo para actualizar.";
                    return RedirectToAction(nameof(Index));
                }


                bool placaDuplicada = await _context.Vehiculo
                    .AnyAsync(v => v.Placa.ToLower() == vehiculo.Placa.ToLower() && v.Id != vehiculo.Id);

                if (placaDuplicada)
                {
                    TempData["MensajeError"] = $"¡Error! La placa '{vehiculo.Placa}' pertenece a otro vehículo.";
                    return RedirectToAction(nameof(Index));
                }


                vehiculoExistente.Placa = vehiculo.Placa;
                vehiculoExistente.Marca = vehiculo.Marca;
                vehiculoExistente.Modelo = vehiculo.Modelo;
                vehiculoExistente.TecnicoId = vehiculo.TecnicoId;

                _context.Update(vehiculoExistente);
                await _context.SaveChangesAsync();


                var usuarioActual = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Admin";
                var fechaModificacion = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

                TempData["MensajeExito"] = $"Vehículo #{vehiculo.Id} actualizado correctamente. (Última modificación por: {usuarioActual} el {fechaModificacion})";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "No se pudo actualizar el vehículo. Ocurrió un error al guardar en la base de datos.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
