using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerVentas")]
    public class CotizacionesController : Controller
    {
        private readonly ErpInventarioContext _context;

        public CotizacionesController(ErpInventarioContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdMetodoPagoNavigation)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
            return View(cotizaciones);
        }

        public async Task<IActionResult> Nueva(int? idCliente)
        {
            var viewModel = new NuevaCotizacionViewModel
            {
                Clientes = await _context.Clientes.ToListAsync()
            };

            if (idCliente.HasValue)
            {
                ViewBag.IdClientePreseleccionado = idCliente.Value;
            }

            ViewBag.Productos = await _context.InventarioProductos
                .Select(p => new { p.IdProducto, p.NombreProducto, p.PrecioVentaSugerido, p.PrecioInterno, p.Stock })
                .ToListAsync();
            ViewBag.Servicios = await _context.InventarioServicios
                .Select(s => new { s.IdServicio, s.NombreServicio, PrecioVenta = s.PrecioVenta })
                .ToListAsync();
            ViewBag.MetodosPago = await _context.MetodosPago.Where(m => m.Estado).ToListAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.DetalleCotizacionProductos)
                .Include(c => c.DetalleCotizacionServicios)
                .FirstOrDefaultAsync(c => c.IdCotizacion == id);

            if (cotizacion == null || (cotizacion.Estado != "Cotizado" && cotizacion.Estado != "Borrador"))
                return RedirectToAction(nameof(Index));

            var viewModel = new NuevaCotizacionViewModel
            {
                Clientes = await _context.Clientes.ToListAsync()
            };

            ViewBag.Productos = await _context.InventarioProductos
                .Select(p => new { p.IdProducto, p.NombreProducto, p.PrecioVentaSugerido, p.PrecioInterno, p.Stock })
                .ToListAsync();
            ViewBag.Servicios = await _context.InventarioServicios
                .Select(s => new { s.IdServicio, s.NombreServicio, PrecioVenta = s.PrecioVenta })
                .ToListAsync();
            ViewBag.MetodosPago = await _context.MetodosPago.Where(m => m.Estado).ToListAsync();
            ViewBag.CotizacionEdit = cotizacion;

            return View("Nueva", viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "CrearVenta")]
        public async Task<IActionResult> GuardarAPI([FromBody] GuardarCotizacionDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (dto == null) return BadRequest("Datos nulos recibidos en el servidor");
                if (dto.IdCliente == 0) return BadRequest("Debe seleccionar un cliente antes de guardar");

                decimal subtotalControl = dto.Subtotal;
                decimal montoImpuestoCalc = subtotalControl * (dto.PorcentajeImpuesto / 100m);
                decimal totalConImpuestoCalc = subtotalControl + montoImpuestoCalc;

                string prefix = dto.TipoOperacion == "Venta" ? "VEN-" : "COT-";


                var maxCorrelativoStr = await _context.Cotizaciones
                    .Where(c => c.NroCorrelativo.StartsWith(prefix))
                    .OrderByDescending(c => c.NroCorrelativo)
                    .Select(c => c.NroCorrelativo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (!string.IsNullOrEmpty(maxCorrelativoStr))
                {
                    string numberPart = maxCorrelativoStr.Substring(prefix.Length);
                    if (int.TryParse(numberPart, out int maxNumber))
                    {
                        nextNumber = maxNumber + 1;
                    }
                }

                string newCorrelativo = prefix + nextNumber.ToString("D5");
                string estadoVenta = "Cotizado";
                if (dto.TipoOperacion == "Venta")
                {
                    estadoVenta = dto.EstadoPago == "Pagado" ? "Realizado" : "Pendiente";
                }

                Cotizacione cotizacion;
                bool esNueva = true;

                if (dto.IdCotizacion > 0)
                {
                    cotizacion = await _context.Cotizaciones
                        .Include(c => c.DetalleCotizacionProductos)
                        .Include(c => c.DetalleCotizacionServicios)
                        .FirstOrDefaultAsync(c => c.IdCotizacion == dto.IdCotizacion);

                    if (cotizacion != null)
                    {
                        esNueva = false;

                        _context.DetalleCotizacionProductos.RemoveRange(cotizacion.DetalleCotizacionProductos);
                        _context.DetalleCotizacionServicios.RemoveRange(cotizacion.DetalleCotizacionServicios);


                        var cobrosAntiguos = await _context.CobrosPendientes.Where(c => c.IdOperacion == cotizacion.IdCotizacion).ToListAsync();
                        if (cobrosAntiguos.Any())
                        {
                            _context.CobrosPendientes.RemoveRange(cobrosAntiguos);
                        }
                    }
                    else { cotizacion = new Cotizacione(); }
                }
                else { cotizacion = new Cotizacione(); }

                cotizacion.IdCliente = dto.IdCliente;
                cotizacion.Estado = estadoVenta;
                cotizacion.TipoOperacion = dto.TipoOperacion;
                cotizacion.EstadoPago = dto.EstadoPago;
                cotizacion.PorcentajeImpuesto = dto.PorcentajeImpuesto;
                cotizacion.MontoImpuesto = montoImpuestoCalc;
                cotizacion.TotalConImpuesto = totalConImpuestoCalc;
                cotizacion.IdMetodoPago = dto.IdMetodoPago;

                if (esNueva)
                {
                    cotizacion.Fecha = DateTime.Now;
                    cotizacion.NroCorrelativo = newCorrelativo;
                    _context.Cotizaciones.Add(cotizacion);
                }
                else
                {

                    if (string.IsNullOrEmpty(cotizacion.NroCorrelativo) || !cotizacion.NroCorrelativo.StartsWith(prefix))
                    {
                        cotizacion.NroCorrelativo = newCorrelativo;
                    }
                    _context.Cotizaciones.Update(cotizacion);
                }

                await _context.SaveChangesAsync();

                foreach (var det in dto.DetallesProductos)
                {
                    var productoCat = await _context.InventarioProductos.FindAsync(det.IdItem);
                    if (productoCat == null) continue;

                    if ((productoCat.Stock ?? 0) < det.Cantidad)
                    {
                        return Json(new { success = false, message = $"El producto '{productoCat.NombreProducto}' no tiene stock suficiente (Disponible: {productoCat.Stock ?? 0}, Solicitado: {det.Cantidad})." });
                    }
                }

                foreach (var det in dto.DetallesProductos)
                {
                    var productoCat = await _context.InventarioProductos.FindAsync(det.IdItem);
                    if (productoCat == null) continue;

                    var inventarioStats = await _context.InventarioCompras
                                            .Where(i => i.IdProducto == det.IdItem)
                                            .ToListAsync();

                    decimal costoReferencial = 0;
                    int cantTotal = inventarioStats.Sum(i => i.CantidadComprada);
                    if (cantTotal > 0)
                    {
                        costoReferencial = inventarioStats.Sum(i => i.CantidadComprada * i.CostoCompraUnitario) / (decimal)cantTotal;
                    }

                    var detalle = new DetalleCotizacionProducto
                    {
                        IdCotizacion = cotizacion.IdCotizacion,
                        IdProducto = det.IdItem,
                        Cantidad = det.Cantidad,
                        PrecioVendido = det.PrecioBase,
                        CostoReferencial = costoReferencial
                    };
                    _context.DetalleCotizacionProductos.Add(detalle);

                    if (dto.TipoOperacion == "Venta")
                    {
                        if (productoCat.Stock != null)
                        {
                            productoCat.Stock -= det.Cantidad;
                            _context.InventarioProductos.Update(productoCat);
                        }
                    }
                }

                foreach (var det in dto.DetallesServicios)
                {
                    var servicioCat = await _context.InventarioServicios.FindAsync(det.IdItem);
                    if (servicioCat == null) continue;

                    var detalle = new DetalleCotizacionServicio
                    {
                        IdCotizacion = cotizacion.IdCotizacion,
                        IdServicio = det.IdItem,
                        Cantidad = det.Cantidad,
                        PrecioCobrado = det.PrecioBase
                    };
                    _context.DetalleCotizacionServicios.Add(detalle);
                }

                if (dto.TipoOperacion == "Venta" && dto.EstadoPago == "Por Cobrar")
                {
                    if (!dto.FechaLimitePago.HasValue)
                    {
                        return Json(new { success = false, message = "Debe proporcionar una fecha límite de pago para ventas por cobrar." });
                    }

                    var cobro = new CobroPendiente
                    {
                        IdOperacion = cotizacion.IdCotizacion,
                        MontoTotal = totalConImpuestoCalc,
                        MontoPagado = 0,
                        MontoPendiente = totalConImpuestoCalc,
                        FechaLimitePago = dto.FechaLimitePago.Value,
                        FechaRegistro = DateTime.Now,
                        EstadoCobro = "Pendiente"
                    };
                    _context.CobrosPendientes.Add(cobro);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMsg = ex.InnerException != null ? " | " + ex.InnerException.Message : "";
                return Json(new { success = false, message = ex.Message + innerMsg });
            }
        }

        [HttpPost]
        [Authorize(Policy = "EliminarVenta")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var cotizacion = await _context.Cotizaciones
                    .Include(c => c.DetalleCotizacionProductos)
                    .Include(c => c.DetalleCotizacionServicios)
                    .Include(c => c.VisitasTecnicas)
                    .FirstOrDefaultAsync(c => c.IdCotizacion == id);

                if (cotizacion == null)
                {
                    TempData["Info"] = "La operación ya no existe o fue eliminada previamente.";
                    return RedirectToAction(nameof(Index));
                }

                var cobros = await _context.CobrosPendientes
                    .Include(c => c.HistorialPagos)
                    .Where(c => c.IdOperacion == id)
                    .ToListAsync();

                foreach (var cobro in cobros)
                {
                    _context.HistorialPagos.RemoveRange(cobro.HistorialPagos);
                }
                _context.CobrosPendientes.RemoveRange(cobros);

                if (cotizacion.VisitasTecnicas != null && cotizacion.VisitasTecnicas.Any())
                {
                    _context.VisitasTecnicas.RemoveRange(cotizacion.VisitasTecnicas);
                }

                _context.DetalleCotizacionProductos.RemoveRange(cotizacion.DetalleCotizacionProductos);
                _context.DetalleCotizacionServicios.RemoveRange(cotizacion.DetalleCotizacionServicios);

                _context.Cotizaciones.Remove(cotizacion);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Operación eliminada correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Info"] = "La operación ya había sido eliminada por otro usuario o proceso.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al intentar eliminar: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "CrearCliente")]
        public async Task<IActionResult> CrearClienteAPI([FromBody] ClienteDTO clienteDto)
        {
            if (clienteDto == null || string.IsNullOrEmpty(clienteDto.NombreRazonSocial))
                return Json(new { success = false, message = "Datos incompletos" });

            var cliente = new Cliente
            {
                Tipo = clienteDto.Tipo,
                NombreRazonSocial = clienteDto.NombreRazonSocial,
                NitCi = clienteDto.NitCi,
                Telefono = clienteDto.Telefono,
                Direccion = clienteDto.Direccion
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return Json(new { success = true, cliente = cliente });
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var cot = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdMetodoPagoNavigation)
                .Include(c => c.DetalleCotizacionProductos).ThenInclude(d => d.IdProductoNavigation)
                .Include(c => c.DetalleCotizacionServicios).ThenInclude(d => d.IdServicioNavigation)
                .FirstOrDefaultAsync(c => c.IdCotizacion == id);

            if (cot == null) return NotFound();

            return PartialView("_DetalleOperacion", cot);
        }

        public async Task<IActionResult> DetalleJSON(int id)
        {
            var cot = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdMetodoPagoNavigation)
                .Include(c => c.DetalleCotizacionProductos).ThenInclude(d => d.IdProductoNavigation)
                .Include(c => c.DetalleCotizacionServicios).ThenInclude(d => d.IdServicioNavigation)
                .FirstOrDefaultAsync(c => c.IdCotizacion == id);

            if (cot == null) return NotFound();

            var resultado = new
            {
                nro = cot.NroCorrelativo ?? "#" + cot.IdCotizacion.ToString("D5"),
                tipo = cot.TipoOperacion,
                fecha = cot.Fecha?.ToString("dd/MM/yyyy HH:mm"),
                cliente = cot.IdClienteNavigation?.NombreRazonSocial ?? "Sin Cliente",
                nit = cot.IdClienteNavigation?.NitCi ?? "---",
                direccion = cot.IdClienteNavigation?.Direccion ?? "",
                metodo = cot.IdMetodoPagoNavigation?.NombreMetodo ?? "No especificado",
                estado = cot.Estado,
                subtotal = (cot.TotalConImpuesto - (cot.MontoImpuesto ?? 0m))?.ToString("N2"),
                ivaPorcentaje = cot.PorcentajeImpuesto,
                ivaMonto = cot.MontoImpuesto?.ToString("N2"),
                total = cot.TotalConImpuesto?.ToString("N2"),
                detalles = cot.DetalleCotizacionProductos.Select(p => new
                {
                    nombre = p.IdProductoNavigation?.NombreProducto ?? "Producto",
                    cant = p.Cantidad,
                    precio = p.PrecioVendido.ToString("N2"),
                    precioRef = p.CostoReferencial.ToString("N2"),
                    subtotal = (p.Cantidad * p.PrecioVendido).ToString("N2")
                }).Concat(cot.DetalleCotizacionServicios.Select(s => new
                {
                    nombre = s.IdServicioNavigation?.NombreServicio ?? "Servicio",
                    cant = s.Cantidad,
                    precio = s.PrecioCobrado.ToString("N2"),
                    precioRef = "---",
                    subtotal = (s.Cantidad * s.PrecioCobrado).ToString("N2")
                }))
            };

            return Json(resultado);
        }
    }
}
