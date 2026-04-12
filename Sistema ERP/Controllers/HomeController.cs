using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_ERP.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace Sistema_ERP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ErpInventarioContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, ErpInventarioContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var context = HttpContext.RequestServices.GetService(typeof(ErpInventarioContext)) as ErpInventarioContext;
            if (context != null)
            {
                var hoy = DateTime.Today;
                var manana = hoy.AddDays(1);
                ViewBag.VisitasHoy = await context.VisitasTecnicas
                    .Include(v => v.IdClienteNavigation)
                    .Where(v => v.FechaVisita >= hoy && v.FechaVisita < manana)
                    .OrderBy(v => v.FechaVisita)
                    .ToListAsync();
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosClientesGrafica(string rango = "global")
        {
            var labels = new List<string>();
            var Data = new List<decimal>();
            var hoy = DateTime.Today;
            var fechaInicio = DateTime.MinValue;

            switch (rango)
            {
                case "hoy": fechaInicio = hoy; break;
                case "1semana": fechaInicio = hoy.AddDays(-7); break;
                case "1mes": fechaInicio = hoy.AddMonths(-1); break;
                case "1ano": fechaInicio = hoy.AddYears(-1); break;
                case "2anos": fechaInicio = hoy.AddYears(-2); break;
                case "3anos": fechaInicio = hoy.AddYears(-3); break;
                case "4anos": fechaInicio = hoy.AddYears(-4); break;
                case "5anos": fechaInicio = hoy.AddYears(-5); break;
                case "global": default: fechaInicio = DateTime.MinValue; break;
            }

            var ventas = await _context.Cotizaciones
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.DetalleCotizacionProductos)
                .Include(c => c.DetalleCotizacionServicios)
                .Where(c => c.TipoOperacion == "Venta" && c.Fecha >= fechaInicio)
                .ToListAsync();

            var resumenGanancias = ventas
                .GroupBy(v => v.IdClienteNavigation.NombreRazonSocial)
                .Select(g => new
                {
                    Cliente = g.Key,
                    FechaPrimeraVenta = g.Min(v => v.Fecha ?? DateTime.MaxValue),
                    Ganancia = g.Sum(v => 
                        v.DetalleCotizacionProductos.Sum(p => (p.PrecioVendido - p.CostoReferencial) * p.Cantidad) +
                        v.DetalleCotizacionServicios.Sum(s => s.PrecioCobrado * s.Cantidad)
                    )
                })
                .Where(x => x.Ganancia > 0)
                .OrderBy(x => x.FechaPrimeraVenta)
                .ToList();

            foreach (var item in resumenGanancias)
            {
                labels.Add(item.Cliente);
                Data.Add(item.Ganancia);
            }

            if (!resumenGanancias.Any())
            {
                labels.Add("Sin Ventas");
                Data.Add(0);
            }

            return Json(new { success = true, labels, Data });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosCobranzasGrafica(string rango = "5anos")
        {
            var labels = new List<string>();
            var DataPendientes = new List<decimal>();
            var DataVencidas = new List<decimal>();
            var hoy = DateTime.Today;

            if (rango == "hoy")
            {
                var ahora = DateTime.Now;
                var resumenClientes = await _context.CobrosPendientes
                    .Include(c => c.IdOperacionNavigation).ThenInclude(o => o.IdClienteNavigation)
                    .Where(c => c.EstadoCobro != "Cobrado")
                    .GroupBy(c => c.IdOperacionNavigation.IdClienteNavigation.NombreRazonSocial)
                    .Select(g => new
                    {
                        Nombre = g.Key,
                        Pendiente = g.Where(x => x.FechaLimitePago >= ahora).Sum(x => x.MontoPendiente),
                        Vencido = g.Where(x => x.FechaLimitePago < ahora).Sum(x => x.MontoPendiente)
                    })
                    .OrderByDescending(x => x.Vencido + x.Pendiente)
                    .Take(15)
                    .ToListAsync();

                foreach (var item in resumenClientes)
                {
                    labels.Add(item.Nombre);
                    DataPendientes.Add(item.Pendiente);
                    DataVencidas.Add(item.Vencido);
                }
                if (!resumenClientes.Any()) { labels.Add("Sin Deudas"); DataPendientes.Add(0); DataVencidas.Add(0); }
            }
            else if (rango == "1semana")
            {
                for (int i = 6; i >= 0; i--)
                {
                    var dia = hoy.AddDays(-i);
                    labels.Add(dia.ToString("ddd dd"));
                    DataPendientes.Add(await _context.CobrosPendientes.Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago >= dia && c.FechaRegistro <= dia).SumAsync(c => c.MontoPendiente));
                    DataVencidas.Add(await _context.CobrosPendientes.Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago < dia && c.FechaRegistro <= dia).SumAsync(c => c.MontoPendiente));
                }
            }
            else if (rango == "global")
            {
                var primerCobro = await _context.CobrosPendientes.OrderBy(c => c.FechaRegistro).FirstOrDefaultAsync();
                var fechaMin = primerCobro?.FechaRegistro ?? hoy.AddYears(-1);
                var totalDias = (hoy - fechaMin).TotalDays;
                int intervalos = 10;
                double salto = totalDias / (intervalos - 1);

                for (int i = 0; i < intervalos; i++)
                {
                    var fechaCorte = fechaMin.AddDays(i * salto);
                    labels.Add(fechaCorte.ToString("MMM yyyy"));
                    DataPendientes.Add(await _context.CobrosPendientes.Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago >= fechaCorte && c.FechaRegistro <= fechaCorte).SumAsync(c => c.MontoPendiente));
                    DataVencidas.Add(await _context.CobrosPendientes.Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago < fechaCorte && c.FechaRegistro <= fechaCorte).SumAsync(c => c.MontoPendiente));
                }
            }
            else
            {
                int meses = 1;
                if (rango == "1mes") meses = 1;
                else if (rango.EndsWith("anos"))
                {
                    int.TryParse(rango.Replace("anos", ""), out meses);
                    meses *= 12;
                }
                else if (rango == "1ano") meses = 12;

                int puntos = 8;
                for (int i = puntos - 1; i >= 0; i--)
                {
                    var fechaCorte = hoy.AddMonths(-i * (meses / puntos));
                    labels.Add(fechaCorte.ToString("dd/MM/yy"));
                    DataPendientes.Add(await _context.CobrosPendientes.Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago >= fechaCorte && c.FechaRegistro <= fechaCorte).SumAsync(c => c.MontoPendiente));
                    DataVencidas.Add(await _context.CobrosPendientes.Where(c => c.EstadoCobro != "Cobrado" && c.FechaLimitePago < fechaCorte && c.FechaRegistro <= fechaCorte).SumAsync(c => c.MontoPendiente));
                }
            }

            return Json(new { success = true, labels, pendientes = DataPendientes, vencidas = DataVencidas });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosLiquidacionesGrafica(string rango = "5anos")
        {
            var labels = new List<string>();
            var Data = new List<decimal>();
            var hoy = DateTime.Today;

            if (rango == "hoy")
            {
                var pagosHoy = await _context.HistorialPagos
                    .Where(p => p.FechaPago >= hoy)
                    .OrderBy(p => p.FechaPago)
                    .ToListAsync();

                foreach (var pago in pagosHoy)
                {
                    labels.Add(pago.FechaPago.ToString("HH:mm"));
                    Data.Add(pago.MontoAbonado);
                }
                if (!pagosHoy.Any()) { labels.Add("Sin Pagos"); Data.Add(0); }
            }
            else if (rango == "1semana")
            {
                for (int i = 6; i >= 0; i--)
                {
                    var dia = hoy.AddDays(-i);
                    labels.Add(dia.ToString("ddd dd"));
                    Data.Add(await _context.HistorialPagos.Where(p => p.FechaPago.Date == dia.Date).SumAsync(p => p.MontoAbonado));
                }
            }
            else if (rango == "global")
            {
                var primerPago = await _context.HistorialPagos.OrderBy(p => p.FechaPago).FirstOrDefaultAsync();
                var fechaMin = primerPago?.FechaPago ?? hoy.AddYears(-1);
                var totalDias = (hoy - fechaMin).TotalDays;
                int intervalos = 10;
                double salto = totalDias / (intervalos - 1);

                for (int i = 0; i < intervalos; i++)
                {
                    var fechaInicio = fechaMin.AddDays(i * salto);
                    var fechaFin = fechaMin.AddDays((i + 1) * salto);
                    labels.Add(fechaInicio.ToString("MMM yy"));
                    Data.Add(await _context.HistorialPagos.Where(p => p.FechaPago >= fechaInicio && p.FechaPago < fechaFin).SumAsync(p => p.MontoAbonado));
                }
            }
            else
            {
                int meses = 1;
                if (rango == "1mes") meses = 1;
                else if (rango.EndsWith("anos"))
                {
                    int.TryParse(rango.Replace("anos", ""), out meses);
                    meses *= 12;
                }
                else if (rango == "1ano") meses = 12;

                int puntos = 10;
                for (int i = puntos - 1; i >= 0; i--)
                {
                    var fInicio = hoy.AddMonths(-(i + 1) * (meses / puntos));
                    var fFin = hoy.AddMonths(-i * (meses / puntos)).AddDays(1).AddSeconds(-1);
                    labels.Add(fFin.ToString("dd/MM/yy"));
                    Data.Add(await _context.HistorialPagos.Where(p => p.FechaPago >= fInicio && p.FechaPago <= fFin).SumAsync(p => p.MontoAbonado));
                }
            }

            return Json(new { success = true, labels, Data });
        }


        [HttpPost]
        public async Task<IActionResult> FinalizarTarea(int id)
        {
            var tarea = await _context.VisitasTecnicas
                .Include(v => v.IdClienteNavigation)
                .Include(v => v.IdTecnicoNavigation)
                .FirstOrDefaultAsync(v => v.IdVisita == id);

            if (tarea == null) return Json(new { success = false, message = "Tarea no encontrada" });

            tarea.Estado = "Finalizada";

            _context.VisitasTecnicas.Update(tarea);
            await _context.SaveChangesAsync();


            var clienteNombre = tarea.IdClienteNavigation?.NombreRazonSocial ?? "Cliente";
            var tecnicoNombre = tarea.IdTecnicoNavigation?.NombreCompleto ?? "Técnico Asignado";

            _ = Task.Run(async () =>
            {
                try
                {
                    await EnviarEmailNotificacionReal(clienteNombre, tarea.Empresa, tarea.FechaVisita, tecnicoNombre, tarea.Descripcion, "Tarea Finalizada desde Dashboard ✅");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando email desde Dashboard: {ex.Message}");
                }
            });

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

                if (!smtps.Any())
                {

                    var settings = _configuration.GetSection("EmailSettings");
                    string senderEmail = settings["SenderEmail"] ?? "";
                    if (string.IsNullOrEmpty(senderEmail) || senderEmail.Contains("[TU_CORREO]")) return;

                    await EnviarEmailIndividual(senderEmail, settings["SenderPassword"] ?? "", settings["SmtpServer"] ?? "",
                                              int.Parse(settings["Port"]?.ToString() ?? "587"),
                                              bool.Parse(settings["EnableSsl"]?.ToString() ?? "true"),
                                              settings["RecipientEmail"] ?? senderEmail,
                                              "Notificaciones ERP",
                                              clienteNombre, empresa, fecha, tecnicoNombre, descripcion, tituloAccion);
                    return;
                }

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
                Console.WriteLine($"❌ Error General Multi-SMTP (Home): {ex.Message}");
            }
        }

        private async Task EnviarEmailIndividual(string senderEmail, string senderPass, string smtpServer, int port, bool ssl,
                                                string recipient, string senderDisplayName, string clienteNombre, string? empresa,
                                                DateTime fecha, string tecnicoNombre, string? descripcion, string tituloAccion, List<string>? todosLosBcc = null)
        {
            string colorHeader = "#10b981";

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
                            <h1 style='color: white; margin: 0; font-size: 24px; text-transform: uppercase; letter-spacing: 2px;'>STC Services</h1>
                            <p style='color: rgba(255,255,255,0.8); margin-top: 10px;'>Gestión Operativa</p>
                        </div>
                        <div style='padding: 40px;'>
                            <h2 style='color: #1e293b; font-size: 20px; border-bottom: 2px solid #f1f5f9; padding-bottom: 10px;'>{tituloAccion}</h2>
                            <p style='color: #64748b; line-height: 1.6;'>Detalles de la operación:</p>
                            <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                                <tr>
                                    <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Cliente</td>
                                    <td style='padding: 12px 0; color: #1e293b; font-weight: bold;'>{clienteNombre}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Empresa</td>
                                    <td style='padding: 12px 0; color: #4f46e5; font-weight: bold;'>{empresa ?? "Particular"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 0; color: #94a3b8; font-size: 12px; font-weight: bold; text-transform: uppercase;'>Fecha</td>
                                    <td style='padding: 12px 0; color: #1e293b;'>{fecha:dd/MM/yyyy hh:mm tt}</td>
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
                    Console.WriteLine($"📧 Email enviado con éxito: {tituloAccion} (Dashboard)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error SMTP Dashboard: {ex.Message}");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
