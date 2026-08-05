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

    // Costa Rica siempre tiene exactamente 7 provincias. Antes esta clase se
    // saltaba por completo si la tabla Provincia ya tenía AL MENOS una fila, lo
    // que dejaba la siembra pegada en un estado parcial si una corrida anterior
    // se interrumpía a medio camino (por ejemplo, una falla de red llamando a la
    // API externa después de guardar solo San José). Ahora solo se omite cuando
    // ya están las 7, y cada provincia se procesa de forma independiente para
    // que una falla puntual no le impida seguir con el resto.
    private const int TotalProvinciasCostaRica = 7;

    public async Task SeedAsync()
    {
        logger.LogInformation("=== [SEEDER] Verificando base de datos de ubicación ===");

        try
        {
            if (await dbContext.Provincia.CountAsync() >= TotalProvinciasCostaRica)
            {
                logger.LogInformation("[SEEDER] Las 7 provincias ya están cargadas. Omitiendo.");
                return;
            }

            logger.LogInformation("[SEEDER] Descargando provincias de la API...");
            var provRaw = await httpClient.GetStringAsync($"{BaseUrl}/provincias");

            using var doc = JsonDocument.Parse(provRaw);

            // Entrar a la propiedad "data" del JSON
            if (!doc.RootElement.TryGetProperty("data", out var provinciasData) || provinciasData.ValueKind != JsonValueKind.Array)
            {
                logger.LogError("[SEEDER] No se encontró la propiedad 'data' o no es un arreglo en el JSON de Provincias.");
                return;
            }

            // Provincias que ya existen (por ejemplo, San José de una corrida
            // anterior incompleta) se saltan para no duplicarlas ni chocar con
            // direcciones de clientes que ya las referencian.
            var provinciasExistentes = await dbContext.Provincia.Select(p => p.Id).ToListAsync();

            // Procesar las Provincias
            foreach (var itemProv in provinciasData.EnumerateArray())
            {
                int provinciaId = itemProv.GetProperty("idProvincia").GetInt32();
                string provinciaNombre = itemProv.GetProperty("descripcion").GetString() ?? "";

                if (provinciasExistentes.Contains(provinciaId))
                {
                    logger.LogInformation($"[SEEDER] Provincia {provinciaNombre} (ID: {provinciaId}) ya existe, se omite.");
                    continue;
                }

                try
                {
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
                catch (Exception exProvincia)
                {
                    // No se relanza: que falle una provincia (p. ej. un timeout
                    // puntual de la API externa) no debe impedir que se sigan
                    // procesando las demás. En el próximo arranque, esta misma
                    // provincia se vuelve a intentar porque no quedó guardada.
                    logger.LogError($"[SEEDER] No se pudo procesar la provincia {provinciaNombre} (ID: {provinciaId}): {exProvincia.Message}");
                    dbContext.ChangeTracker.Clear();
                }
            }

            logger.LogInformation("=== [SEEDER] Siembra de división territorial finalizada ===");
        }
        catch (Exception ex)
        {
            logger.LogCritical($"[SEEDER] Error crítico durante la ejecución: {ex.Message}");
        }
    }
}