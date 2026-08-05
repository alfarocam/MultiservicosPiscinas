using ClosedXML.Excel;
using MultiserviciosPiscinas.DTOs;

namespace MultiserviciosPiscinas.Services
{
    public class ReporteEficienciaRutasExcelService
    {
        public byte[] GenerarExcel(ReporteEficienciaRutasDto reporte)
        {
            using var workbook = new XLWorkbook();

            CrearHojaResumen(workbook, reporte);
            CrearHojaDetalle(workbook, reporte);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        private static void CrearHojaResumen(XLWorkbook workbook, ReporteEficienciaRutasDto reporte)
        {
            var ws = workbook.Worksheets.Add("Resumen");

            ws.Cell(1, 1).Value = "REPORTE DE EFICIENCIA DE RUTAS";
            ws.Range(1, 1, 1, 9).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;

            ws.Cell(3, 1).Value = "Fecha desde";
            ws.Cell(3, 2).Value = reporte.FechaDesde?.ToString("dd/MM/yyyy") ?? "Sin filtro";

            ws.Cell(4, 1).Value = "Fecha hasta";
            ws.Cell(4, 2).Value = reporte.FechaHasta?.ToString("dd/MM/yyyy") ?? "Sin filtro";

            ws.Cell(6, 1).Value = "Técnico";
            ws.Cell(6, 2).Value = "Total rutas";
            ws.Cell(6, 3).Value = "Total visitas";
            ws.Cell(6, 4).Value = "Visitas completadas";
            ws.Cell(6, 5).Value = "Distancia optimizada km";
            ws.Cell(6, 6).Value = "Distancia real km";
            ws.Cell(6, 7).Value = "Diferencia km";
            ws.Cell(6, 8).Value = "Duración min";
            ws.Cell(6, 9).Value = "Eficiencia %";

            var header = ws.Range(6, 1, 6, 9);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
            header.Style.Font.FontColor = XLColor.White;

            int row = 7;

            foreach (var item in reporte.Tecnicos)
            {
                ws.Cell(row, 1).Value = item.Tecnico;
                ws.Cell(row, 2).Value = item.TotalRutas;
                ws.Cell(row, 3).Value = item.TotalVisitas;
                ws.Cell(row, 4).Value = item.VisitasCompletadas;
                ws.Cell(row, 5).Value = item.DistanciaOptimizadaKm;
                ws.Cell(row, 6).Value = item.DistanciaRealKm;
                ws.Cell(row, 7).Value = item.DiferenciaKm;
                ws.Cell(row, 8).Value = item.DuracionTotalMin;
                ws.Cell(row, 9).Value = item.EficienciaPorcentaje;

                ws.Cell(row, 5).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 6).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 7).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 9).Style.NumberFormat.Format = "0.00";

                row++;
            }

            if (reporte.Tecnicos.Any())
            {
                ws.Cell(row, 1).Value = "TOTAL";
                ws.Cell(row, 2).Value = reporte.TotalRutas;
                ws.Cell(row, 3).Value = reporte.TotalVisitas;
                ws.Cell(row, 5).Value = reporte.TotalDistanciaOptimizadaKm;
                ws.Cell(row, 6).Value = reporte.TotalDistanciaRealKm;
                ws.Cell(row, 7).Value = reporte.DiferenciaTotalKm;
                ws.Cell(row, 9).Value = reporte.EficienciaGeneralPorcentaje;

                var total = ws.Range(row, 1, row, 9);
                total.Style.Font.Bold = true;
                total.Style.Fill.BackgroundColor = XLColor.FromArgb(0xD9EAF7);
            }

            ws.Columns().AdjustToContents();
        }

        private static void CrearHojaDetalle(XLWorkbook workbook, ReporteEficienciaRutasDto reporte)
        {
            var ws = workbook.Worksheets.Add("Detalle");

            ws.Cell(1, 1).Value = "Ruta ID";
            ws.Cell(1, 2).Value = "Fecha";
            ws.Cell(1, 3).Value = "Técnico";
            ws.Cell(1, 4).Value = "Total visitas";
            ws.Cell(1, 5).Value = "Visitas completadas";
            ws.Cell(1, 6).Value = "Distancia optimizada km";
            ws.Cell(1, 7).Value = "Distancia real km";
            ws.Cell(1, 8).Value = "Diferencia km";
            ws.Cell(1, 9).Value = "Duración estimada min";
            ws.Cell(1, 10).Value = "Eficiencia %";

            var header = ws.Range(1, 1, 1, 10);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
            header.Style.Font.FontColor = XLColor.White;

            int row = 2;

            foreach (var item in reporte.Detalle)
            {
                ws.Cell(row, 1).Value = item.RutaId;
                ws.Cell(row, 2).Value = item.Fecha.ToString("dd/MM/yyyy");
                ws.Cell(row, 3).Value = item.Tecnico;
                ws.Cell(row, 4).Value = item.TotalVisitas;
                ws.Cell(row, 5).Value = item.VisitasCompletadas;
                ws.Cell(row, 6).Value = item.DistanciaOptimizadaKm;
                ws.Cell(row, 7).Value = item.DistanciaRealKm;
                ws.Cell(row, 8).Value = item.DiferenciaKm;
                ws.Cell(row, 9).Value = item.DuracionEstimadaMin;
                ws.Cell(row, 10).Value = item.EficienciaPorcentaje;

                ws.Cell(row, 6).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 7).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 8).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";

                row++;
            }

            ws.Columns().AdjustToContents();
        }
    }
}