using System.ComponentModel.DataAnnotations;

namespace Sistema_ERP.Models;

public partial class ConfiguracionApi
{
    [Key]
    public int Idapi { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = null!;

    [Required]
    public string ApiKey { get; set; } = null!;

    [Required]
    public int Prioridad { get; set; } = 1;

    public bool Activo { get; set; } = true;

    [MaxLength(255)]
    public string? Proveedor { get; set; }

    public int? IdUsuario { get; set; }

    public virtual Usuario? UsuarioNavigation { get; set; }
}
