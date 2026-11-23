using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Text.RegularExpressions;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
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
                .Where(u => u.RolId == 2 && u.Activo && u.Cliente == null)
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
            if (!ValidarDatosVehiculo(vehiculo, out var placaNormalizada, out var mensajeError))
            {
                TempData["MensajeError"] = mensajeError;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                bool placaExiste = await _context.Vehiculo
                    .AnyAsync(v => v.Placa.ToLower() == placaNormalizada.ToLower());

                if (placaExiste)
                {
                    TempData["MensajeError"] = $"¡Error! La placa '{placaNormalizada}' ya está registrada en el sistema.";
                    return RedirectToAction(nameof(Index));
                }

                // registro exitoso
                vehiculo.Placa = placaNormalizada;
                vehiculo.Marca = vehiculo.Marca.Trim();
                vehiculo.Modelo = string.IsNullOrWhiteSpace(vehiculo.Modelo) ? null : vehiculo.Modelo.Trim();
                vehiculo.Estado = "Activo";

                _context.Vehiculo.Add(vehiculo);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = $"Vehículo registrado con éxito (ID Interno: #{vehiculo.Id}) y asignado como Activo.";
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
            if (!ValidarDatosVehiculo(vehiculo, out var placaNormalizada, out var mensajeError))
            {
                TempData["MensajeError"] = mensajeError;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var vehiculoExistente = await _context.Vehiculo.FindAsync(vehiculo.Id);

                if (vehiculoExistente == null)
                {
                    TempData["MensajeError"] = "No se encontró el vehículo para actualizar.";
                    return RedirectToAction(nameof(Index));
                }


                bool placaDuplicada = await _context.Vehiculo
                    .AnyAsync(v => v.Placa.ToLower() == placaNormalizada.ToLower() && v.Id != vehiculo.Id);

                if (placaDuplicada)
                {
                    TempData["MensajeError"] = $"¡Error! La placa '{placaNormalizada}' pertenece a otro vehículo.";
                    return RedirectToAction(nameof(Index));
                }


                vehiculoExistente.Placa = placaNormalizada;
                vehiculoExistente.Marca = vehiculo.Marca.Trim();
                vehiculoExistente.Modelo = string.IsNullOrWhiteSpace(vehiculo.Modelo) ? null : vehiculo.Modelo.Trim();
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

        private static bool ValidarDatosVehiculo(Vehiculo vehiculo, out string placaNormalizada, out string? mensajeError)
        {
            placaNormalizada = string.Empty;

            if (string.IsNullOrWhiteSpace(vehiculo.Placa)
                || string.IsNullOrWhiteSpace(vehiculo.Marca)
                || vehiculo.TecnicoId <= 0)
            {
                mensajeError = "Debe completar Placa, Marca y seleccionar un Técnico válido.";
                return false;
            }

            if (!NormalizarYValidarPlaca(vehiculo.Placa, out placaNormalizada))
            {
                mensajeError = "Formato de placa inválido. Use ABC-123 (letras y números) o 123456 (solo números).";
                return false;
            }

            mensajeError = null;
            return true;
        }

        private static bool NormalizarYValidarPlaca(string? placa, out string placaNormalizada)
        {
            placaNormalizada = (placa ?? string.Empty).Trim().ToUpperInvariant();

            if (Regex.IsMatch(placaNormalizada, @"^[A-Z]{3}\d{3}$"))
            {
                placaNormalizada = $"{placaNormalizada[..3]}-{placaNormalizada[3..]}";
            }

            return Regex.IsMatch(placaNormalizada, @"^([A-Z]{3}-\d{3}|\d{6})$");
        }
    }
}
