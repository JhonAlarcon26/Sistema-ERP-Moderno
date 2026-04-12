using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Controllers
{
    [Authorize(Policy = "VerUsuarios")]
    public class UsuariosController : Controller
    {
        private readonly ErpInventarioContext _context;

        public UsuariosController(ErpInventarioContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .OrderByDescending(u => u.IdUsuario)
                .ToListAsync();
            return View(usuarios);
        }


        public async Task<IActionResult> Crear()
        {
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol");
            return View(new UsuarioViewModel { Estado = true });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CrearUsuario")]
        public async Task<IActionResult> Crear(UsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "La contraseña es obligatoria para nuevos usuarios.");
                    ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol");
                    return View(model);
                }


                if (await _context.Usuarios.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Ese nombre de usuario ya está en uso.");
                    ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol");
                    return View(model);
                }

                var usuario = new Usuario
                {
                    NombreCompleto = model.NombreCompleto,
                    Username = model.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    IdRol = model.IdRol,

                    Estado = model.Estado,
                    FechaCreacion = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Usuario '{usuario.Username}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol");
            return View(model);
        }


        [Authorize(Policy = "EditarUsuario")]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var model = new UsuarioViewModel
            {
                IdUsuario = usuario.IdUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Username = usuario.Username,
                IdRol = usuario.IdRol,

                Estado = usuario.Estado
            };

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol", model.IdRol);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditarUsuario")]
        public async Task<IActionResult> Editar(int id, UsuarioViewModel model)
        {
            if (id != model.IdUsuario) return NotFound();

            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null) return NotFound();


                if (await _context.Usuarios.AnyAsync(u => u.Username == model.Username && u.IdUsuario != id))
                {
                    ModelState.AddModelError("Username", "Ese nombre de usuario ya está en uso.");
                    ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol");
                    return View(model);
                }

                usuario.NombreCompleto = model.NombreCompleto;
                usuario.Username = model.Username;
                usuario.IdRol = model.IdRol;
                usuario.Estado = model.Estado;


                if (!string.IsNullOrEmpty(model.Password))
                {
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Usuario '{usuario.Username}' actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol");
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EliminarUsuario")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Cotizaciones)
                .Include(u => u.InventarioCompras)
                .Include(u => u.VisitasTecnicas)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return NotFound();


            if (usuario.Username == User.Identity?.Name)
            {
                TempData["Error"] = "No puedes eliminar tu propio usuario.";
                return RedirectToAction(nameof(Index));
            }


            if (usuario.Cotizaciones.Any() || usuario.InventarioCompras.Any() || usuario.VisitasTecnicas.Any())
            {
                TempData["Error"] = $"No se puede eliminar al usuario '{usuario.Username}' porque ya tiene registros asociados (Cotizaciones, Compras o Visitas). Se recomienda cambiar su estado a 'Inactivo' en lugar de eliminarlo.";
                return RedirectToAction(nameof(Index));
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            TempData["Info"] = $"Usuario '{usuario.Username}' eliminado permanentemente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
