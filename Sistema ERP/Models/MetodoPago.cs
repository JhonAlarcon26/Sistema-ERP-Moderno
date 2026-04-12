using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_ERP.Models;

[Table("MetodosPago")]
public partial class MetodoPago
{
    [Key]
    [Column("ID_Metodo")]
    public int IdMetodo { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Nombre_Metodo")]
    public string NombreMetodo { get; set; } = null!;

    public bool Estado { get; set; } = true;

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();
    public virtual ICollection<HistorialPago> HistorialPagos { get; set; } = new List<HistorialPago>();
    public virtual ICollection<InventarioCompra> InventarioCompras { get; set; } = new List<InventarioCompra>();
}
