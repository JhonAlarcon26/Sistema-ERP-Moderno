using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_ERP.Models;

[Table("Visitas_Tecnicas")]
public partial class VisitaTecnica
{
    [Key]
    [Column("ID_Visita")]
    public int IdVisita { get; set; }

    [Required]
    [Column("ID_Cliente")]
    public int IdCliente { get; set; }

    [StringLength(200)]
    public string? Empresa { get; set; }

    [Required]
    [Column("Fecha_Visita")]
    public DateTime FechaVisita { get; set; }

    [Required]
    [Column("ID_Tecnico")]
    public int IdTecnico { get; set; }

    public string? Descripcion { get; set; }

    [StringLength(50)]
    public string Estado { get; set; } = "Pendiente";

    [Column("ID_Cotizacion")]
    public int? IdCotizacion { get; set; }

    [Column("Fecha_Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;


    [ForeignKey("IdCliente")]
    public virtual Cliente? IdClienteNavigation { get; set; }

    [ForeignKey("IdTecnico")]
    public virtual Usuario? IdTecnicoNavigation { get; set; }

    [ForeignKey("IdCotizacion")]
    public virtual Cotizacione? IdCotizacionNavigation { get; set; }
}
