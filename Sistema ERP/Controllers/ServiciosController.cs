using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerServicios")]
    public class ServiciosController : Controller
    {
        private readonly ErpInventarioContext _context;

        public ServiciosController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var servicios = await _context.InventarioServicios.OrderByDescending(s => s.IdServicio).ToListAsync();
            ViewBag.Categorias = await _context.TiposServicio.OrderBy(t => t.Nombre).ToListAsync();
            return View(servicios);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CrearServicio")]
        public async Task<IActionResult> CrearServicio([Bind("NombreServicio,Categoria,PrecioVenta,Descripcion")] InventarioServicio servicio, IFormFile? imagen)
        {
            if (ModelState.IsValid)
            {
                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "servicios");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }
                    servicio.ImagenUrl = "/uploads/servicios/" + fileName;
                }

                _context.Add(servicio);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Servicio creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "Error al crear el servicio.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Policy = "EditarServicio")]
        public async Task<IActionResult> EditarServicio(int id)
        {
            var servicio = await _context.InventarioServicios.FindAsync(id);
            if (servicio == null) return NotFound();

            ViewBag.Categorias = await _context.TiposServicio.OrderBy(t => t.Nombre).ToListAsync();

            return View(servicio);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditarServicio")]
        public async Task<IActionResult> EditarServicio(int id, [Bind("IdServicio,NombreServicio,Categoria,PrecioVenta,Descripcion,ImagenUrl")] InventarioServicio servicio, IFormFile? imagen)
        {
            if (id != servicio.IdServicio) return NotFound();
            if (ModelState.IsValid)
            {
                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "servicios");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }
                    servicio.ImagenUrl = "/uploads/servicios/" + fileName;
                }
                else
                {

                    var existingServicio = await _context.InventarioServicios.AsNoTracking().FirstOrDefaultAsync(s => s.IdServicio == id);
                    if (existingServicio != null)
                    {
                        servicio.ImagenUrl = existingServicio.ImagenUrl;
                    }
                }

                _context.Update(servicio);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Servicio '{servicio.NombreServicio}' actualizado.";
                return RedirectToAction(nameof(Index));
            }
            return View(servicio);
        }


        [HttpPost, ActionName("EliminarServicio")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EliminarServicio")]
        public async Task<IActionResult> EliminarServicioConfirmado(int id)
        {
            var servicio = await _context.InventarioServicios
                .Include(s => s.DetalleCotizacionServicios)
                .FirstOrDefaultAsync(s => s.IdServicio == id);

            if (servicio == null) return NotFound();


            if (servicio.DetalleCotizacionServicios.Any())
            {
                TempData["Error"] = $"No se puede eliminar el servicio '{servicio.NombreServicio}' porque ya ha sido incluido en cotizaciones o ventas registradas.";
                return RedirectToAction(nameof(Index));
            }

            _context.InventarioServicios.Remove(servicio);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Servicio eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
