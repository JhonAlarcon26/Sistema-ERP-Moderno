namespace Sistema_ERP.Models;

public partial class InventarioServicio
{
    public int IdServicio { get; set; }

    public string NombreServicio { get; set; } = null!;

    public string? Categoria { get; set; }

    public decimal PrecioVenta { get; set; }

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public int? IdTipoServicio { get; set; }

    public virtual TipoServicio? TipoServicioNavigation { get; set; }

    public virtual ICollection<DetalleCotizacionServicio> DetalleCotizacionServicios { get; set; } = new List<DetalleCotizacionServicio>();

    public virtual ICollection<EnlaceServicio> EnlacesServicios { get; set; } = new List<EnlaceServicio>();
}
