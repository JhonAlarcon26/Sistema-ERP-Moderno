using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerClientes")]
    public class ClientesController : Controller
    {
        private readonly ErpInventarioContext _context;

        public ClientesController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes.OrderByDescending(c => c.IdCliente).ToListAsync();
            return View(clientes);
        }


        [Authorize(Policy = "CrearCliente")]
        public async Task<IActionResult> Crear()
        {
            var googleConfig = await _context.ConfiguracionesApi.FirstOrDefaultAsync(a => a.Proveedor == "GoogleMaps");
            ViewBag.GoogleApiKey = googleConfig?.ApiKey ?? "";
            return View(new Cliente());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CrearCliente")]
        public async Task<IActionResult> Crear([Bind("Tipo,NombreRazonSocial,NitCi,Telefono,Direccion,Latitud,Longitud")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cliente '{cliente.NombreRazonSocial}' creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }


        [Authorize(Policy = "EditarCliente")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            var googleConfig = await _context.ConfiguracionesApi.FirstOrDefaultAsync(a => a.Proveedor == "GoogleMaps");
            ViewBag.GoogleApiKey = googleConfig?.ApiKey ?? "";

            return View(cliente);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditarCliente")]
        public async Task<IActionResult> Editar(int id, [Bind("IdCliente,Tipo,NombreRazonSocial,NitCi,Telefono,Direccion,Latitud,Longitud")] Cliente cliente)
        {
            if (id != cliente.IdCliente) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Cliente '{cliente.NombreRazonSocial}' actualizado.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Clientes.AnyAsync(c => c.IdCliente == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EliminarCliente")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Cotizaciones)
                .Include(c => c.VisitasTecnicas)
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null) return NotFound();


            if (cliente.Cotizaciones.Any() || cliente.VisitasTecnicas.Any())
            {
                TempData["Error"] = $"No se puede eliminar el cliente '{cliente.NombreRazonSocial}' porque tiene registros históricos asociados (Cotizaciones o Visitas Técnicas). Para mantener la integridad de los Datos, el cliente debe permanecer en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            TempData["Info"] = $"Cliente '{cliente.NombreRazonSocial}' eliminado permanentemente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
