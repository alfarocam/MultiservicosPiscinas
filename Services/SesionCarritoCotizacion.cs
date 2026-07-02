using System.Text.Json;
using MultiserviciosPiscinas.DTOs.Cotizacion;

namespace MultiserviciosPiscinas.Services;

public static class SesionCarritoCotizacion
{
    private const string Clave = "CarritoCotizacion";

    public static List<ItemCarritoDto> ObtenerCarrito(this ISession session)
    {
        var json = session.GetString(Clave);
        return json == null ? new List<ItemCarritoDto>() : JsonSerializer.Deserialize<List<ItemCarritoDto>>(json) ?? new List<ItemCarritoDto>();
    }

    public static void GuardarCarrito(this ISession session, List<ItemCarritoDto> carrito)
        => session.SetString(Clave, JsonSerializer.Serialize(carrito));

    public static void LimpiarCarrito(this ISession session)
        => session.Remove(Clave);
}
