using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerConfig")]
    public class MetodosPagoController : Controller
    {
        private readonly ErpInventarioContext _context;

        public MetodosPagoController(ErpInventarioContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.MetodosPago.OrderByDescending(m => m.IdMetodo).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Guardar(MetodoPago metodo)
        {
            if (metodo.IdMetodo == 0)
            {
                _context.MetodosPago.Add(metodo);
                TempData["Success"] = "Método de pago creado.";
            }
            else
            {
                _context.MetodosPago.Update(metodo);
                TempData["Success"] = "Método de pago actualizado.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var metodo = await _context.MetodosPago.FindAsync(id);

            if (metodo == null) return NotFound();

            metodo.Estado = !metodo.Estado;
            await _context.SaveChangesAsync();

            var estadoTexto = metodo.Estado ? "activado" : "desactivado";
            TempData["Success"] = $"Método '{metodo.NombreMetodo}' {estadoTexto} correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var metodo = await _context.MetodosPago
                .Include(m => m.Cotizaciones)
                .Include(m => m.InventarioCompras)
                .Include(m => m.HistorialPagos)
                .FirstOrDefaultAsync(m => m.IdMetodo == id);

            if (metodo == null) return NotFound();

            if (metodo.Cotizaciones.Any() || metodo.InventarioCompras.Any() || metodo.HistorialPagos.Any())
            {
                TempData["Error"] = $"No se puede eliminar '{metodo.NombreMetodo}' porque tiene registros asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.MetodosPago.Remove(metodo);
            await _context.SaveChangesAsync();
            TempData["Info"] = $"Método '{metodo.NombreMetodo}' eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
