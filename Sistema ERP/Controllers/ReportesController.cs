using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerReportes")]
    public class ReportesController : Controller
    {
        private readonly ErpInventarioContext _context;

        public ReportesController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Compra(string periodo = "Mes")
        {
            var fechaInicio = CalcularFechaInicio(periodo);
            var fechaFin = DateTime.Now;

            var compras = await _context.InventarioCompras
                .Include(c => c.IdProductoNavigation)
                .Include(c => c.IdProveedorNavigation)
                .Where(c => c.FechaIngreso >= fechaInicio && c.FechaIngreso <= fechaFin)
                .OrderByDescending(c => c.IdIngreso)
                .ToListAsync();

            var model = new ReporteCompraViewModel
            {
                Periodo = periodo,
                TotalGasto = compras.Sum(c => (decimal)c.CantidadComprada * c.CostoCompraUnitario),
                CantidadCompras = compras.Count,
                ProveedorPrincipal = compras.GroupBy(c => c.IdProveedorNavigation != null ? c.IdProveedorNavigation.Nombre : "N/A")
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "N/A",
                Detalles = compras.Select(c => new CompraDetalleDto
                {
                    IdCompra = c.IdIngreso,
                    Producto = c.IdProductoNavigation?.NombreProducto ?? "N/A",
                    Proveedor = c.IdProveedorNavigation?.Nombre ?? "N/A",
                    Fecha = c.FechaIngreso ?? DateTime.Now,
                    Cantidad = c.CantidadComprada,
                    CostoUnitario = c.CostoCompraUnitario
                }).ToList(),
                ChartData = GenerarChartDataCompras(compras, periodo)
            };

            return View(model);
        }


        public async Task<IActionResult> Venta(string periodo = "Mes")
        {
            var fechaInicio = CalcularFechaInicio(periodo);
            var fechaFin = DateTime.Now;

            var ventas = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.DetalleCotizacionProductos)
                .ThenInclude(d => d.IdProductoNavigation)
                .Where(c => c.TipoOperacion == "Venta" && c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
                .OrderByDescending(c => c.IdCotizacion)
                .ToListAsync();

            var model = new ReporteVentaViewModel
            {
                Periodo = periodo,
                TotalIngresos = ventas.Sum(v => v.TotalConImpuesto ?? 0m),
                TotalCostos = ventas.SelectMany(v => v.DetalleCotizacionProductos).Sum(d => d.CostoReferencial),
                Detalles = ventas.Select(v => new VentaDetalleDto
                {
                    IdVenta = v.IdCotizacion,
                    Cliente = v.IdClienteNavigation?.NombreRazonSocial ?? "N/A",
                    Fecha = v.Fecha ?? DateTime.Now,
                    ImporteBruto = v.TotalConImpuesto ?? 0m,
                    CostoEstimado = v.DetalleCotizacionProductos.Sum(d => d.CostoReferencial)
                }).ToList(),
                ChartData = GenerarChartDataVentas(ventas, periodo)
            };

            return View(model);
        }

        public async Task<IActionResult> Stock()
        {
            var productos = await _context.InventarioProductos
                .OrderByDescending(p => p.IdProducto)
                .ToListAsync();

            var model = new ReporteStockViewModel
            {
                TotalProductos = productos.Count,
                ValorTotalInventario = productos.Sum(p => (decimal)(p.Stock ?? 0) * p.PrecioCompra),
                ItemsEnCritico = productos.Count(p => (p.Stock ?? 0) <= 5),
                CategoriaDominante = productos.GroupBy(p => p.Categoria)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "Otros",
                Detalles = productos.Select(p => new StockDetalleDto
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.NombreProducto,
                    Tipo = p.Categoria ?? "General",
                    Cantidadactual = p.Stock ?? 0,
                    StockMinimo = 5,
                    PrecioVenta = p.PrecioVentaSugerido
                }).ToList(),
                ChartData = productos.Take(15)
                    .Select(p => new ChartDataDto
                    {
                        Label = p.NombreProducto,
                        Value = (decimal)(p.Stock ?? 0)
                    }).ToList()
            };

            return View(model);
        }

        private DateTime CalcularFechaInicio(string periodo)
        {
            return periodo switch
            {
                "Dia" => DateTime.Today,
                "Semana" => DateTime.Today.AddDays(-7),
                "Mes" => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                "1_Año" => DateTime.Now.AddYears(-1),
                "2_Años" => DateTime.Now.AddYears(-2),
                "3_Años" => DateTime.Now.AddYears(-3),
                "4_Años" => DateTime.Now.AddYears(-4),
                "5_Años" => DateTime.Now.AddYears(-5),
                _ => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            };
        }

        private List<ChartDataDto> GenerarChartDataCompras(List<InventarioCompra> compras, string periodo)
        {
            if (periodo == "Dia")
            {
                return compras.GroupBy(c => c.FechaIngreso?.ToString("HH:mm") ?? "")
                    .Select(g => new ChartDataDto { Label = g.Key, Value = g.Sum(c => (decimal)c.CantidadComprada * c.CostoCompraUnitario) })
                    .OrderBy(x => x.Label)
                    .ToList();
            }

            return compras.GroupBy(c => c.FechaIngreso?.ToString("yyyy-MM-dd") ?? "")
                .Select(g => new ChartDataDto { Label = g.Key, Value = g.Sum(c => (decimal)c.CantidadComprada * c.CostoCompraUnitario) })
                .OrderBy(x => x.Label)
                .ToList();
        }

        private List<ChartDataDto> GenerarChartDataVentas(List<Cotizacione> ventas, string periodo)
        {
            if (periodo == "Dia")
            {
                return ventas.GroupBy(v => v.Fecha?.ToString("HH:mm") ?? "")
                    .Select(g => new ChartDataDto { Label = g.Key, Value = g.Sum(v => v.TotalConImpuesto ?? 0m) })
                    .OrderBy(x => x.Label)
                    .ToList();
            }

            return ventas.GroupBy(v => v.Fecha?.ToString("yyyy-MM-dd") ?? "")
                .Select(g => new ChartDataDto { Label = g.Key, Value = g.Sum(v => v.TotalConImpuesto ?? 0m) })
                .OrderBy(x => x.Label)
                .ToList();
        }


        public async Task<IActionResult> ExportarCompraExcel(string periodo = "Mes")
        {
            var fechaInicio = CalcularFechaInicio(periodo);
            var compras = await _context.InventarioCompras
                .Include(c => c.IdProductoNavigation)
                .Include(c => c.IdProveedorNavigation)
                .Where(c => c.FechaIngreso >= fechaInicio)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Compras");


                ws.Cell(1, 1).Value = "REPORTE DE COMPRAS - STC Services";
                ws.Range(1, 1, 1, 8).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


                var headers = new string[] { "ID", "Fecha", "Proveedor", "NIT/CI", "Producto", "Cantidad", "Costo U.", "Total" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(3, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Fill.BackgroundColor = XLColor.Black;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Font.Bold = true;
                }

                int row = 4;
                foreach (var c in compras)
                {
                    ws.Cell(row, 1).Value = c.IdIngreso;
                    ws.Cell(row, 2).Value = c.FechaIngreso?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 3).Value = c.IdProveedorNavigation?.Nombre ?? "N/A";
                    ws.Cell(row, 4).Value = c.IdProveedorNavigation?.NitCi ?? "N/A";
                    ws.Cell(row, 5).Value = c.IdProductoNavigation?.NombreProducto ?? "N/A";
                    ws.Cell(row, 6).Value = c.CantidadComprada;
                    ws.Cell(row, 7).Value = c.CostoCompraUnitario;
                    ws.Cell(row, 7).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                    ws.Cell(row, 8).FormulaA1 = $"F{row}*G{row}";
                    ws.Cell(row, 8).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                    ws.Cell(row, 8).Style.Font.Bold = true;

                    if (row % 2 == 0) ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                    row++;
                }


                var lastRow = row - 1;
                ws.Cell(row, 7).Value = "TOTAL GASTADO";
                ws.Cell(row, 7).Style.Font.Bold = true;
                ws.Cell(row, 8).FormulaA1 = $"SUM(H4:H{lastRow})";
                ws.Cell(row, 8).Style.Font.Bold = true;
                ws.Cell(row, 8).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                ws.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.Black;
                ws.Cell(row, 8).Style.Font.FontColor = XLColor.White;

                ws.Columns().AdjustToContents();
                using (var ms = new MemoryStream()) { workbook.SaveAs(ms); return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Compras.xlsx"); }
            }
        }

        public async Task<IActionResult> ExportarVentaExcel(string periodo = "Mes")
        {
            var fechaInicio = CalcularFechaInicio(periodo);
            var ventas = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.DetalleCotizacionProductos)
                .Where(c => c.TipoOperacion == "Venta" && c.Fecha >= fechaInicio)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Ventas");
                ws.Cell(1, 1).Value = "REPORTE DE VENTAS - STC Services";
                ws.Range(1, 1, 1, 7).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headers = new string[] { "ID", "Fecha", "Cliente", "Ingreso", "Costo Est.", "Margen", "%" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(3, i + 1).Value = headers[i];
                    ws.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.Black;
                    ws.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
                }

                int row = 4;
                foreach (var v in ventas)
                {
                    var costo = v.DetalleCotizacionProductos.Sum(d => d.CostoReferencial);
                    var ingreso = v.TotalConImpuesto ?? 0m;
                    var margen = ingreso - costo;

                    ws.Cell(row, 1).Value = v.IdCotizacion;
                    ws.Cell(row, 2).Value = v.Fecha?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 3).Value = v.IdClienteNavigation?.NombreRazonSocial;
                    ws.Cell(row, 4).Value = ingreso;
                    ws.Cell(row, 5).Value = costo;
                    ws.Cell(row, 6).Value = margen;
                    ws.Cell(row, 7).Value = ingreso > 0 ? (double)(margen / ingreso) : 0;

                    ws.Cell(row, 4).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                    ws.Cell(row, 5).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                    ws.Cell(row, 6).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                    ws.Cell(row, 7).Style.NumberFormat.Format = "0.0%";

                    if (row % 2 == 0) ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                    row++;
                }


                var lastRowV = row - 1;
                ws.Cell(row, 3).Value = "TOTAL VENDIDO";
                ws.Cell(row, 3).Style.Font.Bold = true;

                ws.Cell(row, 4).FormulaA1 = $"SUM(D4:D{lastRowV})";
                ws.Cell(row, 5).FormulaA1 = $"SUM(E4:E{lastRowV})";
                ws.Cell(row, 6).FormulaA1 = $"SUM(F4:F{lastRowV})";

                ws.Range(row, 4, row, 6).Style.Font.Bold = true;
                ws.Range(row, 4, row, 6).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                ws.Range(row, 4, row, 6).Style.Fill.BackgroundColor = XLColor.Black;
                ws.Range(row, 4, row, 6).Style.Font.FontColor = XLColor.White;

                ws.Columns().AdjustToContents();
                using (var ms = new MemoryStream()) { workbook.SaveAs(ms); return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Ventas.xlsx"); }
            }
        }

        public async Task<IActionResult> ExportarStockExcel()
        {
            var productos = await _context.InventarioProductos.ToListAsync();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Stock");
                ws.Cell(1, 1).Value = "ESTADO DE STOCK - STC Services";
                ws.Range(1, 1, 1, 6).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headers = new string[] { "Producto", "Categoría", "Stock Actual", "Costo Pro.", "Precio Venta", "Valor Inventario" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(3, i + 1).Value = headers[i];
                    ws.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.Black;
                    ws.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
                }

                int row = 4;
                foreach (var p in productos)
                {
                    ws.Cell(row, 1).Value = p.NombreProducto;
                    ws.Cell(row, 2).Value = p.Categoria ?? "General";
                    ws.Cell(row, 3).Value = p.Stock ?? 0;
                    ws.Cell(row, 4).Value = p.PrecioCompra;
                    ws.Cell(row, 5).Value = p.PrecioVentaSugerido;
                    ws.Cell(row, 6).FormulaA1 = $"C{row}*D{row}";

                    ws.Cell(row, 4).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                    ws.Cell(row, 5).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                    ws.Cell(row, 6).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";

                    if (row % 2 == 0) ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                    row++;
                }


                var lastRowS = row - 1;
                ws.Cell(row, 5).Value = "VALORIZACIÓN TOTAL";
                ws.Cell(row, 5).Style.Font.Bold = true;
                ws.Cell(row, 6).FormulaA1 = $"SUM(F4:F{lastRowS})";
                ws.Cell(row, 6).Style.Font.Bold = true;
                ws.Cell(row, 6).Style.NumberFormat.Format = "\"Bs.\" #,##0.00";
                ws.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.Black;
                ws.Cell(row, 6).Style.Font.FontColor = XLColor.White;

                ws.Columns().AdjustToContents();
                using (var ms = new MemoryStream()) { workbook.SaveAs(ms); return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Stock.xlsx"); }
            }
        }


        public async Task<IActionResult> ExportarCompraPDF(string periodo = "Mes")
        {
            var fechaInicio = CalcularFechaInicio(periodo);
            var compras = await _context.InventarioCompras
                .Include(c => c.IdProductoNavigation).Include(c => c.IdProveedorNavigation)
                .Where(c => c.FechaIngreso >= fechaInicio).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Element(ComposeHeader);
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("REPORTE DE COMPRAS").FontSize(16).SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });
                            table.Header(h =>
                            {
                                h.Cell().Element(CellStyle).Text("Fecha");
                                h.Cell().Element(CellStyle).Text("Producto");
                                h.Cell().Element(CellStyle).Text("Proveedor");
                                h.Cell().Element(CellStyle).AlignRight().Text("Cant.");
                                h.Cell().Element(CellStyle).AlignRight().Text("Total");
                            });
                            foreach (var c in compras)
                            {
                                table.Cell().Element(ItemStyle).Text(c.FechaIngreso?.ToString("dd/MM/yyyy"));
                                table.Cell().Element(ItemStyle).Text(c.IdProductoNavigation?.NombreProducto);
                                table.Cell().Element(ItemStyle).Text(c.IdProveedorNavigation?.Nombre);
                                table.Cell().Element(ItemStyle).AlignRight().Text(c.CantidadComprada.ToString());
                                table.Cell().Element(ItemStyle).AlignRight().Text(((decimal)c.CantidadComprada * c.CostoCompraUnitario).ToString("'Bs.' #,##0.00"));
                            }
                        });
                        col.Item().PaddingTop(10).AlignRight().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                            t.Cell().Element(CellStyle).Text("TOTAL GASTADO").Bold();
                            t.Cell().Element(CellStyle).AlignRight().Text(compras.Sum(c => (decimal)c.CantidadComprada * c.CostoCompraUnitario).ToString("'Bs.' #,##0.00")).Bold();
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
            });

            using (var stream = new MemoryStream()) { document.GeneratePdf(stream); return File(stream.ToArray(), "application/pdf", "Reporte_Compras.pdf"); }
        }

        public async Task<IActionResult> ExportarVentaPDF(string periodo = "Mes")
        {
            var fechaInicio = CalcularFechaInicio(periodo);
            var ventas = await _context.Cotizaciones.Include(c => c.IdClienteNavigation)
                .Include(c => c.DetalleCotizacionProductos)
                .Where(v => v.TipoOperacion == "Venta" && v.Fecha >= fechaInicio).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Element(ComposeHeader);
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("REPORTE DE VENTAS").FontSize(16).SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn(3);
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });
                            table.Header(h =>
                            {
                                h.Cell().Element(CellStyle).Text("ID");
                                h.Cell().Element(CellStyle).Text("Cliente");
                                h.Cell().Element(CellStyle).AlignRight().Text("Ingreso");
                                h.Cell().Element(CellStyle).AlignRight().Text("Costo");
                                h.Cell().Element(CellStyle).AlignRight().Text("Margen");
                            });
                            foreach (var v in ventas)
                            {
                                var costo = v.DetalleCotizacionProductos.Sum(d => d.CostoReferencial);
                                var ingreso = v.TotalConImpuesto ?? 0m;
                                var margen = ingreso - costo;

                                table.Cell().Element(ItemStyle).Text($"#{v.IdCotizacion}");
                                table.Cell().Element(ItemStyle).Text(v.IdClienteNavigation?.NombreRazonSocial);
                                table.Cell().Element(ItemStyle).AlignRight().Text(ingreso.ToString("'Bs.' #,##0.00"));
                                table.Cell().Element(ItemStyle).AlignRight().Text(costo.ToString("'Bs.' #,##0.00"));
                                table.Cell().Element(ItemStyle).AlignRight().Text(margen.ToString("'Bs.' #,##0.00"));
                            }
                        });
                        col.Item().PaddingTop(10).AlignRight().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });

                            var totalIngreso = ventas.Sum(v => v.TotalConImpuesto ?? 0m);
                            var totalCosto = ventas.Sum(v => v.DetalleCotizacionProductos.Sum(d => d.CostoReferencial));
                            var totalMargen = totalIngreso - totalCosto;

                            t.Cell().Element(CellStyle).Text("TOTAL INGRESOS").Bold();
                            t.Cell().Element(CellStyle).AlignRight().Text(totalIngreso.ToString("'Bs.' #,##0.00")).Bold();

                            t.Cell().Element(CellStyle).Text("COSTO DE VENTAS (EST.)");
                            t.Cell().Element(CellStyle).AlignRight().Text(totalCosto.ToString("'Bs.' #,##0.00"));

                            t.Cell().Element(CellStyle).Text("MARGEN BRUTO");
                            t.Cell().Element(CellStyle).AlignRight().Text(totalMargen.ToString("'Bs.' #,##0.00")).Bold();
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
            });
            using (var stream = new MemoryStream()) { document.GeneratePdf(stream); return File(stream.ToArray(), "application/pdf", "Reporte_Ventas.pdf"); }
        }

        public async Task<IActionResult> ExportarStockPDF()
        {
            var productos = await _context.InventarioProductos.ToListAsync();
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Element(ComposeHeader);
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("REPORTE DE STOCK").FontSize(16).SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols => { cols.RelativeColumn(3); cols.RelativeColumn(2); cols.RelativeColumn(); cols.RelativeColumn(); });
                            table.Header(h =>
                            {
                                h.Cell().Element(CellStyle).Text("Producto");
                                h.Cell().Element(CellStyle).Text("Categoría");
                                h.Cell().Element(CellStyle).AlignRight().Text("Stock");
                                h.Cell().Element(CellStyle).AlignRight().Text("Venta");
                            });
                            foreach (var p in productos)
                            {
                                table.Cell().Element(ItemStyle).Text(p.NombreProducto);
                                table.Cell().Element(ItemStyle).Text(p.Categoria ?? "General");
                                table.Cell().Element(ItemStyle).AlignRight().Text(p.Stock?.ToString() ?? "0");
                                table.Cell().Element(ItemStyle).AlignRight().Text(p.PrecioVentaSugerido.ToString("'Bs.' #,##0.00"));
                            }
                        });

                        col.Item().PaddingTop(10).AlignRight().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });

                            var totalStock = productos.Sum(p => p.Stock ?? 0);
                            var totalValor = productos.Sum(p => (decimal)(p.Stock ?? 0) * p.PrecioCompra);

                            t.Cell().Element(CellStyle).Text("UNIdaDES EN ALMACÉN").Bold();
                            t.Cell().Element(CellStyle).AlignRight().Text(totalStock.ToString()).Bold();

                            t.Cell().Element(CellStyle).Text("VALORIZACIÓN TOTAL");
                            t.Cell().Element(CellStyle).AlignRight().Text(totalValor.ToString("'Bs.' #,##0.00")).Bold();
                        });
                    });
                    page.Footer().Element(ComposeFooter);
                });
            });
            using (var stream = new MemoryStream()) { document.GeneratePdf(stream); return File(stream.ToArray(), "application/pdf", "Reporte_Stock.pdf"); }
        }


        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("STC Services INNOVATE FORWARD").FontSize(22).ExtraBold().FontColor(Colors.Black);
                    col.Item().Text("Gestión Integral de Negocio").FontSize(10).FontColor(Colors.Black);
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(10);
                    col.Item().Text(DateTime.Now.ToString("HH:mm")).FontSize(8).FontColor(Colors.Black);
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Row(row =>
            {
                row.RelativeItem().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }

        static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold().FontSize(10)).PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Black).PaddingRight(5);
        static IContainer ItemStyle(IContainer container) => container.DefaultTextStyle(x => x.FontSize(9)).PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Black).PaddingRight(5);
    }
}
