using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerConfig")]
    public class TiposProductoController : Controller
    {
        private readonly ErpInventarioContext _context;

        public TiposProductoController(ErpInventarioContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tipos = await _context.TiposProducto.OrderByDescending(t => t.IdTipoProducto).ToListAsync();
            return View(tipos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind("Nombre,Descripcion")] TipoProducto tipo)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(tipo.Nombre))
            {
                _context.Add(tipo);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Tipo de Producto '{tipo.Nombre}' creado.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var tipo = await _context.TiposProducto.FindAsync(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("IdTipoProducto,Nombre,Descripcion")] TipoProducto tipo)
        {
            if (id != tipo.IdTipoProducto) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(tipo);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Tipo '{tipo.Nombre}' actualizado.";
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var tipo = await _context.TiposProducto.FindAsync(id);
            if (tipo == null) return NotFound();


            if (await _context.InventarioProductos.AnyAsync(p => p.Categoria == tipo.Nombre))
            {
                TempData["Error"] = $"No se puede eliminar la categoría '{tipo.Nombre}' porque hay productos asignados a ella.";
                return RedirectToAction(nameof(Index));
            }

            _context.TiposProducto.Remove(tipo);
            await _context.SaveChangesAsync();
            TempData["Info"] = $"Tipo '{tipo.Nombre}' eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
