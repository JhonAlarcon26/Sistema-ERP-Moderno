namespace Sistema_ERP.Models;

public partial class InventarioCompra
{
    public int IdIngreso { get; set; }

    public int IdProducto { get; set; }

    public int CantidadComprada { get; set; }

    public decimal CostoCompraUnitario { get; set; }

    public DateTime? FechaIngreso { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdProveedor { get; set; }
    public int? IdMetodoPago { get; set; }

    public decimal? PrecioRefHistorico { get; set; }
    public decimal? PrecioVentaHistorico { get; set; }

    public virtual InventarioProducto IdProductoNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual Proveedor? IdProveedorNavigation { get; set; }
    public virtual MetodoPago? IdMetodoPagoNavigation { get; set; }
}
