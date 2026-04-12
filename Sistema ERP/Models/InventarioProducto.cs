namespace Sistema_ERP.Models;

public partial class InventarioProducto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = null!;

    public string? Categoria { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal PrecioInterno { get; set; }

    public decimal PrecioVentaSugerido { get; set; }

    public int? Stock { get; set; }

    public string? ImagenUrl { get; set; }

    public string? CodigoSN { get; set; }

    public string? Descripcion { get; set; }
    
    public int? IdTipoProducto { get; set; }

    public virtual TipoProducto? TipoProductoNavigation { get; set; }

    public virtual ICollection<DetalleCotizacionProducto> DetalleCotizacionProductos { get; set; } = new List<DetalleCotizacionProducto>();

    public virtual ICollection<EnlaceProducto> EnlacesProductos { get; set; } = new List<EnlaceProducto>();
    
    public virtual ICollection<InventarioCompra> InventarioCompras { get; set; } = new List<InventarioCompra>();
}
