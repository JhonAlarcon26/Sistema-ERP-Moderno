namespace Sistema_ERP.Models;

public partial class TipoServicio
{
    public int IdTipoServicio { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<InventarioServicio> InventarioServicios { get; set; } = new List<InventarioServicio>();
}
