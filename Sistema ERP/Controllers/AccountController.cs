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


        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            try
            {

                var xPermisos = new[] {
                    "VerDashboard",
                    "VerVentas", "CrearVenta", "EditarVenta", "EliminarVenta", "Cotizar", "ImprimirVenta", "Vender", "CancelarVenta", "GuardarVenta",
                    "VerCompras", "CrearCompra", "EditarCompra", "EliminarCompra", "ImprimirCompra", "CancelarCompra", "GuardarCompra",
                    "VerStock", "VerSaldo", "EditarStock", "EliminarStock",
                    "VerClientes", "CrearCliente", "EditarCliente", "EliminarCliente", "VerMapaCliente", "GuardarCliente",
                    "VerProveedores", "CrearProveedor", "EditarProveedor", "EliminarProveedor", "VerMapaProveedor", "GuardarProveedor",
                    "VerAgenda", "AgendarCita", "EditarCita", "EliminarCita", "FinalizarCita", "CancelarCita",
                    "VerCobros", "Liquidar", "GestionarPendientes", "GestionarRetrasados", "ImprimirRecibo",
                    "VerConfig", "GestionarCuentasApi", "AdministrarSmtp",
                    "VerUsuarios", "CrearUsuario", "EditarUsuario", "EliminarUsuario", "CambiarEstadoUsuario",
                    "VerRoles", "CrearRol", "EditarRol", "EliminarRol", "AsignarPermisos", "VerPermisos", "SincronizarPermisos",
                    "VerProductos", "CrearProducto", "EditarProducto", "EliminarProducto",
                    "VerServicios", "CrearServicio", "EditarServicio", "EliminarServicio",
                    "VerReportes", "VerCatalogo", "PresentarCatalogo"
                };


                var permisosExistentes = await _context.Permisos.ToListAsync();
                foreach (var pName in xPermisos)
                {
                    if (!permisosExistentes.Any(p => p.NombrePermiso == pName))
                    {
                        _context.Permisos.Add(new Permiso { NombrePermiso = pName, Descripcion = $"Permiso para {pName}" });
                    }
                }
                await _context.SaveChangesAsync();


                var rol = await _context.Roles.Include(r => r.IdPermisos).FirstOrDefaultAsync(r => r.NombreRol == "Administrador");
                if (rol == null)
                {
                    rol = new Role { NombreRol = "Administrador", Descripcion = "Rol con acceso total al sistema" };
                    _context.Roles.Add(rol);
                    await _context.SaveChangesAsync();
                }


                var todosLosPermisos = await _context.Permisos.ToListAsync();
                foreach (var p in todosLosPermisos)
                {
                    if (!rol.IdPermisos.Any(rp => rp.IdPermiso == p.IdPermiso))
                    {
                        rol.IdPermisos.Add(p);
                    }
                }
                await _context.SaveChangesAsync();


                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin12345");
                string hash = BCrypt.Net.BCrypt.HashPassword("75629487", 11);

                if (usuario != null)
                {
                    usuario.PasswordHash = hash;
                    usuario.Estado = true;
                    usuario.IdRol = rol.IdRol;
                    _context.Update(usuario);
                }
                else
                {
                    _context.Usuarios.Add(new Usuario
                    {
                        IdRol = rol.IdRol,
                        NombreCompleto = "Administrador Sistema",
                        Username = "admin12345",
                        PasswordHash = hash,
                        Estado = true,
                        FechaCreacion = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();

                return Content("✅ Sistema de Permisos inicializado correctamente. Usuario 'admin12345' con acceso total configurado.");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error durante el Seed: {ex.Message} - {ex.InnerException?.Message}");
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
