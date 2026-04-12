using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PermisosController : Controller
    {
        private readonly ErpInventarioContext _context;

        public PermisosController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var permisos = await _context.Permisos.OrderByDescending(p => p.IdPermiso).ToListAsync();
            return View(permisos);
        }


        public IActionResult Crear()
        {
            return View(new Permiso());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([Bind("NombrePermiso,Descripcion")] Permiso permiso)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Permisos.AnyAsync(p => p.NombrePermiso == permiso.NombrePermiso))
                {
                    ModelState.AddModelError("NombrePermiso", "Ya existe un permiso con ese nombre.");
                    return View(permiso);
                }
                _context.Permisos.Add(permiso);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Permiso '{permiso.NombrePermiso}' creado.";
                return RedirectToAction(nameof(Index));
            }
            return View(permiso);
        }


        public async Task<IActionResult> Editar(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return NotFound();
            return View(permiso);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("IdPermiso,NombrePermiso,Descripcion")] Permiso permiso)
        {
            if (id != permiso.IdPermiso) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.Permisos.AnyAsync(p => p.NombrePermiso == permiso.NombrePermiso && p.IdPermiso != id))
                {
                    ModelState.AddModelError("NombrePermiso", "Ya existe un permiso con ese nombre.");
                    return View(permiso);
                }
                try
                {
                    _context.Update(permiso);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Permiso '{permiso.NombrePermiso}' actualizado.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Permisos.AnyAsync(p => p.IdPermiso == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(permiso);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var permiso = await _context.Permisos
                .Include(p => p.IdRols)
                .FirstOrDefaultAsync(p => p.IdPermiso == id);

            if (permiso == null) return NotFound();

            if (permiso.IdRols.Any())
            {
                TempData["Error"] = $"No se puede eliminar '{permiso.NombrePermiso}' porque está asignado a {permiso.IdRols.Count} rol(es).";
                return RedirectToAction(nameof(Index));
            }

            _context.Permisos.Remove(permiso);
            await _context.SaveChangesAsync();
            TempData["Info"] = $"Permiso '{permiso.NombrePermiso}' eliminado.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SincronizarPermisos")]
        public async Task<IActionResult> Sincronizar()
        {
            try
            {
                var xPermisos = new[] {
                    "VerVentas", "CrearVenta", "EditarVenta", "EliminarVenta", "Cotizar", "ImprimirVenta", "Vender", "CancelarVenta", "GuardarVenta",
                    "VerCompras", "CrearCompra", "EditarCompra", "EliminarCompra", "ImprimirCompra", "CancelarCompra", "GuardarCompra",
                    "VerStock", "VerSaldo", "EditarStock", "EliminarStock",
                    "VerClientes", "CrearCliente", "EditarCliente", "EliminarCliente", "VerMapaCliente", "GuardarCliente",
                    "VerProveedores", "CrearProveedor", "EditarProveedor", "EliminarProveedor", "VerMapaProveedor", "GuardarProveedor",
                    "VerAgenda", "CrearCita", "EditarCita", "EliminarCita", "FinalizarCita", "CancelarCita",
                    "VerCobros", "Liquidar", "GestionarPendientes", "GestionarRetrasados", "ImprimirRecibo",
                    "VerConfig", "GestionarCuentasApi", "AdministrarSmtp",
                    "VerUsuarios", "CrearUsuario", "EditarUsuario", "EliminarUsuario", "CambiarEstadoUsuario",
                    "VerRoles", "CrearRol", "EditarRol", "EliminarRol", "AsignarPermisos", "VerPermisos", "SincronizarPermisos",
                    "VerProductos", "CrearProducto", "EditarProducto", "EliminarProducto",
                    "VerServicios", "CrearServicio", "EditarServicio", "EliminarServicio",
                    "VerReportes", "VerCatalogo", "PresentarCatalogo", "VerDashboard"
                };

                var permisosExistentes = await _context.Permisos.ToListAsync();
                int creados = 0;

                foreach (var pName in xPermisos)
                {
                    if (!permisosExistentes.Any(p => p.NombrePermiso == pName))
                    {
                        _context.Permisos.Add(new Permiso { NombrePermiso = pName, Descripcion = $"Permiso para {pName}" });
                        creados++;
                    }
                }

                if (creados > 0) await _context.SaveChangesAsync();


                var rolAdmin = await _context.Roles.Include(r => r.IdPermisos).FirstOrDefaultAsync(r => r.NombreRol == "Administrador");
                if (rolAdmin != null)
                {
                    var todosLosP = await _context.Permisos.ToListAsync();
                    foreach (var p in todosLosP)
                    {
                        if (xPermisos.Contains(p.NombrePermiso) && !rolAdmin.IdPermisos.Any(rp => rp.IdPermiso == p.IdPermiso))
                        {
                            rolAdmin.IdPermisos.Add(p);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"Sincronización completada. Se restauraron {creados} permisos y se actualizaron los accesos del Administrador.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error durante la sincronización: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
