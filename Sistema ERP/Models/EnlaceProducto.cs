using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_ERP.Models;

[Table("Enlaces_Productos")]
public class EnlaceProducto
{
    public int IdEnlace { get; set; }
    public int IdProducto { get; set; }

    public virtual EnlaceCompartido Enlace { get; set; } = null!;
    public virtual InventarioProducto Producto { get; set; } = null!;
}
