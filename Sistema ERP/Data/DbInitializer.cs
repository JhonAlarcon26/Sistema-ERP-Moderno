using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;

namespace Sistema_ERP.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ErpInventarioContext context)
        {
            context.Database.EnsureCreated();

            if (context.Usuarios.Any(u => u.Username == "admin"))
            {
                return;
            }

            var permisosList = new[] {
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

            foreach (var pName in permisosList)
            {
                if (!context.Permisos.Any(p => p.NombrePermiso == pName))
                {
                    context.Permisos.Add(new Permiso { NombrePermiso = pName, Descripcion = $"Permiso para {pName}" });
                }
            }
            context.SaveChanges();

            var rolAdmin = context.Roles.Include(r => r.IdPermisos).FirstOrDefault(r => r.NombreRol == "Administrador");
            if (rolAdmin == null)
            {
                rolAdmin = new Role { NombreRol = "Administrador", Descripcion = "Rol con acceso total" };
                context.Roles.Add(rolAdmin);
                context.SaveChanges();
            }

            var todosLosPermisos = context.Permisos.ToList();
            foreach (var p in todosLosPermisos)
            {
                if (!rolAdmin.IdPermisos.Any(rp => rp.IdPermiso == p.IdPermiso))
                {
                    rolAdmin.IdPermisos.Add(p);
                }
            }
            context.SaveChanges();

            string hash = BCrypt.Net.BCrypt.HashPassword("admin", 11);
            context.Usuarios.Add(new Usuario
            {
                IdRol = rolAdmin.IdRol,
                NombreCompleto = "Administrador Sistema",
                Username = "admin",
                PasswordHash = hash,
                Estado = true,
                FechaCreacion = DateTime.Now
            });

            context.SaveChanges();
        }
    }
}
