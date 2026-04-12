namespace Sistema_ERP.Models;

public partial class TipoProducto
{
    public int IdTipoProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<InventarioProducto> InventarioProductos { get; set; } = new List<InventarioProducto>();
}
