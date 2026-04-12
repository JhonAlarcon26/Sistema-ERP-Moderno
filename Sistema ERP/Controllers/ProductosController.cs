using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerProductos")]
    public class ProductosController : Controller
    {
        private readonly ErpInventarioContext _context;

        public ProductosController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {

            var productos = await _context.InventarioProductos
                .Include(p => p.InventarioCompras)
                    .ThenInclude(c => c.IdProveedorNavigation)
                .OrderByDescending(p => p.IdProducto)
                .ToListAsync();
            ViewBag.ProductosSelectList = new SelectList(productos, "IdProducto", "NombreProducto");


            var proveedores = await _context.Proveedores.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Proveedores = proveedores.Select(p => new
            {
                Value = p.IdProveedor.ToString(),
                Text = p.Nombre,
                NitCi = p.NitCi ?? ""
            }).ToList();


            ViewBag.Categorias = await _context.TiposProducto.OrderBy(t => t.Nombre).ToListAsync();

            return View(productos);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CrearProducto")]
        public async Task<IActionResult> CrearProducto([Bind("NombreProducto,Categoria,PrecioCompra,PrecioInterno,PrecioVentaSugerido,Stock,CodigoSN,Descripcion")] InventarioProducto producto, IFormFile? imagen)
        {
            if (ModelState.IsValid)
            {


                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "productos");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }
                    producto.ImagenUrl = "/uploads/productos/" + fileName;
                }


                if (producto.Stock == null) producto.Stock = 0;

                _context.Add(producto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Producto registrado en el Catálogo exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "No se pudo registrar el producto.";
            return RedirectToAction(nameof(Index));
        }




        [Authorize(Policy = "EditarProducto")]
        public async Task<IActionResult> EditarProducto(int id)
        {
            var producto = await _context.InventarioProductos
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null) return NotFound();

            ViewBag.Categorias = await _context.TiposProducto.OrderBy(t => t.Nombre).ToListAsync();

            return View(producto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditarProducto")]
        public async Task<IActionResult> EditarProducto(int id, [Bind("IdProducto,NombreProducto,Categoria,PrecioCompra,PrecioInterno,PrecioVentaSugerido,Stock,CodigoSN,Descripcion,ImagenUrl")] InventarioProducto producto, IFormFile? imagen)
        {
            if (id != producto.IdProducto) return NotFound();
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.InventarioProductos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);
                if (existingProduct == null) return NotFound();



                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "productos");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }
                    producto.ImagenUrl = "/uploads/productos/" + fileName;
                }
                else
                {
                    producto.ImagenUrl = existingProduct.ImagenUrl;
                }


                if (producto.Stock == null) producto.Stock = 0;

                _context.Update(producto);

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Producto '{producto.NombreProducto}' actualizado.";
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }


        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EliminarProducto")]
        public async Task<IActionResult> EliminarProductoConfirmado(int id)
        {
            var producto = await _context.InventarioProductos
                .Include(p => p.DetalleCotizacionProductos)
                .Include(p => p.InventarioCompras)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null) return NotFound();


            if (producto.DetalleCotizacionProductos.Any() || producto.InventarioCompras.Any())
            {
                TempData["Error"] = $"No se puede eliminar el producto '{producto.NombreProducto}' porque ya tiene registros asociados en Compras o Ventas/Cotizaciones. Para mantener la integridad del historial, el producto debe permanecer en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            _context.InventarioProductos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["Info"] = "Producto eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
