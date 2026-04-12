namespace Sistema_ERP.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Tipo { get; set; } = null!;

    public string NombreRazonSocial { get; set; } = null!;

    public string? NitCi { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Latitud { get; set; }
    public string? Longitud { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();
    public virtual ICollection<VisitaTecnica> VisitasTecnicas { get; set; } = new List<VisitaTecnica>();
}
