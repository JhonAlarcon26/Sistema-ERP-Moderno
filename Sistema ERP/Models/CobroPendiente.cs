namespace Sistema_ERP.Models;

public partial class CobroPendiente
{
    public int IdCobro { get; set; }

    public int IdOperacion { get; set; }

    public decimal MontoTotal { get; set; }

    public decimal MontoPagado { get; set; }

    public decimal MontoPendiente { get; set; }

    public DateTime FechaLimitePago { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string EstadoCobro { get; set; } = null!;

    public string? NotasAdicionales { get; set; }

    public virtual Cotizacione IdOperacionNavigation { get; set; } = null!;

    public virtual ICollection<HistorialPago> HistorialPagos { get; set; } = new List<HistorialPago>();
}
