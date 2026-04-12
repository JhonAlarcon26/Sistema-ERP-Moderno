namespace Sistema_ERP.Models;

public partial class DetalleCotizacionProducto
{
    public int IdDetalleProd { get; set; }

    public int IdCotizacion { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioVendido { get; set; }

    public decimal CostoReferencial { get; set; }

    public bool Facturado { get; set; }

    public virtual Cotizacione IdCotizacionNavigation { get; set; } = null!;

    public virtual InventarioProducto IdProductoNavigation { get; set; } = null!;
}
