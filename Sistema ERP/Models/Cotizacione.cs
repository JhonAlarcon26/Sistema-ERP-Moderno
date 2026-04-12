namespace Sistema_ERP.Models;

public partial class Cotizacione
{
    public int IdCotizacion { get; set; }

    public int IdCliente { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Estado { get; set; }

    public int? IdUsuario { get; set; }


    public string? NroCorrelativo { get; set; }
    public string TipoOperacion { get; set; } = "Cotizacion";
    public decimal? PorcentajeImpuesto { get; set; }
    public decimal? MontoImpuesto { get; set; }
    public decimal? TotalConImpuesto { get; set; }
    public string? EstadoPago { get; set; }
    public int? IdCotizacionOrigen { get; set; }
    public int? IdMetodoPago { get; set; }

    public virtual Cotizacione? IdCotizacionOrigenNavigation { get; set; }
    public virtual MetodoPago? IdMetodoPagoNavigation { get; set; }
    public virtual ICollection<Cotizacione> InverseIdCotizacionOrigenNavigation { get; set; } = new List<Cotizacione>();
    public virtual ICollection<CobroPendiente> CobrosPendientes { get; set; } = new List<CobroPendiente>();


    public virtual ICollection<DetalleCotizacionProducto> DetalleCotizacionProductos { get; set; } = new List<DetalleCotizacionProducto>();

    public virtual ICollection<DetalleCotizacionServicio> DetalleCotizacionServicios { get; set; } = new List<DetalleCotizacionServicio>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioNavigation { get; set; }
    public virtual ICollection<VisitaTecnica> VisitasTecnicas { get; set; } = new List<VisitaTecnica>();
}
