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
}
