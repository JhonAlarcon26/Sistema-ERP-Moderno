using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize]
    [Authorize(Policy = "VerProveedores")]
    public class ProveedoresController : Controller
    {
        private readonly ErpInventarioContext _context;

        public ProveedoresController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var proveedores = await _context.Proveedores.OrderByDescending(p => p.IdProveedor).ToListAsync();
            return View(proveedores);
        }


        [Authorize(Policy = "CrearProveedor")]
        public async Task<IActionResult> Crear()
        {
            var googleConfig = await _context.ConfiguracionesApi.FirstOrDefaultAsync(a => a.Proveedor == "GoogleMaps");
            ViewBag.GoogleApiKey = googleConfig?.ApiKey ?? "";
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CrearProveedor")]
        public async Task<IActionResult> Crear([Bind("IdProveedor,Nombre,NitCi,Telefono,Direccion,TipoProveedor,Latitud,Longitud")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Proveedor '{proveedor.Nombre}' creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "No se pudo crear el proveedor. Verifique los Datos.";

            var googleConfig = await _context.ConfiguracionesApi.FirstOrDefaultAsync(a => a.Proveedor == "GoogleMaps");
            ViewBag.GoogleApiKey = googleConfig?.ApiKey ?? "";
            return View(proveedor);
        }


        [Authorize(Policy = "EditarProveedor")]
        public async Task<IActionResult> Editar(int? id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();
            var googleConfig = await _context.ConfiguracionesApi.FirstOrDefaultAsync(a => a.Proveedor == "GoogleMaps");
            ViewBag.GoogleApiKey = googleConfig?.ApiKey ?? "";
            return View(proveedor);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditarProveedor")]
        public async Task<IActionResult> Editar(int id, [Bind("IdProveedor,Nombre,NitCi,Telefono,Direccion,TipoProveedor,Latitud,Longitud")] Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(proveedor);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Proveedor '{proveedor.Nombre}' actualizado.";
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }


        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EliminarProveedor")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var proveedor = await _context.Proveedores
                .Include(p => p.InventarioCompras)
                .FirstOrDefaultAsync(p => p.IdProveedor == id);

            if (proveedor == null) return NotFound();


            if (proveedor.InventarioCompras.Any())
            {
                TempData["Error"] = $"No se puede eliminar al proveedor '{proveedor.Nombre}' porque ya tiene facturas de compra registradas a su nombre.";
                return RedirectToAction(nameof(Index));
            }

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            TempData["Info"] = $"Proveedor '{proveedor.Nombre}' eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> GetJson(int id)
        {
            var p = await _context.Proveedores.FindAsync(id);
            if (p == null) return NotFound();
            return Json(new { nitCi = p.NitCi ?? "", nombre = p.Nombre });
        }
    }
}
