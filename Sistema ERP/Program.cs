using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Sistema_ERP.Authorization;
using Sistema_ERP.Models;
using System.Globalization;



QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddDbContext<ErpInventarioContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();


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

builder.Services.AddAuthorization(options =>
{
    foreach (var p in permisosList)
    {
        options.AddPolicy(p, policy => policy.Requirements.Add(new PermissionRequirement(p)));
    }
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });



var app = builder.Build();


var cultureInfo = new CultureInfo("es-BO");
cultureInfo.NumberFormat.CurrencySymbol = "Bs.";

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
});


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
