using ClosedXML.Excel;
using MultiserviciosPiscinas.Models;
using System.IO;

namespace MultiserviciosPiscinas.Services
{
    public class ReportesGeneralesExcelService
    {
        public byte[] GenerarExcelServicios(ReporteServiciosViewModel reporte)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Servicios Realizados");

                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "Cliente";
                ws.Cell(1, 3).Value = "Tipo Servicio";
                ws.Cell(1, 4).Value = "Técnico";
                ws.Cell(1, 5).Value = "Estado";
                ws.Cell(1, 6).Value = "Fecha y Hora";

                var headerRange = ws.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var item in reporte.Servicios)
                {
                    ws.Cell(row, 1).Value = item.Id;
                    ws.Cell(row, 2).Value = item.Cliente;
                    ws.Cell(row, 3).Value = item.TipoServicio;
                    ws.Cell(row, 4).Value = item.Tecnico;
                    ws.Cell(row, 5).Value = item.Estado;
                    ws.Cell(row, 6).Value = item.FechaHora.ToString("yyyy-MM-dd HH:mm");
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerarExcelProyectos(ReporteProyectosViewModel reporte)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Proyectos");

                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "Nombre";
                ws.Cell(1, 3).Value = "Cliente";
                ws.Cell(1, 4).Value = "Estado";
                ws.Cell(1, 5).Value = "Inicio";
                ws.Cell(1, 6).Value = "Fin Estimado";
                ws.Cell(1, 7).Value = "Presupuesto";

                var headerRange = ws.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var item in reporte.Proyectos)
                {
                    ws.Cell(row, 1).Value = item.Id;
                    ws.Cell(row, 2).Value = item.Nombre;
                    ws.Cell(row, 3).Value = item.Cliente;
                    ws.Cell(row, 4).Value = item.Estado;
                    ws.Cell(row, 5).Value = item.FechaInicio.ToString("yyyy-MM-dd");
                    ws.Cell(row, 6).Value = item.FechaFinEstimada?.ToString("yyyy-MM-dd") ?? "N/A";
                    ws.Cell(row, 7).Value = item.Presupuesto;
                    ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerarExcelRentabilidad(ReporteRentabilidadViewModel reporte)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Rentabilidad Anual");

                ws.Cell(1, 1).Value = "Mes";
                ws.Cell(1, 2).Value = "Ingresos";
                ws.Cell(1, 3).Value = "Gastos";
                ws.Cell(1, 4).Value = "Rentabilidad";

                var headerRange = ws.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var item in reporte.Detalles)
                {
                    ws.Cell(row, 1).Value = item.Mes;
                    ws.Cell(row, 2).Value = item.Ingresos;
                    ws.Cell(row, 3).Value = item.Gastos;
                    ws.Cell(row, 4).Value = item.Rentabilidad;

                    ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    row++;
                }

                ws.Cell(row, 1).Value = "TOTAL";
                ws.Cell(row, 2).Value = reporte.TotalIngresos;
                ws.Cell(row, 3).Value = reporte.TotalGastos;
                ws.Cell(row, 4).Value = reporte.RentabilidadTotal;
                
                var footerRange = ws.Range(row, 1, row, 4);
                footerRange.Style.Font.Bold = true;
                ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
