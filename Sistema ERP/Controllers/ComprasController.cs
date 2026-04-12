using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    public class CompraItemDTO
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal PrecioRef { get; set; }
        public decimal PrecioVenta { get; set; }
    }

    public class CompraRequestDTO
    {
        public int? IdProveedor { get; set; }
        public int? IdMetodoPago { get; set; }
        public List<CompraItemDTO> Detalles { get; set; } = new List<CompraItemDTO>();
    }

    public class CompraAgrupadaVM
    {
        public DateTime? Fecha { get; set; }
        public string ProveedorNombre { get; set; }
        public string ProveedorNit { get; set; }
        public string UsuarioNombre { get; set; }
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }

        public string DetallesJson { get; set; }

        public int IdReferencia { get; set; }
    }

    [Authorize(Policy = "VerCompras")]
    public class ComprasController : Controller
    {
        private readonly ErpInventarioContext _context;

        public ComprasController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var todasLasCompras = await _context.InventarioCompras
                .Include(i => i.IdProductoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdProveedorNavigation)
                .Include(i => i.IdMetodoPagoNavigation)
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();


            var agrupadas = todasLasCompras
                .GroupBy(c => new
                {
                    Fecha = c.FechaIngreso,
                    ProvId = c.IdProveedor,
                    UserId = c.IdUsuario
                })
                .Select(g =>
                {
                    var primer = g.First();
                    var detalles = g.Select(d => new
                    {
                        producto = d.IdProductoNavigation?.NombreProducto,
                        cantidad = d.CantidadComprada,
                        costo = d.CostoCompraUnitario,
                        precioRef = d.PrecioRefHistorico ?? (d.IdProductoNavigation?.PrecioInterno ?? 0),
                        precioVenta = d.PrecioVentaHistorico ?? (d.IdProductoNavigation?.PrecioVentaSugerido ?? 0),
                        subtotal = d.CantidadComprada * d.CostoCompraUnitario
                    }).ToList();

                    return new CompraAgrupadaVM
                    {
                        Fecha = g.Key.Fecha,
                        ProveedorNombre = primer.IdProveedorNavigation?.Nombre ?? "Genérico",
                        ProveedorNit = primer.IdProveedorNavigation?.NitCi ?? "",
                        UsuarioNombre = primer.IdUsuarioNavigation?.NombreCompleto ?? "Sistema",
                        MetodoPago = primer.IdMetodoPagoNavigation?.NombreMetodo ?? "N/A",
                        Total = g.Sum(x => x.CantidadComprada * x.CostoCompraUnitario),
                        CantidadItems = g.Count(),
                        DetallesJson = System.Text.Json.JsonSerializer.Serialize(detalles),
                        IdReferencia = primer.IdIngreso
                    };
                })
                .ToList();

            return View(agrupadas);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EliminarCompra")]
        public async Task<IActionResult> EliminarCompra(int id)
        {
            var compraRef = await _context.InventarioCompras
                .FirstOrDefaultAsync(i => i.IdIngreso == id);

            if (compraRef == null) return NotFound();


            var lote = await _context.InventarioCompras
                .Where(c => c.FechaIngreso == compraRef.FechaIngreso &&
                            c.IdProveedor == compraRef.IdProveedor &&
                            c.IdUsuario == compraRef.IdUsuario)
                .ToListAsync();

            foreach (var item in lote)
            {

                var producto = await _context.InventarioProductos.FindAsync(item.IdProducto);
                if (producto != null)
                {
                    producto.Stock = (producto.Stock ?? 0) - item.CantidadComprada;
                    if (producto.Stock < 0) producto.Stock = 0;
                    _context.Update(producto);
                }
                _context.InventarioCompras.Remove(item);
            }

            await _context.SaveChangesAsync();
            TempData["Info"] = $"Compra ({lote.Count} productos) eliminada y stock ajustado.";

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> NuevaCompra()
        {

            ViewBag.Proveedores = await _context.Proveedores
                .OrderBy(p => p.Nombre)
                .Select(p => new { Value = p.IdProveedor, Text = p.Nombre, NitCi = p.NitCi })
                .ToListAsync();

            ViewBag.MetodosPago = await _context.MetodosPago
                .Where(m => m.Estado)
                .OrderBy(m => m.NombreMetodo)
                .ToListAsync();


            var productos = await _context.InventarioProductos
                .OrderBy(p => p.NombreProducto)
                .Select(p => new
                {
                    id = p.IdProducto,
                    nombre = p.NombreProducto,
                    stock = p.Stock,
                    precioCompra = p.PrecioCompra,
                    precioRef = p.PrecioInterno,
                    precioVenta = p.PrecioVentaSugerido,
                    categoria = p.Categoria
                }).ToListAsync();

            ViewBag.ProductosJson = System.Text.Json.JsonSerializer.Serialize(productos);

            return View();
        }


        [HttpPost]
        [Authorize(Policy = "CrearCompra")]
        public async Task<IActionResult> ConfirmarCompra([FromBody] CompraRequestDTO request)
        {
            if (request == null || request.Detalles == null || !request.Detalles.Any())
            {
                return BadRequest("El carrito de compras está vacío.");
            }

            if (request.IdProveedor == null)
            {
                return BadRequest("El proveedor es obligatorio.");
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            int? userId = int.TryParse(userIdClaim, out int uid) ? uid : null;
            DateTime fechaActual = DateTime.Now;


            foreach (var item in request.Detalles)
            {
                if (item.Cantidad <= 0 || item.CostoUnitario < 0) continue;

                var producto = await _context.InventarioProductos.FindAsync(item.IdProducto);
                if (producto == null) continue;


                producto.Stock = (producto.Stock ?? 0) + item.Cantidad;
                producto.PrecioCompra = item.CostoUnitario;
                producto.PrecioInterno = item.PrecioRef;
                producto.PrecioVentaSugerido = item.PrecioVenta;

                _context.Update(producto);


                var nuevaCompra = new InventarioCompra
                {
                    IdProducto = item.IdProducto,
                    CantidadComprada = item.Cantidad,
                    CostoCompraUnitario = item.CostoUnitario,
                    FechaIngreso = fechaActual,
                    IdUsuario = userId,
                    IdProveedor = request.IdProveedor,
                    IdMetodoPago = request.IdMetodoPago,
                    PrecioRefHistorico = item.PrecioRef,
                    PrecioVentaHistorico = item.PrecioVenta
                };
                _context.InventarioCompras.Add(nuevaCompra);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Compra registrada exitosamente." });
        }
    }
}
