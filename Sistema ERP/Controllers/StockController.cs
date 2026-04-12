using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerStock")]
    public class StockController : Controller
    {
        private readonly ErpInventarioContext _context;

        public StockController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {

            var inventarioResume = await _context.InventarioProductos
                .Select(p => new InventarioResumeViewModel
                {
                    IdProducto = p.IdProducto,
                    NombreProducto = p.NombreProducto,
                    StockActual = p.Stock ?? 0,

                    CostoCompraPromedio = p.InventarioCompras.Sum(c => c.CantidadComprada) > 0
                        ? p.InventarioCompras.Sum(c => c.CantidadComprada * c.CostoCompraUnitario) /
                          p.InventarioCompras.Sum(c => (decimal)c.CantidadComprada)
                        : p.PrecioCompra,

                    ValorTotalStock = (decimal)(p.Stock ?? 0) * (
                        p.InventarioCompras.Sum(c => c.CantidadComprada) > 0
                        ? p.InventarioCompras.Sum(c => c.CantidadComprada * c.CostoCompraUnitario) /
                          p.InventarioCompras.Sum(c => (decimal)c.CantidadComprada)
                        : p.PrecioCompra
                    )
                })
                .OrderByDescending(x => x.IdProducto)
                .ToListAsync();

            return View(inventarioResume);
        }
    }
}
