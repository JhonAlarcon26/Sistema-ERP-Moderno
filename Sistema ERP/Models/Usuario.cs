namespace Sistema_ERP.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool Estado { get; set; }
    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual ICollection<InventarioCompra> InventarioCompras { get; set; } = new List<InventarioCompra>();
    public virtual ICollection<VisitaTecnica> VisitasTecnicas { get; set; } = new List<VisitaTecnica>();
    public virtual ICollection<ConfiguracionApi> ConfiguracionesApi { get; set; } = new List<ConfiguracionApi>();
    public virtual ICollection<ConfiguracionSmtp> ConfiguracionesSmtp { get; set; } = new List<ConfiguracionSmtp>();
}
