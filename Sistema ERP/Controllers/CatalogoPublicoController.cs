using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{


    [Authorize(Policy = "VerCatalogo")]
    public class CatalogoPublicoController : Controller
    {
        private readonly ErpInventarioContext _context;

        public CatalogoPublicoController(ErpInventarioContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new CatalogosViewModel
            {
                Productos = await _context.InventarioProductos.ToListAsync(),
                Servicios = await _context.InventarioServicios.ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Presentacion()
        {
            var viewModel = new CatalogosViewModel
            {
                Productos = await _context.InventarioProductos.ToListAsync(),
                Servicios = await _context.InventarioServicios.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerarEnlace()
        {
            var token = Guid.NewGuid().ToString();
            var enlace = new EnlaceCompartido
            {
                Token = token,
                FechaCreacion = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddDays(7),
                EstaActivo = true
            };

            _context.EnlacesCompartidos.Add(enlace);
            await _context.SaveChangesAsync();

            var host = Request.Host.Value;
            var protocol = Request.Scheme;
            var url = $"{protocol}://{host}/CatalogoPublico/v/{token}";

            return Json(new { success = true, url = url });
        }

        [AllowAnonymous]
        [Route("CatalogoPublico/v/{token}")]
        public async Task<IActionResult> VistaPublica(string token)
        {
            var enlace = await _context.EnlacesCompartidos
                .FirstOrDefaultAsync(e => e.Token == token && e.EstaActivo);

            if (enlace == null || (enlace.FechaExpiracion.HasValue && enlace.FechaExpiracion < DateTime.Now))
            {
                return NotFound("Este enlace ha expirado o no es válido.");
            }

            var viewModel = new CatalogosViewModel
            {
                Productos = await _context.InventarioProductos.ToListAsync(),
                Servicios = await _context.InventarioServicios.ToListAsync()
            };

            return View(viewModel);
        }
    }
}
