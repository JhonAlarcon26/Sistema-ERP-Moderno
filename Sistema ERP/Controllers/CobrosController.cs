using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize]
    public class CobrosController : Controller
    {
        private readonly ErpInventarioContext _context;

        public CobrosController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var cobros = await _context.CobrosPendientes
                .Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago >= DateTime.Now)
                .Include(c => c.IdOperacionNavigation)
                    .ThenInclude(op => op.IdClienteNavigation)
                .OrderByDescending(c => c.IdCobro)
                .ToListAsync();


            ViewBag.TotalVencido = await _context.CobrosPendientes
                .Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago < DateTime.Now)
                .SumAsync(c => c.MontoPendiente);

            return View(cobros);
        }


        public async Task<IActionResult> Vencidos()
        {
            var cobrosVencidos = await _context.CobrosPendientes
                .Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago < DateTime.Now)
                .Include(c => c.IdOperacionNavigation)
                    .ThenInclude(op => op.IdClienteNavigation)
                .OrderByDescending(c => c.IdCobro)
                .ToListAsync();

            return View(cobrosVencidos);
        }


        public async Task<IActionResult> Historial()
        {
            var cobrosLiquidacion = await _context.CobrosPendientes
                .Where(c => c.EstadoCobro == "Cobrado")
                .Include(c => c.IdOperacionNavigation)
                    .ThenInclude(op => op.IdClienteNavigation)
                .Include(c => c.HistorialPagos)
                .OrderByDescending(c => c.IdCobro)
                .ToListAsync();

            return View(cobrosLiquidacion);
        }


        public async Task<IActionResult> DetalleDeuda(int id, string modo = "historial")
        {
            ViewBag.Modo = modo;
            var cobro = await _context.CobrosPendientes
                .Include(c => c.IdOperacionNavigation)
                    .ThenInclude(op => op.IdClienteNavigation)
                .Include(c => c.HistorialPagos)
                    .ThenInclude(h => h.IdMetodoPagoNavigation)
                .FirstOrDefaultAsync(c => c.IdCobro == id);

            if (cobro == null) return NotFound();

            ViewBag.MetodosPago = await _context.MetodosPago.OrderBy(m => m.NombreMetodo).ToListAsync();

            return PartialView("_DetalleCobro", cobro);
        }

        public class AbonoDto
        {
            public int IdCobro { get; set; }
            public decimal MontoAbono { get; set; }
            public int? IdMetodoPago { get; set; }
            public string Comprobante { get; set; } = "";
        }

        [HttpPost]
        public async Task<IActionResult> AbonarAPI([FromBody] AbonoDto dto)
        {
            try
            {
                if (dto.MontoAbono <= 0) return BadRequest("El monto del abono debe ser mayor a 0");
                if (dto.IdMetodoPago == null || dto.IdMetodoPago <= 0) return BadRequest("Debe seleccionar un método de pago.");

                var cobro = await _context.CobrosPendientes.FindAsync(dto.IdCobro);
                if (cobro == null) return NotFound("Cuenta pendiente no encontrada.");

                if (cobro.EstadoCobro == "Cobrado") return BadRequest("Esta deuda ya fue liquidada.");


                if (dto.MontoAbono > cobro.MontoPendiente)
                    return Ok(new { success = false, message = $"El monto ingresado (Bs. {dto.MontoAbono:N2}) supera el saldo pendiente (Bs. {cobro.MontoPendiente:N2}). No se pueden registrar pagos en exceso." });


                cobro.MontoPagado += dto.MontoAbono;
                cobro.MontoPendiente = cobro.MontoTotal - cobro.MontoPagado;


                var historial = new HistorialPago
                {
                    IdCobro = cobro.IdCobro,
                    MontoAbonado = dto.MontoAbono,
                    FechaPago = DateTime.Now,
                    IdMetodoPago = dto.IdMetodoPago,
                    Comprobante = dto.Comprobante
                };
                _context.HistorialPagos.Add(historial);


                var venta = await _context.Cotizaciones.FindAsync(cobro.IdOperacion);
                if (venta != null)
                {

                    venta.IdMetodoPago = dto.IdMetodoPago;
                }


                if (cobro.MontoPendiente <= 0)
                {
                    cobro.MontoPendiente = 0;
                    cobro.EstadoCobro = "Cobrado";
                    cobro.NotasAdicionales = "Deuda liquidada el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                    if (venta != null && venta.EstadoPago != "Pagado")
                    {
                        venta.EstadoPago = "Pagado";
                        venta.Estado = "Realizado";
                    }
                }
                else
                {

                    cobro.EstadoCobro = "Abono Parcial";
                }

                if (venta != null)
                {
                    _context.Cotizaciones.Update(venta);
                }

                _context.CobrosPendientes.Update(cobro);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, Message = "Abono registrado correctamente", NuevoSaldo = cobro.MontoPendiente, EstadoActual = cobro.EstadoCobro });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosRecibo(int id)
        {
            var cobro = await _context.CobrosPendientes
                .Include(c => c.IdOperacionNavigation)
                    .ThenInclude(v => v.IdClienteNavigation)
                .Include(c => c.HistorialPagos)
                    .ThenInclude(h => h.IdMetodoPagoNavigation)
                .FirstOrDefaultAsync(c => c.IdCobro == id);

            if (cobro == null) return NotFound("Cobro no encontrado");

            var result = new
            {
                success = true,
                cliente = cobro.IdOperacionNavigation.IdClienteNavigation.NombreRazonSocial,
                operacion = cobro.IdOperacionNavigation.NroCorrelativo,
                fechaRegistro = cobro.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                montoTotal = cobro.MontoTotal,
                montoPagado = cobro.MontoPagado,
                montoPendiente = cobro.MontoPendiente,
                estado = cobro.EstadoCobro,
                historial = cobro.HistorialPagos.OrderByDescending(p => p.FechaPago).Select(p => new
                {
                    fecha = p.FechaPago.ToString("dd/MM/yyyy HH:mm"),
                    monto = p.MontoAbonado,
                    metodo = p.IdMetodoPagoNavigation?.NombreMetodo ?? p.MetodoPago ?? "Efectivo"
                })
            };

            return Json(result);
        }
    }
}
