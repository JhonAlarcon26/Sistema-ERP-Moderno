using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;
using System.Net;
using System.Net.Mail;

namespace Sistema_ERP.Controllers;

[Authorize(Policy = "VerAgenda")]
public class AgendaController : Controller
{
    private readonly ErpInventarioContext _context;
    private readonly IConfiguration _configuration;

    public AgendaController(ErpInventarioContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Clientes = await _context.Clientes.OrderBy(c => c.NombreRazonSocial).ToListAsync();
        ViewBag.Tecnicos = await _context.Usuarios.Where(u => u.Estado).OrderBy(u => u.NombreCompleto).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Listado()
    {
        var visitas = await _context.VisitasTecnicas
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.IdTecnicoNavigation)
            .OrderByDescending(v => v.IdVisita)
            .ToListAsync();
        return View(visitas);
    }


    [HttpGet]
    public async Task<IActionResult> GetVisitas(DateTime start, DateTime end)
    {
        var visitas = await _context.VisitasTecnicas
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.IdTecnicoNavigation)
            .Where(v => v.FechaVisita >= start && v.FechaVisita <= end)
            .Select(v => new
            {
                id = v.IdVisita,
                title = (v.IdClienteNavigation != null ? v.IdClienteNavigation.NombreRazonSocial : "Visita") + " - " + v.Estado,
                start = v.FechaVisita.ToString("yyyy-MM-ddTHH:mm:ss"),
                description = v.Descripcion,
                empresa = v.Empresa,
                tecnico = v.IdTecnicoNavigation != null ? v.IdTecnicoNavigation.NombreCompleto : "Sin asignar",
                className = v.Estado == "Pendiente" ? "bg-amber-500 border-amber-600" :
                            v.Estado == "En Proceso" ? "bg-indigo-500 border-indigo-600" :
                            "bg-emerald-500 border-emerald-600"
            })
            .ToListAsync();

