using MultiserviciosPiscinas.DTOs.Factura;
using QuestPDF.Fluent;

namespace MultiserviciosPiscinas.Services;

public class FacturaPdfService
{
    public byte[] GenerarPdf(FacturaPdfDto datos)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header().ShowOnce().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("FACTURA")
                            .FontSize(24)
                            .Bold();

                        row.RelativeItem().AlignRight().Text(datos.NumeroFactura)
                            .FontSize(16)
                            .Bold();
                    });

                    col.Item().PaddingTop(10).BorderBottom(1);
                });

                page.Content().Column(col =>
                {
                    // Datos cliente
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("CLIENTE").Bold();
                            c.Item().Text(datos.NombreCliente);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("FECHA EMISIÓN").Bold();
                            c.Item().Text(datos.FechaEmision.ToString("dd/MM/yyyy"));

                            c.Item().PaddingTop(10).Text("FECHA VENCIMIENTO").Bold();
                            c.Item().Text(datos.FechaVencimiento.ToString("dd/MM/yyyy"));

                            c.Item().PaddingTop(10).Text("CONDICIÓN").Bold();
                            c.Item().Text(datos.CondicionPago);
                        });
                    });

                    col.Item().PaddingTop(20).Table(table =>
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

                        // Encabezados
                        table.Header(header =>
                        {
                            header.Cell().Background("#f0f0f0").Text("Descripción").Bold();
                            header.Cell().Background("#f0f0f0").AlignRight().Text("P. Unit.").Bold();
                            header.Cell().Background("#f0f0f0").AlignRight().Text("Cantidad").Bold();
                            header.Cell().Background("#f0f0f0").AlignRight().Text("Subtotal").Bold();
                            header.Cell().Background("#f0f0f0").AlignRight().Text("IVA").Bold();
                            header.Cell().Background("#f0f0f0").AlignRight().Text("Total").Bold();
                        });

                        // Filas
                        foreach (var linea in datos.Lineas)
                        {
                            table.Cell().Text($"{linea.Nombre} - {(linea.Descripcion ?? "Sin descripción")}");
                            table.Cell().AlignRight().Text($"₡{linea.PrecioUnitario:N2}");
                            table.Cell().AlignRight().Text($"{linea.Cantidad:N2}");
                            table.Cell().AlignRight().Text($"₡{linea.LineaSubtotal:N2}");
                            table.Cell().AlignRight().Text($"₡{linea.LineaImpuesto:N2}");
                            table.Cell().AlignRight().Text($"₡{linea.LineaTotal:N2}");
                        }
                    });

                    // Totales
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem();

                        row.RelativeItem(2).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Subtotal:").AlignRight();
                                r.RelativeItem().AlignRight().Text($"₡{datos.Subtotal:N2}");
                            });

                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("IVA:").AlignRight();
                                r.RelativeItem().AlignRight().Text($"₡{datos.ImpuestoTotal:N2}");
                            });

                            c.Item().BorderTop(1).Row(r =>
                            {
                                r.RelativeItem().Text("TOTAL:").Bold().AlignRight();
                                r.RelativeItem().AlignRight().Text($"₡{datos.Total:N2}").Bold().FontSize(14);
                            });
                        });
                    });

                    if (!string.IsNullOrEmpty(datos.ComprobanteRutaFisica) && File.Exists(datos.ComprobanteRutaFisica))
                    {
                        col.Item().PaddingTop(30).Column(c =>
                        {
                            c.Item().AlignCenter().Text("Comprobante de Pago SINPE").Bold();
                            c.Item().PaddingTop(5).AlignCenter().MaxHeight(220).Image(datos.ComprobanteRutaFisica).FitArea();
                        });
                    }
                    else
                    {
                        col.Item().PaddingTop(30).AlignCenter().Text("Comprobante de pago SINPE adjunto")
                            .FontSize(10)
                            .Italic();
                    }
                });

                page.Footer().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10)
                    .FontColor("#808080");
            });
        }).GeneratePdf();
    }
}
