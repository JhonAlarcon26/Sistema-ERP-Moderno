using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;
using System.Security.Claims;

namespace Sistema_ERP.Controllers
{
    public class AccountController : Controller
    {
        private readonly ErpInventarioContext _context;

        public AccountController(ErpInventarioContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.HasClaim("Permission", "VerDashboard") || User.IsInRole("Administrador")) return RedirectToAction("Index", "Home");
                if (User.HasClaim("Permission", "VerVentas")) return RedirectToAction("Index", "Cotizaciones");
                if (User.HasClaim("Permission", "VerCompras")) return RedirectToAction("Index", "Compras");
                if (User.HasClaim("Permission", "VerStock")) return RedirectToAction("Index", "Stock");
                if (User.HasClaim("Permission", "VerAgenda")) return RedirectToAction("Index", "Agenda");
                if (User.HasClaim("Permission", "VerCobros")) return RedirectToAction("Index", "Cobros");
                if (User.HasClaim("Permission", "VerClientes")) return RedirectToAction("Index", "Clientes");
                if (User.HasClaim("Permission", "VerProveedores")) return RedirectToAction("Index", "Proveedores");
                if (User.HasClaim("Permission", "VerProductos")) return RedirectToAction("Index", "Productos");
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdRolNavigation)
                    .ThenInclude(r => r.IdPermisos)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower() && u.Estado == true);

                if (usuario != null && usuario.IdRolNavigation != null && BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.Username),
                        new Claim("FullName", usuario.NombreCompleto),
                        new Claim(ClaimTypes.Role, usuario.IdRolNavigation.NombreRol),
                        new Claim("UserId", usuario.IdUsuario.ToString())
                    };

                    foreach (var permiso in usuario.IdRolNavigation.IdPermisos)
                    {
                        claims.Add(new Claim("Permission", permiso.NombrePermiso));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);


                    if (usuario.IdRolNavigation.NombreRol == "Administrador" || usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerDashboard"))
                        return RedirectToAction("Index", "Home");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerVentas")) return RedirectToAction("Index", "Cotizaciones");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerCompras")) return RedirectToAction("Index", "Compras");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerStock")) return RedirectToAction("Index", "Stock");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerAgenda")) return RedirectToAction("Index", "Agenda");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerCobros")) return RedirectToAction("Index", "Cobros");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerClientes")) return RedirectToAction("Index", "Clientes");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerProveedores")) return RedirectToAction("Index", "Proveedores");
                    if (usuario.IdRolNavigation.IdPermisos.Any(p => p.NombrePermiso == "VerProductos")) return RedirectToAction("Index", "Productos");

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

   }
        [AllowAnonymous]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                var userCount = await _context.Usuarios.CountAsync();
                var roles = await _context.Roles.Select(r => r.NombreRol).ToListAsync();
                return Content($"✅ Conexión exitosa. Usuarios en DB: {userCount}. Roles disponibles: {string.Join(", ", roles)}");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error de conexión: {ex.Message} - Asegúrate de que SQL Server esté corriendo y la cadena de conexión en appsettings.json sea correcta.");
            }
        }
    }
}
