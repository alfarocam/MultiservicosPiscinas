using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiserviciosPiscinas.Models;

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
            await AsegurarCategoriasGastoOperativoAsync();

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
    ALTER TABLE ops.INSPECCION ADD temperatura_c float NULL;";

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
}
