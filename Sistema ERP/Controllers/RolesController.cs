using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerRoles")]
    public class RolesController : Controller
    {
        private readonly ErpInventarioContext _context;

        public RolesController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Include(r => r.IdPermisos)
                .Include(r => r.Usuarios)
                .OrderByDescending(r => r.IdRol)
                .ToListAsync();
            return View(roles);
        }


        public IActionResult Crear()
        {
            return View(new Role());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CrearRol")]
        public async Task<IActionResult> Crear([Bind("NombreRol,Descripcion")] Role role)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Roles.AnyAsync(r => r.NombreRol == role.NombreRol))
                {
                    ModelState.AddModelError("NombreRol", "Ya existe un rol con ese nombre.");
                    return View(role);
                }
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Rol '{role.NombreRol}' creado. Ahora asigne sus permisos.";
                return RedirectToAction(nameof(GestionarPermisos), new { id = role.IdRol });
            }
            return View(role);
        }


        [Authorize(Policy = "EditarRol")]
        public async Task<IActionResult> Editar(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditarRol")]
        public async Task<IActionResult> Editar(int id, [Bind("IdRol,NombreRol,Descripcion")] Role role)
        {
            if (id != role.IdRol) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.Roles.AnyAsync(r => r.NombreRol == role.NombreRol && r.IdRol != id))
                {
                    ModelState.AddModelError("NombreRol", "Ya existe un rol con ese nombre.");
                    return View(role);
                }

                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Rol '{role.NombreRol}' actualizado.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Roles.AnyAsync(r => r.IdRol == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }


        [Authorize(Policy = "AsignarPermisos")]
        public async Task<IActionResult> GestionarPermisos(int id)
        {
            var role = await _context.Roles
                .Include(r => r.IdPermisos)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (role == null) return NotFound();

            var todosPermisos = await _context.Permisos.OrderBy(p => p.NombrePermiso).ToListAsync();
            var permisosDelRol = role.IdPermisos.Select(p => p.IdPermiso).ToHashSet();

            var viewModel = new RolPermisoViewModel
            {
                Rol = role,
                Permisos = todosPermisos.Select(p => new PermisoCheckbox
                {
                    IdPermiso = p.IdPermiso,
                    NombrePermiso = p.NombrePermiso,
                    Descripcion = p.Descripcion,
                    Seleccionado = permisosDelRol.Contains(p.IdPermiso)
                }).OrderByDescending(p => p.Seleccionado).ThenBy(p => p.NombrePermiso).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AsignarPermisos")]
        public async Task<IActionResult> GestionarPermisos(int id, List<int> permisosSeleccionados)
        {
            var role = await _context.Roles
                .Include(r => r.IdPermisos)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (role == null) return NotFound();


            role.IdPermisos.Clear();

            if (permisosSeleccionados != null && permisosSeleccionados.Any())
            {
                var permisosElegidos = await _context.Permisos
                    .Where(p => permisosSeleccionados.Contains(p.IdPermiso))
                    .ToListAsync();

                foreach (var permiso in permisosElegidos)
                    role.IdPermisos.Add(permiso);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Permisos del rol '{role.NombreRol}' guardados correctamente.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EliminarRol")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Usuarios)
                .Include(r => r.IdPermisos)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (role == null) return NotFound();

            if (role.Usuarios.Any())
            {
                TempData["Error"] = $"No se puede eliminar '{role.NombreRol}' porque tiene usuarios asignados.";
                return RedirectToAction(nameof(Index));
            }


            role.IdPermisos.Clear();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            TempData["Info"] = $"Rol '{role.NombreRol}' eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
