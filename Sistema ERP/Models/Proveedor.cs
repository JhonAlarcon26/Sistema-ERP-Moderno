namespace Sistema_ERP.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public string Nombre { get; set; } = null!;

    public string? TipoProveedor { get; set; }

    public string? NitCi { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }
    public string? Latitud { get; set; }
    public string? Longitud { get; set; }

    public virtual ICollection<InventarioCompra> InventarioCompras { get; set; } = new List<InventarioCompra>();
}
