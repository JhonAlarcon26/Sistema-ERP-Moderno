using System.ComponentModel.DataAnnotations;

namespace Sistema_ERP.Models;

public partial class ConfiguracionSmtp
{
    [Key]
    public int IdSmtp { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombrePerfil { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Host { get; set; } = null!;

    [Required]
    public int Port { get; set; } = 587;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Password { get; set; } = null!;

    public bool EnableSsl { get; set; } = true;

    [Required]
    public int Prioridad { get; set; } = 1;

    public bool Activo { get; set; } = true;

    [MaxLength(150)]
    public string? SenderName { get; set; } = "Notificaciones ERP";

    public int? IdUsuario { get; set; }

    public virtual Usuario? UsuarioNavigation { get; set; }
}
