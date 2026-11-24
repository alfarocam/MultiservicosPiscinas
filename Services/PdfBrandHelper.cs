using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace MultiserviciosPiscinas.Services;

/// <summary>
/// Helpers de marca, paleta de colores del sistema y componentes reutilizables para los PDFs generados con QuestPDF.
/// </summary>
public static class PdfBrandHelper
{
    public const string ColorPrimarioOscuro = "#002d38";
    public const string ColorTurquesa = "#0a9396";
    public const string ColorTurquesaOscuro = "#005f73";
    public const string ColorAzulEnlace = "#0077b6";
    public const string ColorAcentoDorado = "#e9d8a6";
    public const string ColorFondoSuave = "#f4f8fb";
    public const string ColorFondoCabeceraSuave = "#e6f4f7";
    public const string ColorBordeSuave = "#d0e8f0";
    public const string ColorTextoOscuro = "#1a2f3b";
    public const string ColorTextoSecundario = "#627b87";
    public const string ColorBlanco = "#ffffff";

    private static readonly object LogoLock = new();
    private static SvgImage? _logoCache;
    private static string? _logoCachePath;

    /// <summary>
    /// Carga y cachea el logotipo SVG oficial de la empresa desde wwwroot/images/svg/LogoMultiServiciosYPiscinas.svg.
    /// </summary>
    public static SvgImage? CargarLogoSvg(IWebHostEnvironment env)
    {
        var webRoot = !string.IsNullOrWhiteSpace(env.WebRootPath)
            ? env.WebRootPath
            : Path.Combine(env.ContentRootPath, "wwwroot");

        var rutaLogo = Path.Combine(webRoot, "images", "svg", "LogoMultiServiciosYPiscinas.svg");
        if (!File.Exists(rutaLogo))
            return null;

        lock (LogoLock)
        {
            if (_logoCache != null && string.Equals(_logoCachePath, rutaLogo, StringComparison.OrdinalIgnoreCase))
                return _logoCache;

            _logoCache = SvgImage.FromFile(rutaLogo);
            _logoCachePath = rutaLogo;
            return _logoCache;
        }
    }

    /// <summary>
    /// Compone el encabezado institucional del documento con el logotipo oficial y la paleta del sistema.
    /// </summary>
    public static void ComponerEncabezado(
        IContainer container,
        IWebHostEnvironment env,
        string tipoDocumento,
        string numeroDocumento)
    {
        var logo = CargarLogoSvg(env);

        container.Column(col =>
        {
            col.Item()
                .Background(ColorPrimarioOscuro)
                .BorderBottom(3)
                .BorderColor(ColorAcentoDorado)
                .Padding(16)
                .Row(row =>
                {
                    row.RelativeItem(3).Row(brandRow =>
                    {
                        if (logo != null)
                        {
                            brandRow.ConstantItem(56)
                                .Height(58)
                                .Background(ColorBlanco)
                                .Border(1)
                                .BorderColor(ColorAcentoDorado)
                                .Padding(4)
                                .AlignMiddle()
                                .AlignCenter()
                                .Svg(logo)
                                .FitArea();
                        }

                        brandRow.RelativeItem()
                            .PaddingLeft(logo != null ? 12 : 0)
                            .AlignMiddle()
                            .Column(titleCol =>
                            {
                                titleCol.Item()
                                    .Text("MULTISERVICIOS Y PISCINAS")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(ColorBlanco);

                                titleCol.Item()
                                    .PaddingTop(2)
                                    .Text("Servicios Profesionales de Piscinas y Construcción")
                                    .FontSize(8.5f)
                                    .FontColor(ColorAcentoDorado);
                            });
                    });

                    row.RelativeItem(2)
                        .AlignRight()
                        .AlignMiddle()
                        .Column(docCol =>
                        {
                            docCol.Item()
                                .AlignRight()
                                .Text(tipoDocumento)
                                .FontSize(18)
                                .Bold()
                                .FontColor(ColorBlanco);

                            docCol.Item()
                                .PaddingTop(5)
                                .AlignRight()
                                .Background(ColorTurquesa)
                                .PaddingVertical(4)
                                .PaddingHorizontal(10)
                                .Text(numeroDocumento)
                                .FontSize(11)
                                .Bold()
                                .FontColor(ColorBlanco);
                        });
                });
        });
    }

    /// <summary>
    /// Compone el bloque estándar de subtotal, IVA y total final con los colores institucionales.
    /// </summary>
    public static void ComponerTotales(IContainer container, decimal subtotal, decimal impuestoTotal, decimal total)
    {
        container.Row(row =>
        {
            row.RelativeItem();

            row.ConstantItem(240)
                .Border(1)
                .BorderColor(ColorBordeSuave)
                .Background(ColorFondoSuave)
                .Column(c =>
                {
                    c.Item()
                        .PaddingHorizontal(10)
                        .PaddingVertical(6)
                        .Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontColor(ColorTextoSecundario).SemiBold();
                            r.RelativeItem().AlignRight().Text($"₡{subtotal:N2}").FontColor(ColorTextoOscuro);
                        });

                    c.Item()
                        .BorderTop(1)
                        .BorderColor(ColorBordeSuave)
                        .PaddingHorizontal(10)
                        .PaddingVertical(6)
                        .Row(r =>
                        {
                            r.RelativeItem().Text("IVA:").FontColor(ColorTextoSecundario).SemiBold();
                            r.RelativeItem().AlignRight().Text($"₡{impuestoTotal:N2}").FontColor(ColorTextoOscuro);
                        });

                    c.Item()
                        .Background(ColorPrimarioOscuro)
                        .PaddingHorizontal(10)
                        .PaddingVertical(8)
                        .Row(r =>
                        {
                            r.RelativeItem().AlignMiddle().Text("TOTAL:").FontSize(11).Bold().FontColor(ColorAcentoDorado);
                            r.RelativeItem().AlignRight().AlignMiddle().Text($"₡{total:N2}").FontSize(13).Bold().FontColor(ColorBlanco);
                        });
                });
        });
    }

    /// <summary>
    /// Compone el pie de página estándar con marca, fecha/hora de generación y numeración de páginas.
    /// </summary>
    public static void ComponerPiePagina(IContainer container)
    {
        container
            .BorderTop(1.5f)
            .BorderColor(ColorTurquesa)
            .PaddingTop(8)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text("Multiservicios y Piscinas")
                    .FontSize(9)
                    .SemiBold()
                    .FontColor(ColorTurquesaOscuro);

                row.RelativeItem()
                    .AlignCenter()
                    .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(9)
                    .FontColor(ColorTextoSecundario);

                row.RelativeItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(ColorTextoSecundario));
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
            });
    }
}
