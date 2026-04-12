using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;
using System.Net;
using System.Net.Mail;

namespace Sistema_ERP.Controllers;

[Authorize(Policy = "VerConfig")]
public class ConfiguracionController : Controller
{
    private readonly ErpInventarioContext _context;

    public ConfiguracionController(ErpInventarioContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.GoogleApi = await _context.ConfiguracionesApi.FirstOrDefaultAsync(a => a.Proveedor == "GoogleMaps");
        ViewBag.Smtps = await _context.ConfiguracionesSmtp.OrderBy(s => s.Prioridad).ToListAsync();
        return View();
    }


    [HttpPost]
    [Authorize(Policy = "GestionarCuentasApi")]
    public async Task<IActionResult> GuardarGoogleApi([FromBody] string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || !apiKey.StartsWith("AIza") || apiKey.Length < 30)
        {
            return Json(new { success = false, message = "La API Key de Google no tiene un formato válido (debe empezar con AIza y tener al menos 30 caracteres)." });
        }

        try
        {
            var config = await _context.ConfiguracionesApi.FirstOrDefaultAsync(a => a.Proveedor == "GoogleMaps");
            if (config == null)
            {
                config = new ConfiguracionApi { Nombre = "Google Maps Principal", Proveedor = "GoogleMaps", ApiKey = apiKey, Activo = true, Prioridad = 1 };
                _context.ConfiguracionesApi.Add(config);
            }
            else
            {
                config.ApiKey = apiKey;
                _context.ConfiguracionesApi.Update(config);
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

    [HttpPost]
    [Authorize(Policy = "GestionarCuentasApi")]
    public async Task<IActionResult> EliminarApi(int id)
    {
        var api = await _context.ConfiguracionesApi.FindAsync(id);
        if (api != null)
        {
            _context.ConfiguracionesApi.Remove(api);
            await _context.SaveChangesAsync();
        }
        return Json(new { success = true });
    }


    [HttpPost]
    [Authorize(Policy = "AdministrarSmtp")]
    public async Task<IActionResult> GuardarSmtp([FromBody] ConfiguracionSmtp smtp)
    {
        try
        {
            if (smtp.IdSmtp > 0) { _context.ConfiguracionesSmtp.Update(smtp); }
            else { _context.ConfiguracionesSmtp.Add(smtp); }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

    [HttpPost]
    [Authorize(Policy = "AdministrarSmtp")]
    public async Task<IActionResult> EliminarSmtp(int id)
    {
        var smtp = await _context.ConfiguracionesSmtp.FindAsync(id);
        if (smtp != null)
        {
            _context.ConfiguracionesSmtp.Remove(smtp);
            await _context.SaveChangesAsync();
        }
        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize(Policy = "AdministrarSmtp")]
    public async Task<IActionResult> ProbarSmtp(int id)
    {
        var smtp = await _context.ConfiguracionesSmtp.FindAsync(id);
        if (smtp == null) return Json(new { success = false, message = "Configuración no encontrada." });

        try
        {
            using (var smtpClient = new SmtpClient(smtp.Host))
            {
                smtpClient.Port = smtp.Port;
                smtpClient.Credentials = new NetworkCredential(smtp.Email, smtp.Password);
                smtpClient.EnableSsl = smtp.EnableSsl;

                var ahora = DateTime.Now;

                var bodyHtml = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 16px; overflow: hidden; background-color: #ffffff;'>
                    <div style='background-color: #4f46e5; padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 24px; text-transform: uppercase; letter-spacing: 2px;'>STC Services SMTP</h1>
                        <p style='color: rgba(255,255,255,0.8); margin-top: 10px;'>Motor de Notificaciones</p>
                    </div>
                    <div style='padding: 40px;'>
                        <h2 style='color: #1e293b; font-size: 20px; border-bottom: 2px solid #f1f5f9; padding-bottom: 10px;'>Conexión Exitosa ✅</h2>
                        <p style='color: #64748b; line-height: 1.6;'>Su motor de correos está listo para funcionar correctamente. Prueba recibida a las {ahora:HH:mm:ss}.</p>
                        <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Cuenta Enviadora</td>
                                <td style='padding: 12px 0; color: #1e293b; font-weight: bold;'>{smtp.Email}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Servidor Host</td>
                                <td style='padding: 12px 0; color: #4f46e5; font-weight: bold;'>{smtp.Host} : {smtp.Port}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Protección SSL</td>
                                <td style='padding: 12px 0; color: #1e293b;'>{(smtp.EnableSsl ? "Activada" : "Desactivada")}</td>
                            </tr>
                        </table>
                    </div>
                </div>";




                var mailPrueba = new MailMessage
                {
                    From = new MailAddress(smtp.Email, smtp.SenderName),
                    Subject = $"✅ Prueba de Conexión SMTP - {ahora:dd/MM/yyyy HH:mm}",
                    Body = bodyHtml,
                    IsBodyHtml = true,
                };
                mailPrueba.To.Add(smtp.Email);
                await smtpClient.SendMailAsync(mailPrueba);







                var bodyBackup = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 16px; overflow: hidden; background-color: #ffffff;'>
                    <div style='background-color: #0f172a; padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 20px; letter-spacing: 2px;'>📋 COPIA DE SEGURIDAD</h1>
                        <p style='color: rgba(255,255,255,0.6); margin-top: 8px; font-size: 12px;'>Registro de Prueba SMTP — No Eliminar</p>
                    </div>
                    <div style='padding: 30px;'>
                        <p style='color: #64748b; font-size: 13px; line-height: 1.6;'>
                            Se realizó una prueba exitosa del perfil <strong style='color:#1e293b;'>{smtp.NombrePerfil}</strong> 
                            el día <strong style='color:#1e293b;'>{ahora:dd/MM/yyyy}</strong> a las <strong style='color:#1e293b;'>{ahora:HH:mm:ss}</strong>.
                        </p>
                        <div style='margin-top: 16px; padding: 16px; background: #f8fafc; border-radius: 12px; border: 1px solid #e2e8f0;'>
                            <p style='margin: 0; font-size: 11px; color: #94a3b8; text-transform: uppercase; font-weight: bold;'>Detalles Técnicos</p>
                            <p style='margin: 6px 0 0; font-size: 13px; color: #475569;'>Cuenta: {smtp.Email} · Host: {smtp.Host}:{smtp.Port} · SSL: {(smtp.EnableSsl ? "Sí" : "No")}</p>
                        </div>
                    </div>
                </div>";

                var mailBackup = new MailMessage
                {
                    From = new MailAddress(smtp.Email, "STC Backup"),
                    Subject = $"📋 Respaldo SMTP [{smtp.NombrePerfil}] — {ahora:dd/MM/yyyy HH:mm}",
                    Body = bodyBackup,
                    IsBodyHtml = true,
                };
                mailBackup.To.Add(smtp.Email);
                await smtpClient.SendMailAsync(mailBackup);
            }
            return Json(new { success = true, message = "Correo de prueba enviado con éxito a " + smtp.Email });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error de conexión: " + ex.Message });
        }
    }



}
