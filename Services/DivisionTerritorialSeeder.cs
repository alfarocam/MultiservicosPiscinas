using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Data;

public class DivisionTerritorialSeeder(
    HttpClient httpClient,
    PiscinasYMultiserviciosContext dbContext,
    ILogger<DivisionTerritorialSeeder> logger)
{
    private const string BaseUrl = "https://api-geo-cr.vercel.app";

    // La API pagina de a 7 elementos por defecto. Sin pedir un límite alto solo
    // se guardaba la primera página de cada listado, dejando provincias con 7
    // cantones y cantones con 7 distritos en vez de todos los que tienen.
    private const int LimitePorPagina = 200;

    public async Task SeedAsync()
    {
        logger.LogInformation("=== [SEEDER] Verificando división territorial ===");

        try
        {
            await AsegurarColumnasOperacionesAsync();
            await MigrarContrasenasTextoPlanoABcryptSafeAsync();
            await AsegurarCategoriasGastoOperativoAsync();
            await AsegurarCategoriasProductoAsync();
            await AsegurarIntegridadRolesYDescripcionesSafeAsync();
            await AsegurarIntegridadClientesYTelefonosSafeAsync();
            await CerrarCitasVencidasSafeAsync();

            var provincias = await ObtenerCatalogoAsync(
                $"{BaseUrl}/provincias?limit={LimitePorPagina}", "idProvincia");

            if (provincias.Count == 0)
            {
                logger.LogWarning("[SEEDER] La API no devolvió provincias. Se conserva lo que ya está guardado.");
                return;
            }

            var provinciasExistentes = await dbContext.Provincia
                .Select(p => p.Id)
                .ToListAsync();

            foreach (var (provinciaId, provinciaNombre) in provincias)
            {
                try
                {
                    await SincronizarProvinciaAsync(
                        provinciaId,
                        provinciaNombre,
                        provinciasExistentes.Contains(provinciaId));
                }
                catch (Exception exProvincia)
                {
                    // No se relanza: que falle una provincia (p. ej. un timeout
                    // puntual de la API) no debe impedir procesar las demás. En
                    // el próximo arranque se vuelve a intentar lo que faltó.
                    logger.LogError("[SEEDER] No se pudo procesar la provincia ID {Id}: {Error}",
                        provinciaId, exProvincia.Message);
                    dbContext.ChangeTracker.Clear();
                }
            }

            logger.LogInformation("=== [SEEDER] División territorial al día ===");
        }
        catch (Exception ex)
        {
            logger.LogCritical("[SEEDER] Error crítico durante la ejecución: {Error}", ex.Message);
        }
    }

    // Agrega solo lo que falta comparando la base contra la API, de modo que una
    // provincia sembrada a medias se pueda completar en un arranque posterior.
    private async Task SincronizarProvinciaAsync(int provinciaId, string provinciaNombre, bool provinciaExiste)
    {
        if (!provinciaExiste)
        {
            dbContext.Provincia.Add(new Provincia { Id = provinciaId, Nombre = provinciaNombre });
        }

        var cantonesGuardados = await dbContext.Canton
            .Where(c => c.ProvinciaId == provinciaId)
            .Select(c => c.Id)
            .ToListAsync();

        // Cantones guardados a los que nunca se les cargaron distritos.
        var cantonesSinDistritos = await dbContext.Canton
            .Where(c => c.ProvinciaId == provinciaId && !c.Distrito.Any())
            .Select(c => c.Id)
            .ToListAsync();

        var cantones = await ObtenerCatalogoAsync(
            $"{BaseUrl}/provincias/{provinciaId}/cantones?limit={LimitePorPagina}", "idCanton");

        int cantonesNuevos = 0;
        int distritosNuevos = 0;

        foreach (var (cantonId, cantonNombre) in cantones)
        {
            bool cantonExiste = cantonesGuardados.Contains(cantonId);

            if (!cantonExiste)
            {
                dbContext.Canton.Add(new Canton
                {
                    Id = cantonId,
                    ProvinciaId = provinciaId,
                    Nombre = cantonNombre
                });
                cantonesNuevos++;
            }
            else if (!cantonesSinDistritos.Contains(cantonId))
            {
                // Ya está guardado y con distritos: no hace falta pedirlos.
                continue;
            }

            var distritos = await ObtenerCatalogoAsync(
                $"{BaseUrl}/cantones/{cantonId}/distritos?limit={LimitePorPagina}", "idDistrito");

            foreach (var (distritoId, distritoNombre) in distritos)
            {
                dbContext.Distrito.Add(new Distrito
                {
                    Id = distritoId,
                    CantonId = cantonId,
                    Nombre = distritoNombre
                });
                distritosNuevos++;
            }
        }

        if (!provinciaExiste || cantonesNuevos > 0 || distritosNuevos > 0)
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("[SEEDER] Provincia ID {Id}: {Cantones} cantones y {Distritos} distritos agregados.",
                provinciaId, cantonesNuevos, distritosNuevos);
        }
    }

    // Los tres endpoints comparten la forma de respuesta: un arreglo "data" con
    // el id en un campo propio y el nombre siempre en "descripcion".
    private async Task<List<(int Id, string Nombre)>> ObtenerCatalogoAsync(string url, string campoId)
    {
        var lista = new List<(int, string)>();

        var raw = await httpClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(raw);

        if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            logger.LogError("[SEEDER] Respuesta inesperada de {Url}: no trae un arreglo 'data'.", url);
            return lista;
        }

        foreach (var item in data.EnumerateArray())
        {
            lista.Add((
                item.GetProperty(campoId).GetInt32(),
                item.GetProperty("descripcion").GetString() ?? ""));
        }

        return lista;
    }

    private async Task AsegurarColumnasOperacionesAsync()
    {
        try
        {
            const string sql = @"
IF COL_LENGTH('ops.TAREA_SERVICIO', 'fecha_hora_registro') IS NULL
    ALTER TABLE ops.TAREA_SERVICIO ADD fecha_hora_registro datetime NULL;

IF COL_LENGTH('ops.INSPECCION', 'temperatura_c') IS NULL
    ALTER TABLE ops.INSPECCION ADD temperatura_c float NULL;

IF COL_LENGTH('seg.USUARIO', 'telefono') IS NULL
    ALTER TABLE seg.USUARIO ADD telefono varchar(20) NULL;";

            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            logger.LogWarning("[SEEDER] No se pudieron verificar las columnas de ops: {Error}", ex.Message);
        }
    }

    private async Task AsegurarCategoriasGastoOperativoAsync()
    {
        try
        {
            bool estabaVacia = !await dbContext.CategoriaGastoOperativo.AnyAsync();
            await AsegurarCategoriasGastoOperativoAsync(dbContext);
            if (estabaVacia)
            {
                logger.LogInformation("[SEEDER] Categorías base de gastos operativos sembradas.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("[SEEDER] No se pudieron sembrar las categorías de gastos operativos: {Error}", ex.Message);
        }
    }

    public static async Task AsegurarCategoriasGastoOperativoAsync(PiscinasYMultiserviciosContext context)
    {
        if (await context.CategoriaGastoOperativo.AnyAsync())
            return;

        var categoriasBase = new[]
        {
            "Combustible",
            "Alimentación / Viáticos",
            "Materiales y Químicos",
            "Repuestos y Herramientas",
            "Peajes y Parqueo",
            "Mantenimiento de Vehículo",
            "Servicios Públicos / Operativos",
            "Otros"
        };

        foreach (var nombre in categoriasBase)
        {
            context.CategoriaGastoOperativo.Add(new CategoriaGastoOperativo
            {
                NombreCategoria = nombre
            });
        }

        await context.SaveChangesAsync();
    }

    public static async Task<(bool Exito, string Mensaje, CategoriaGastoOperativo? Categoria)> RegistrarCategoriaGastoAsync(
        PiscinasYMultiserviciosContext context,
        string? nombreCategoria)
    {
        if (string.IsNullOrWhiteSpace(nombreCategoria))
            return (false, "El nombre de la categoría es obligatorio.", null);

        string nombreLimpio = nombreCategoria.Trim();
        if (nombreLimpio.Length > 100)
            return (false, "El nombre de la categoría no puede superar los 100 caracteres.", null);

        await AsegurarCategoriasGastoOperativoAsync(context);

        var existentes = await context.CategoriaGastoOperativo.ToListAsync();
        if (existentes.Any(c => string.Equals(c.NombreCategoria.Trim(), nombreLimpio, StringComparison.OrdinalIgnoreCase)))
            return (false, $"La categoría '{nombreLimpio}' ya existe.", null);

        var nuevaCategoria = new CategoriaGastoOperativo
        {
            NombreCategoria = nombreLimpio
        };

        context.CategoriaGastoOperativo.Add(nuevaCategoria);
        await context.SaveChangesAsync();

        return (true, $"Categoría '{nombreLimpio}' creada correctamente.", nuevaCategoria);
    }

    private async Task AsegurarIntegridadRolesYDescripcionesSafeAsync()
    {
        try
        {
            await AsegurarIntegridadRolesYDescripcionesAsync(dbContext);
        }
        catch (Exception ex)
        {
            logger.LogWarning("[SEEDER] No se pudo verificar integridad de roles/usuario: {Error}", ex.Message);
        }
    }

    private async Task AsegurarIntegridadClientesYTelefonosSafeAsync()
    {
        try
        {
            await AsegurarIntegridadClientesYTelefonosAsync(dbContext);
        }
        catch (Exception ex)
        {
            logger.LogWarning("[SEEDER] No se pudo verificar integridad de clientes/teléfonos: {Error}", ex.Message);
        }
    }

    private async Task MigrarContrasenasTextoPlanoABcryptSafeAsync()
    {
        try
        {
            int migradas = await MigrarContrasenasTextoPlanoABcryptAsync(dbContext);
            if (migradas > 0)
            {
                logger.LogInformation("[SEEDER] {Cantidad} contraseñas en texto plano migradas a BCrypt.", migradas);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("[SEEDER] No se pudieron migrar contraseñas legadas a BCrypt: {Error}", ex.Message);
            dbContext.ChangeTracker.Clear();
        }
    }

    public static async Task<int> MigrarContrasenasTextoPlanoABcryptAsync(PiscinasYMultiserviciosContext context)
    {
        var usuarios = await context.Usuario.ToListAsync();
        int migradas = 0;

        foreach (var usuario in usuarios)
        {
            if (!string.IsNullOrEmpty(usuario.Contrasena) && !PasswordHelper.EsHashBCrypt(usuario.Contrasena))
            {
                usuario.Contrasena = PasswordHelper.Hash(usuario.Contrasena);
                migradas++;
            }
        }

        if (migradas > 0)
        {
            await context.SaveChangesAsync();
        }

        return migradas;
    }

    private async Task CerrarCitasVencidasSafeAsync()
    {
        try
        {
            await CerrarCitasVencidasAsync(dbContext);
        }
        catch (Exception ex)
        {
            logger.LogWarning("[SEEDER] No se pudieron cerrar citas vencidas: {Error}", ex.Message);
        }
    }

    public static async Task CerrarCitasVencidasAsync(PiscinasYMultiserviciosContext context)
    {
        var ahora = DateTime.Now;

        var citasVencidas = await context.Cita
            .Include(c => c.Servicio)
            .Where(c => c.FechaHora < ahora
                     && c.Estado != "Completada"
                     && c.Estado != "Cancelada")
            .ToListAsync();

        if (citasVencidas.Count == 0)
            return;

        foreach (var cita in citasVencidas)
        {
            cita.Estado = "Completada";

            if (cita.Servicio != null
                && cita.Servicio.Estado != "Cerrado"
                && cita.Servicio.Estado != "Cancelado")
            {
                cita.Servicio.Estado = "Cerrado";
                cita.Servicio.FechaCierre ??= DateOnly.FromDateTime(cita.FechaHora);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task AsegurarCategoriasProductoAsync()
    {
        try
        {
            bool estabaVacia = !await dbContext.CategoriaProducto.AnyAsync();
            await AsegurarCategoriasProductoAsync(dbContext);
            if (estabaVacia)
            {
                logger.LogInformation("[SEEDER] Categorías base de inventario sembradas.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("[SEEDER] No se pudieron sembrar las categorías de inventario: {Error}", ex.Message);
        }
    }

    public static async Task AsegurarCategoriasProductoAsync(PiscinasYMultiserviciosContext context)
    {
        if (await context.CategoriaProducto.AnyAsync())
            return;

        var categoriasBase = new[]
        {
            "Químicos",
            "Equipos y Bombas",
            "Accesorios de Limpieza",
            "Repuestos y Tubos"
        };

        foreach (var nombre in categoriasBase)
        {
            context.CategoriaProducto.Add(new CategoriaProducto
            {
                NombreCategoria = nombre
            });
        }

        await context.SaveChangesAsync();
    }

    private static (bool Valido, string MensajeError, string NombreLimpio) ValidarNombreCategoriaProducto(string? nombreCategoria)
    {
        if (string.IsNullOrWhiteSpace(nombreCategoria))
            return (false, "El nombre de la categoría es obligatorio.", string.Empty);

        string nombreLimpio = nombreCategoria.Trim();
        if (nombreLimpio.Length > 100)
            return (false, "El nombre de la categoría no puede superar los 100 caracteres.", string.Empty);

        return (true, string.Empty, nombreLimpio);
    }

    public static async Task<(bool Exito, string Mensaje, CategoriaProducto? Categoria)> RegistrarCategoriaProductoAsync(
        PiscinasYMultiserviciosContext context,
        string? nombreCategoria)
    {
        var (valido, mensajeError, nombreLimpio) = ValidarNombreCategoriaProducto(nombreCategoria);
        if (!valido)
            return (false, mensajeError, null);

        await AsegurarCategoriasProductoAsync(context);

        var existentes = await context.CategoriaProducto.ToListAsync();
        if (existentes.Any(c => string.Equals(c.NombreCategoria.Trim(), nombreLimpio, StringComparison.OrdinalIgnoreCase)))
            return (false, $"La categoría '{nombreLimpio}' ya existe.", null);

        var nuevaCategoria = new CategoriaProducto
        {
            NombreCategoria = nombreLimpio
        };

        context.CategoriaProducto.Add(nuevaCategoria);
        await context.SaveChangesAsync();

        return (true, $"Categoría '{nombreLimpio}' creada correctamente.", nuevaCategoria);
    }

    public static async Task<(bool Exito, string Mensaje, string? NombreAnterior, CategoriaProducto? Categoria)> EditarCategoriaProductoAsync(
        PiscinasYMultiserviciosContext context,
        int id,
        string? nombreCategoria)
    {
        var (valido, mensajeError, nombreLimpio) = ValidarNombreCategoriaProducto(nombreCategoria);
        if (!valido)
            return (false, mensajeError, null, null);

        var categoria = await context.CategoriaProducto.FindAsync(id);
        if (categoria == null)
            return (false, "La categoría seleccionada no existe.", null, null);

        var existentes = await context.CategoriaProducto.ToListAsync();
        if (existentes.Any(c => c.Id != id && string.Equals(c.NombreCategoria.Trim(), nombreLimpio, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe otra categoría con el nombre '{nombreLimpio}'.", null, null);

        string nombreAnterior = categoria.NombreCategoria;
        categoria.NombreCategoria = nombreLimpio;
        await context.SaveChangesAsync();

        return (true, $"Categoría actualizada a '{nombreLimpio}' correctamente.", nombreAnterior, categoria);
    }

    public static async Task AsegurarIntegridadRolesYDescripcionesAsync(PiscinasYMultiserviciosContext context)
    {
        const string sql = @"
IF COL_LENGTH('seg.USUARIO', 'telefono') IS NULL
    ALTER TABLE seg.USUARIO ADD telefono varchar(20) NULL;

UPDATE u
SET u.rol_id = 3
FROM seg.USUARIO u
INNER JOIN cli.CLIENTE c ON c.usuario_id = u.id
WHERE u.rol_id = 2 AND u.correo <> 'admin@piscinas.com';

INSERT INTO cli.CLIENTE (usuario_id, notas)
SELECT u.id, ''
FROM seg.USUARIO u
LEFT JOIN cli.CLIENTE c ON c.usuario_id = u.id
WHERE u.rol_id = 3 AND u.activo = 1 AND c.id IS NULL;";

        await context.Database.ExecuteSqlRawAsync(sql);

        var descripcionesPorDefecto = new Dictionary<int, string>
        {
            [1] = "Control total del sistema: gestión de usuarios, roles, clientes, piscinas, proyectos, agenda general, vehículos, inventario, finanzas, bitácora de auditoría y reportes.",
            [2] = "Operación técnica: consulta y ejecución de servicios asignados, mi agenda, rutas del día, registro de parámetros del agua, consulta de inventario, mis gastos y anuncios.",
            [3] = "Portal de cliente: acceso a tienda en línea y carrito, consulta de mis piscinas e historial de servicios, cotizaciones, facturación, encuestas de satisfacción y soporte."
        };

        var roles = await context.Rol.ToListAsync();
        bool huboCambios = false;

        foreach (var rol in roles)
        {
            if (descripcionesPorDefecto.TryGetValue(rol.Id, out var descripcionPrecisa) &&
                EsDescripcionGenerica(rol.Descripcion))
            {
                rol.Descripcion = descripcionPrecisa;
                huboCambios = true;
            }
        }

        if (huboCambios)
        {
            await context.SaveChangesAsync();
        }

        await AsegurarIntegridadClientesYTelefonosAsync(context);
    }

    public static async Task AsegurarIntegridadClientesYTelefonosAsync(PiscinasYMultiserviciosContext context)
    {
        // 1. Limpiar textos de prueba heredados en notas
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE cli.CLIENTE SET notas = '' WHERE notas LIKE 'Cliente semilla%';");

        // 2. Deduplicar clientes repetidos que compartan tanto el mismo nombre como el mismo teléfono
        var clientes = await context.Cliente
            .Include(c => c.Usuario)
            .Include(c => c.TelefonosCliente)
            .Include(c => c.DireccionCliente)
            .OrderBy(c => c.Id)
            .ToListAsync();

        var gruposDuplicados = clientes
            .Where(c => c.Usuario != null)
            .Select(c => new
            {
                Cliente = c,
                NombreKey = string.Join(
                    " ",
                    $"{c.Usuario.Nombre} {c.Usuario.ApellidoPaterno}"
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .ToUpperInvariant(),
                TelefonoKey = Generales.NormalizarTelefono(
                    c.TelefonosCliente
                        .OrderByDescending(t => t.EsPrincipal)
                        .ThenBy(t => t.Id)
                        .FirstOrDefault()?.NumeroTelefono)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.NombreKey) && !string.IsNullOrEmpty(x.TelefonoKey))
            .GroupBy(x => new { x.NombreKey, x.TelefonoKey })
            .Where(g => g.Count() > 1)
            .Select(g => g.Select(x => x.Cliente).ToList())
            .ToList();

        bool huboFusiones = false;

        foreach (var grupo in gruposDuplicados)
        {
            var ordenados = grupo
                .OrderByDescending(c => c.TelefonosCliente.Any(t => Generales.EsTelefonoValido(t.NumeroTelefono)))
                .ThenByDescending(c => c.DireccionCliente.Any())
                .ThenBy(c => c.Id)
                .ToList();

            var principal = ordenados[0];

            foreach (var dup in ordenados.Skip(1))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE act.PISCINA SET cliente_id = @p0 WHERE cliente_id = @p1;",
                    principal.Id, dup.Id);

                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE pry.PROYECTO_CONSTRUCCION SET cliente_id = @p0 WHERE cliente_id = @p1;",
                    principal.Id, dup.Id);

                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE ven.COTIZACION SET cliente_id = @p0 WHERE cliente_id = @p1;",
                    principal.Id, dup.Id);

                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE ven.FACTURA SET cliente_id = @p0 WHERE cliente_id = @p1;",
                    principal.Id, dup.Id);

                await context.Database.ExecuteSqlRawAsync(
                    "DELETE ic FROM ven.ITEM_CARRITO ic INNER JOIN ven.CARRITO car ON car.id = ic.carrito_id WHERE car.cliente_id = @p0; DELETE FROM ven.CARRITO WHERE cliente_id = @p0;",
                    dup.Id);

                bool principalTieneDirPrincipal = await context.DireccionCliente
                    .AnyAsync(d => d.ClienteId == principal.Id && d.EsPrincipal == 1);

                if (principalTieneDirPrincipal)
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE cli.DIRECCION_CLIENTE SET cliente_id = @p0, es_principal = 0 WHERE cliente_id = @p1;",
                        principal.Id, dup.Id);
                }
                else
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE cli.DIRECCION_CLIENTE SET cliente_id = @p0 WHERE cliente_id = @p1;",
                        principal.Id, dup.Id);
                }

                bool principalTieneTelefono = await context.TelefonosCliente
                    .AnyAsync(t => t.ClienteId == principal.Id);

                if (!principalTieneTelefono)
                {
                    var telefonoDup = dup.TelefonosCliente
                        .OrderByDescending(t => Generales.EsTelefonoValido(t.NumeroTelefono))
                        .ThenByDescending(t => t.EsPrincipal)
                        .ThenBy(t => t.Id)
                        .FirstOrDefault();

                    if (telefonoDup != null)
                    {
                        await context.Database.ExecuteSqlRawAsync(
                            "UPDATE cli.TELEFONOS_CLIENTE SET cliente_id = @p0, es_principal = 1 WHERE id = @p1;",
                            principal.Id, telefonoDup.Id);
                    }
                }

                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM cli.TELEFONOS_CLIENTE WHERE cliente_id = @p0;",
                    dup.Id);

                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM cli.CLIENTE WHERE id = @p0;",
                    dup.Id);

                if (dup.UsuarioId != principal.UsuarioId &&
                    dup.Usuario.RolId != 1 &&
                    !string.Equals(dup.Usuario.Correo, "admin@piscinas.com", StringComparison.OrdinalIgnoreCase))
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE ops.CITA SET tecnico_id = @p0 WHERE tecnico_id = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE fin.GASTO_OPERATIVO SET usuario_id = @p0 WHERE usuario_id = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE aud.BITACORA_AUDITORIA SET usuario_id = @p0 WHERE usuario_id = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE ops.NOTIFICACION SET usuario_id = @p0 WHERE usuario_id = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE crm.ANUNCIO SET autor_id = @p0 WHERE autor_id = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE log.RUTA_OPTIMIZADA SET tecnico_id = @p0 WHERE tecnico_id = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE log.VEHICULO SET tecnico_id = @p0 WHERE tecnico_id = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE ven.FACTURA SET creado_por = @p0 WHERE creado_por = @p1;",
                        principal.UsuarioId, dup.UsuarioId);

                    await context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM seg.USUARIO WHERE id = @p0;",
                        dup.UsuarioId);
                }

                huboFusiones = true;
            }
        }

        if (huboFusiones)
        {
            context.ChangeTracker.Clear();
        }

        // 3. Normalizar todos los teléfonos al formato 8888-8888 y asegurar unicidad entre clientes
        var clientesActualizados = await context.Cliente
            .Include(c => c.TelefonosCliente)
            .OrderBy(c => c.Id)
            .ToListAsync();

        var telefonosUsados = new HashSet<string>(StringComparer.Ordinal);
        bool huboCambiosTelefonos = false;

        foreach (var cliente in clientesActualizados)
        {
            var telefonos = cliente.TelefonosCliente
                .OrderByDescending(t => t.EsPrincipal)
                .ThenBy(t => t.Id)
                .ToList();

            var principalTel = telefonos.FirstOrDefault();

            if (telefonos.Count > 1)
            {
                context.TelefonosCliente.RemoveRange(telefonos.Skip(1));
                huboCambiosTelefonos = true;
            }

            string digitos = string.Empty;
            if (principalTel != null && Generales.EsTelefonoValido(principalTel.NumeroTelefono))
            {
                var candidatoActual = Generales.NormalizarTelefono(principalTel.NumeroTelefono);
                if (!telefonosUsados.Contains(candidatoActual))
                {
                    digitos = candidatoActual;
                }
            }

            if (string.IsNullOrEmpty(digitos))
            {
                int candidato = 88011000 + cliente.Id;
                while (telefonosUsados.Contains(candidato.ToString("D8")))
                {
                    candidato++;
                }
                digitos = candidato.ToString("D8");
            }

            telefonosUsados.Add(digitos);
            string formateado = Generales.FormatearTelefono(digitos);

            if (principalTel != null)
            {
                if (!string.Equals(principalTel.NumeroTelefono, formateado, StringComparison.Ordinal) ||
                    principalTel.EsPrincipal != 1)
                {
                    principalTel.NumeroTelefono = formateado;
                    principalTel.EsPrincipal = 1;
                    if (string.IsNullOrWhiteSpace(principalTel.TipoTelefono))
                    {
                        principalTel.TipoTelefono = "Principal";
                    }
                    huboCambiosTelefonos = true;
                }
            }
            else
            {
                context.TelefonosCliente.Add(new TelefonosCliente
                {
                    ClienteId = cliente.Id,
                    TipoTelefono = "Principal",
                    NumeroTelefono = formateado,
                    EsPrincipal = 1
                });
                huboCambiosTelefonos = true;
            }
        }

        if (huboCambiosTelefonos)
        {
            await context.SaveChangesAsync();
        }
    }

    private static bool EsDescripcionGenerica(string? descripcion) =>
        string.IsNullOrWhiteSpace(descripcion) ||
        descripcion.Contains("casi todos los permisos", StringComparison.OrdinalIgnoreCase) ||
        descripcion.Contains("sin descripción", StringComparison.OrdinalIgnoreCase);
}
