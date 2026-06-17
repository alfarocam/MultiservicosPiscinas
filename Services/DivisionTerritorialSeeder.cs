using System.Net.Http.Json;
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

    public async Task SeedAsync()
    {
        logger.LogInformation("=== [SEEDER] Verificando base de datos de ubicación ===");

        // 1. Verificar si la tabla ya tiene datos
        if (await dbContext.Provincia.AnyAsync())
        {
            logger.LogInformation("[SEEDER] La base de datos ya contiene datos territoriales. Omitiendo.");
            return;
        }

        try
        {
            logger.LogInformation("[SEEDER] Descargando provincias de la API...");
            var provRaw = await httpClient.GetStringAsync($"{BaseUrl}/provincias");

            using var doc = JsonDocument.Parse(provRaw);

            // Entrar a la propiedad "data" del JSON
            if (!doc.RootElement.TryGetProperty("data", out var provinciasData) || provinciasData.ValueKind != JsonValueKind.Array)
            {
                logger.LogError("[SEEDER] No se encontró la propiedad 'data' o no es un arreglo en el JSON de Provincias.");
                return;
            }

            // Procesar las Provincias
            foreach (var itemProv in provinciasData.EnumerateArray())
            {
                int provinciaId = itemProv.GetProperty("idProvincia").GetInt32();
                string provinciaNombre = itemProv.GetProperty("descripcion").GetString() ?? "";

                logger.LogInformation($"[SEEDER] Procesando provincia: {provinciaNombre} (ID: {provinciaId})...");

                var provincia = new Provincia { Id = provinciaId, Nombre = provinciaNombre };
                dbContext.Provincia.Add(provincia);

                // PROCESAR CANTONES DE ESTA PROVINCIA
                var cantRaw = await httpClient.GetStringAsync($"{BaseUrl}/provincias/{provinciaId}/cantones");
                using var cantDoc = JsonDocument.Parse(cantRaw);

                if (cantDoc.RootElement.TryGetProperty("data", out var cantonesData) && cantonesData.ValueKind == JsonValueKind.Array)
                {
                    foreach (var itemCant in cantonesData.EnumerateArray())
                    {
                        int cantonId = itemCant.GetProperty("idCanton").GetInt32();
                        string cantonNombre = itemCant.GetProperty("descripcion").GetString() ?? "";

                        var canton = new Canton { Id = cantonId, ProvinciaId = provinciaId, Nombre = cantonNombre };
                        dbContext.Canton.Add(canton);

                        // PROCESAR DISTRITOS DE ESTE CANTÓN
                        var distRaw = await httpClient.GetStringAsync($"{BaseUrl}/cantones/{cantonId}/distritos");
                        using var distDoc = JsonDocument.Parse(distRaw);

                        if (distDoc.RootElement.TryGetProperty("data", out var distritosData) && distritosData.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var itemDist in distritosData.EnumerateArray())
                            {
                                int distritoId = itemDist.GetProperty("idDistrito").GetInt32();
                                string distritoNombre = itemDist.GetProperty("descripcion").GetString() ?? "";

                                var distrito = new Distrito { Id = distritoId, CantonId = cantonId, Nombre = distritoNombre };
                                dbContext.Distrito.Add(distrito);
                            }
                        }
                    }
                }

                // Guardar los cambios en la base de datos por cada provincia procesada
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"[SEEDER] ¡Provincia {provinciaNombre} guardada con éxito con sus cantones y distritos!");
            }

            logger.LogInformation("=== [SEEDER] ¡Toda Costa Rica se ha guardado exitosamente en SQL Server! ===");
        }
        catch (Exception ex)
        {
            logger.LogCritical($"[SEEDER] Error crítico durante la ejecución: {ex.Message}");
        }
    }
}