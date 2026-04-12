using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_ERP.Models;

[Table("Enlaces_Servicios")]
public class EnlaceServicio
{
    public int IdEnlace { get; set; }
    public int IdServicio { get; set; }

    public virtual EnlaceCompartido Enlace { get; set; } = null!;
    public virtual InventarioServicio Servicio { get; set; } = null!;
}
