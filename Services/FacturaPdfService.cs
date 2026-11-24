using MultiserviciosPiscinas.DTOs.Factura;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace MultiserviciosPiscinas.Services;

public class FacturaPdfService(IWebHostEnvironment env)
{
    public byte[] GenerarPdf(FacturaPdfDto datos)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(PdfBrandHelper.ColorTextoOscuro));

                page.Header().ShowOnce().Element(c =>
                    PdfBrandHelper.ComponerEncabezado(c, env, "FACTURA", datos.NumeroFactura));

                page.Content().PaddingTop(18).Column(col =>
                {
                    // Datos cliente y fechas
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3)
                            .Background(PdfBrandHelper.ColorFondoSuave)
                            .BorderLeft(4)
                            .BorderColor(PdfBrandHelper.ColorTurquesa)
                            .Padding(12)
                            .Column(c =>
                            {
                                c.Item().Text("CLIENTE").FontSize(8.5f).Bold().FontColor(PdfBrandHelper.ColorTurquesaOscuro);
                                c.Item().PaddingTop(3).Text(datos.NombreCliente).FontSize(11).Bold().FontColor(PdfBrandHelper.ColorTextoOscuro);
                            });

                        row.ConstantItem(12);

                        row.RelativeItem(2)
                            .Background(PdfBrandHelper.ColorFondoSuave)
                            .BorderLeft(4)
                            .BorderColor(PdfBrandHelper.ColorAzulEnlace)
                            .Padding(12)
                            .Column(c =>
                            {
                                c.Item().Text("FECHA DE EMISIÓN").FontSize(8.5f).Bold().FontColor(PdfBrandHelper.ColorAzulEnlace);
                                c.Item().PaddingTop(2).Text(datos.FechaEmision.ToString("dd/MM/yyyy")).SemiBold().FontColor(PdfBrandHelper.ColorTextoOscuro);

                                c.Item().PaddingTop(6).Text("FECHA DE VENCIMIENTO").FontSize(8.5f).Bold().FontColor(PdfBrandHelper.ColorAzulEnlace);
                                c.Item().PaddingTop(2).Text(datos.FechaVencimiento.ToString("dd/MM/yyyy")).SemiBold().FontColor(PdfBrandHelper.ColorTextoOscuro);

                                c.Item().PaddingTop(6).Text("CONDICIÓN DE PAGO").FontSize(8.5f).Bold().FontColor(PdfBrandHelper.ColorAzulEnlace);
                                c.Item().PaddingTop(2).Text(datos.CondicionPago).SemiBold().FontColor(PdfBrandHelper.ColorTextoOscuro);
                            });
                    });

                    // Tabla de productos y servicios
                    col.Item().PaddingTop(18).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(PdfBrandHelper.ColorTurquesaOscuro).Padding(7).Text("Descripción").FontSize(9).Bold().FontColor(PdfBrandHelper.ColorBlanco);
                            header.Cell().Background(PdfBrandHelper.ColorTurquesaOscuro).Padding(7).AlignRight().Text("P. Unit.").FontSize(9).Bold().FontColor(PdfBrandHelper.ColorBlanco);
                            header.Cell().Background(PdfBrandHelper.ColorTurquesaOscuro).Padding(7).AlignRight().Text("Cantidad").FontSize(9).Bold().FontColor(PdfBrandHelper.ColorBlanco);
                            header.Cell().Background(PdfBrandHelper.ColorTurquesaOscuro).Padding(7).AlignRight().Text("Subtotal").FontSize(9).Bold().FontColor(PdfBrandHelper.ColorBlanco);
                            header.Cell().Background(PdfBrandHelper.ColorTurquesaOscuro).Padding(7).AlignRight().Text("IVA").FontSize(9).Bold().FontColor(PdfBrandHelper.ColorBlanco);
                            header.Cell().Background(PdfBrandHelper.ColorTurquesaOscuro).Padding(7).AlignRight().Text("Total").FontSize(9).Bold().FontColor(PdfBrandHelper.ColorBlanco);
                        });

                        var index = 0;
                        foreach (var linea in datos.Lineas)
                        {
                            var fondoFila = index % 2 == 0 ? PdfBrandHelper.ColorBlanco : PdfBrandHelper.ColorFondoSuave;
                            index++;

                            IContainer EstiloCelda(IContainer cell) =>
                                cell.Background(fondoFila)
                                    .BorderBottom(1)
                                    .BorderColor(PdfBrandHelper.ColorBordeSuave)
                                    .Padding(7);

                            table.Cell().Element(EstiloCelda).Text($"{linea.Nombre} - {(linea.Descripcion ?? "Sin descripción")}").FontSize(9.5f);
                            table.Cell().Element(EstiloCelda).AlignRight().Text($"₡{linea.PrecioUnitario:N2}").FontSize(9.5f);
                            table.Cell().Element(EstiloCelda).AlignRight().Text($"{linea.Cantidad:N2}").FontSize(9.5f);
                            table.Cell().Element(EstiloCelda).AlignRight().Text($"₡{linea.LineaSubtotal:N2}").FontSize(9.5f);
                            table.Cell().Element(EstiloCelda).AlignRight().Text($"₡{linea.LineaImpuesto:N2}").FontSize(9.5f);
                            table.Cell().Element(EstiloCelda).AlignRight().Text($"₡{linea.LineaTotal:N2}").FontSize(9.5f).Bold().FontColor(PdfBrandHelper.ColorTurquesaOscuro);
                        }
                    });

                    // Totales
                    col.Item().PaddingTop(16).Element(c =>
                        PdfBrandHelper.ComponerTotales(c, datos.Subtotal, datos.ImpuestoTotal, datos.Total));

                    // Comprobante SINPE
                    col.Item()
                        .PaddingTop(20)
                        .Background(PdfBrandHelper.ColorFondoSuave)
                        .Border(1)
                        .BorderColor(PdfBrandHelper.ColorBordeSuave)
                        .BorderTop(3)
                        .BorderColor(PdfBrandHelper.ColorTurquesa)
                        .Padding(12)
                        .Column(c =>
                        {
                            if (!string.IsNullOrEmpty(datos.ComprobanteRutaFisica) && File.Exists(datos.ComprobanteRutaFisica))
                            {
                                c.Item().AlignCenter().Text("Comprobante de Pago SINPE").FontSize(10).Bold().FontColor(PdfBrandHelper.ColorTurquesaOscuro);
                                c.Item().PaddingTop(8).AlignCenter().MaxHeight(220).Image(datos.ComprobanteRutaFisica).FitArea();
                            }
                            else
                            {
                                c.Item().AlignCenter().Text("Comprobante de pago SINPE adjunto")
                                    .FontSize(10)
                                    .Italic()
                                    .FontColor(PdfBrandHelper.ColorTextoSecundario);
                            }
                        });
                });

                page.Footer().Element(PdfBrandHelper.ComponerPiePagina);
            });
        }).GeneratePdf();
    }
}
