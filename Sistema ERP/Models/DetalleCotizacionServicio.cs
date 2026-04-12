namespace Sistema_ERP.Models;

public partial class DetalleCotizacionServicio
{
    public int IdDetalleServ { get; set; }

    public int IdCotizacion { get; set; }

    public int IdServicio { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioCobrado { get; set; }

    public bool Facturado { get; set; }

    public virtual Cotizacione IdCotizacionNavigation { get; set; } = null!;

    public virtual InventarioServicio IdServicioNavigation { get; set; } = null!;
}