        return Json(visitas);
    }

    [HttpGet]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var v = await _context.VisitasTecnicas
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.IdTecnicoNavigation)
            .FirstOrDefaultAsync(m => m.IdVisita == id);

        if (v == null) return NotFound();

        return Json(new
        {
            id = v.IdVisita,
            cliente = v.IdClienteNavigation?.NombreRazonSocial,
            empresa = v.Empresa,
            fecha = v.FechaVisita.ToString("dd/MM/yyyy hh:mm tt"),
            tecnico = v.IdTecnicoNavigation?.NombreCompleto,
            desc = v.Descripcion,
            estado = v.Estado,
            idCotizacion = v.IdCotizacion,
            idCliente = v.IdCliente
        });
    }

    [HttpPost]
    [Authorize(Policy = "CrearCita")]
    public async Task<IActionResult> Crear(int idCliente, string empresa, DateTime fecha, int idTecnico, string descripcion)
    {
        if (idCliente <= 0) return Json(new { success = false, message = "Cliente inválido." });

        try
        {
            var visita = new VisitaTecnica
            {
                IdCliente = idCliente,
                Empresa = empresa,
                FechaVisita = fecha,
                IdTecnico = idTecnico,
                Descripcion = descripcion,
                Estado = "Pendiente"
            };

            _context.VisitasTecnicas.Add(visita);
            await _context.SaveChangesAsync();


            var tecnico = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == visita.IdTecnico);
            var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == visita.IdCliente);
            string tecnicoNombre = tecnico?.NombreCompleto ?? "Técnico Asignado";
            string clienteNombre = cliente?.NombreRazonSocial ?? "Cliente";


            try
            {
                await EnviarEmailNotificacionReal(clienteNombre, visita.Empresa, visita.FechaVisita, tecnicoNombre, visita.Descripcion, "Nueva Visita Técnica Agendada 🗓️");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico enviando email (Crear): {ex.Message}");
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "EditarCita")]
    public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
    {
        var visita = await _context.VisitasTecnicas.FindAsync(id);
        if (visita == null) return Json(new { success = false });

        visita.Estado = nuevoEstado;
        await _context.SaveChangesAsync();


        if (nuevoEstado.Contains("Finalizad", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var tecnico = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == visita.IdTecnico);
                var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == visita.IdCliente);
                string tecnicoNombre = tecnico?.NombreCompleto ?? "Técnico Asignado";
                string clienteNombre = cliente?.NombreRazonSocial ?? "Cliente";


                try
                {
                    await EnviarEmailNotificacionReal(clienteNombre, visita.Empresa, visita.FechaVisita, tecnicoNombre, visita.Descripcion, "Visita Técnica Finalizada ✅");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error crítico enviando email: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error preparando Datos de email: {ex.Message}");
            }
        }

        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize(Policy = "EliminarCita")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var visita = await _context.VisitasTecnicas.FindAsync(id);
        if (visita != null)
        {
            _context.VisitasTecnicas.Remove(visita);
            await _context.SaveChangesAsync();
        }
        return Json(new { success = true });
    }

    private async Task EnviarEmailNotificacionReal(string clienteNombre, string? empresa, DateTime fecha, string tecnicoNombre, string? descripcion, string tituloAccion)
    {
        try
        {

            var smtps = await _context.ConfiguracionesSmtp
                .Where(s => s.Activo)
                .OrderBy(s => s.Prioridad)
                .ToListAsync();

            var todosLosCorreos = smtps.Select(s => s.Email).Distinct().ToList();
            var recipientEmailGeneral = _configuration["EmailSettings:RecipientEmail"];

            foreach (var smtp in smtps)
            {
                try
                {
                    string finalRecipient = string.IsNullOrWhiteSpace(recipientEmailGeneral) ? smtp.Email : recipientEmailGeneral;
                    await EnviarEmailIndividual(smtp.Email, smtp.Password, smtp.Host, smtp.Port,
                                              smtp.EnableSsl, finalRecipient,
                                              smtp.SenderName ?? "STC Services Notificaciones",
                                              clienteNombre, empresa, fecha, tecnicoNombre, descripcion, tituloAccion, todosLosCorreos);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Fallo en cuenta SMTP [{smtp.NombrePerfil}]: {ex.Message}");

                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error General Multi-SMTP: {ex.Message}");
        }
    }

    private async Task EnviarEmailIndividual(string senderEmail, string senderPass, string smtpServer, int port, bool ssl,
                                            string recipient, string senderDisplayName, string clienteNombre, string? empresa,
                                            DateTime fecha, string tecnicoNombre, string? descripcion, string tituloAccion, List<string>? todosLosBcc = null)
    {
        string colorHeader = tituloAccion.Contains("Finalizada") ? "#10b981" : "#4f46e5";

        try
        {
            using (var smtpClient = new SmtpClient(smtpServer))
            {
                smtpClient.Port = port;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPass);
                smtpClient.EnableSsl = ssl;

                var bodyHtml = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 16px; overflow: hidden; background-color: #ffffff;'>
                    <div style='background-color: {colorHeader}; padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 24px; text-transform: uppercase; letter-spacing: 2px;'>STC Services Agenda</h1>
                        <p style='color: rgba(255,255,255,0.8); margin-top: 10px;'>Gestión Técnica Profesional</p>
                    </div>
                    <div style='padding: 40px;'>
                        <h2 style='color: #1e293b; font-size: 20px; border-bottom: 2px solid #f1f5f9; padding-bottom: 10px;'>{tituloAccion}</h2>
                        <p style='color: #64748b; line-height: 1.6;'>Notificación de actualización de servicio técnico:</p>
                        <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Cliente</td>
                                <td style='padding: 12px 0; color: #1e293b; font-weight: bold;'>{clienteNombre}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Empresa / Tipo</td>
                                <td style='padding: 12px 0; color: #4f46e5; font-weight: bold;'>{empresa ?? "Particular"}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Fecha y Hora</td>
                                <td style='padding: 12px 0; color: #1e293b;'>{fecha:dd/MM/yyyy hh:mm tt}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Técnico Responsable</td>
                                <td style='padding: 12px 0; color: #1e293b; font-weight: bold;'>{tecnicoNombre}</td>
                            </tr>
                            <tr>
                                <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Descripción</td>
                                <td style='padding: 12px 0; color: #64748b; font-size: 14px;'>{descripcion ?? "Sin detalles adicionales."}</td>
                            </tr>
                        </table>
                    </div>
                </div>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderDisplayName),
                    Subject = $"{tituloAccion} - {tecnicoNombre}",
                    Body = bodyHtml,
                    IsBodyHtml = true,
                    SubjectEncoding = System.Text.Encoding.UTF8,
                    BodyEncoding = System.Text.Encoding.UTF8
                };

                if (!string.IsNullOrWhiteSpace(recipient))
                {
                    mailMessage.To.Add(recipient);
                }
                else
                {

                    mailMessage.To.Add(senderEmail);
                }




                if (todosLosBcc != null && todosLosBcc.Any())
                {
                    foreach (var bcc in todosLosBcc)
                    {
                        if (!string.Equals(recipient, bcc, StringComparison.OrdinalIgnoreCase))
                        {
                            mailMessage.Bcc.Add(bcc);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(senderEmail) &&
                    !string.Equals(recipient, senderEmail, StringComparison.OrdinalIgnoreCase))
                {
                    mailMessage.Bcc.Add(senderEmail);
                }

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"📧 Email enviado con éxito: {tituloAccion} para {clienteNombre}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error SMTP: {ex.Message}");
        }
    }
}
