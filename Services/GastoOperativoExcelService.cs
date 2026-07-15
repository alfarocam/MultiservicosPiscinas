using ClosedXML.Excel;
using MultiserviciosPiscinas.DTOs;

namespace MultiserviciosPiscinas.Services
{
    public class GastoOperativoExcelService
    {
        public byte[] GenerarExcel(ReporteGastosOperativosDto reporte)
        {
            using (var workbook = new XLWorkbook())
            {
                // Hoja 1: Resumen por Categoría
                var wsResumen = workbook.Worksheets.Add("Resumen por Categoria");

                // Encabezados
                wsResumen.Cell(1, 1).Value = "Categoría";
                wsResumen.Cell(1, 2).Value = "Total";
                wsResumen.Cell(1, 3).Value = "Cantidad";
                wsResumen.Cell(1, 4).Value = "Porcentaje (%)";

                // Formatear encabezados
                var headerRange = wsResumen.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
                headerRange.Style.Font.FontColor = XLColor.White;

                // Llenar datos de resumen
                int rowResumen = 2;
                foreach (var categoria in reporte.TotalesPorCategoria)
                {
                    wsResumen.Cell(rowResumen, 1).Value = categoria.NombreCategoria;
                    wsResumen.Cell(rowResumen, 2).Value = categoria.Total;
                    wsResumen.Cell(rowResumen, 3).Value = categoria.Cantidad;
                    wsResumen.Cell(rowResumen, 4).Value = categoria.Porcentaje;

                    // Formato de moneda para la columna de Total
                    wsResumen.Cell(rowResumen, 2).Style.NumberFormat.Format = "#,##0.00";
                    wsResumen.Cell(rowResumen, 4).Style.NumberFormat.Format = "0.00";

                    rowResumen++;
                }

                // Fila de total
                if (reporte.TotalesPorCategoria.Count > 0)
                {
                    wsResumen.Cell(rowResumen, 1).Value = "TOTAL";
                    wsResumen.Cell(rowResumen, 2).Value = reporte.TotalGeneral;
                    wsResumen.Cell(rowResumen, 1).Style.Font.Bold = true;
                    wsResumen.Cell(rowResumen, 2).Style.Font.Bold = true;
                    wsResumen.Cell(rowResumen, 2).Style.NumberFormat.Format = "#,##0.00";
                }

                // Ajustar ancho de columnas
                wsResumen.Column(1).Width = 25;
                wsResumen.Column(2).Width = 15;
                wsResumen.Column(3).Width = 12;
                wsResumen.Column(4).Width = 15;

                // Hoja 2: Detalle
                var wsDetalle = workbook.Worksheets.Add("Detalle");

                // Encabezados
                wsDetalle.Cell(1, 1).Value = "Fecha";
                wsDetalle.Cell(1, 2).Value = "Categoría";
                wsDetalle.Cell(1, 3).Value = "Descripción";
                wsDetalle.Cell(1, 4).Value = "Monto";

                // Formatear encabezados
                var headerRangeDetalle = wsDetalle.Range(1, 1, 1, 4);
                headerRangeDetalle.Style.Font.Bold = true;
                headerRangeDetalle.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
                headerRangeDetalle.Style.Font.FontColor = XLColor.White;

                // Llenar datos de detalle
                int rowDetalle = 2;
                foreach (var gasto in reporte.Detalle)
                {
                    wsDetalle.Cell(rowDetalle, 1).Value = gasto.Fecha.ToString("yyyy-MM-dd");
                    wsDetalle.Cell(rowDetalle, 2).Value = gasto.NombreCategoria;
                    wsDetalle.Cell(rowDetalle, 3).Value = gasto.Descripcion ?? string.Empty;
                    wsDetalle.Cell(rowDetalle, 4).Value = gasto.Monto;

                    // Formato de moneda
                    wsDetalle.Cell(rowDetalle, 4).Style.NumberFormat.Format = "#,##0.00";

                    rowDetalle++;
                }

                // Fila de total en detalle
                if (reporte.Detalle.Count > 0)
                {
                    wsDetalle.Cell(rowDetalle, 3).Value = "TOTAL";
                    wsDetalle.Cell(rowDetalle, 4).Value = reporte.TotalGeneral;
                    wsDetalle.Cell(rowDetalle, 3).Style.Font.Bold = true;
                    wsDetalle.Cell(rowDetalle, 4).Style.Font.Bold = true;
                    wsDetalle.Cell(rowDetalle, 4).Style.NumberFormat.Format = "#,##0.00";
                }

                // Ajustar ancho de columnas
                wsDetalle.Column(1).Width = 12;
                wsDetalle.Column(2).Width = 25;
                wsDetalle.Column(3).Width = 40;
                wsDetalle.Column(4).Width = 15;

                // Convertir a bytes
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
