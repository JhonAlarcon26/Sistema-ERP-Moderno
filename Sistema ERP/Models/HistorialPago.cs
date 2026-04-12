namespace Sistema_ERP.Models;

public partial class HistorialPago
{
    public int IdPago { get; set; }

    public int IdCobro { get; set; }

    public decimal MontoAbonado { get; set; }

    public DateTime FechaPago { get; set; }

    public string? MetodoPago { get; set; }

    public string? Comprobante { get; set; }

    public int? IdMetodoPago { get; set; }

    public virtual CobroPendiente IdCobroNavigation { get; set; } = null!;

    public virtual MetodoPago? IdMetodoPagoNavigation { get; set; }
}
