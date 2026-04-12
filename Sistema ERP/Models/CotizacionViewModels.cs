namespace Sistema_ERP.Models
{
    public class NuevaCotizacionViewModel
    {
        public IEnumerable<Cliente> Clientes { get; set; } = new List<Cliente>();
    }


    public class GuardarCotizacionDto
    {
        public int IdCotizacion { get; set; }
        public int IdCliente { get; set; }
        public string TipoOperacion { get; set; } = "Cotizacion";
        public string EstadoPago { get; set; } = "N/A";
        public decimal PorcentajeImpuesto { get; set; } = 0;
        public int? IdMetodoPago { get; set; }
        public decimal Subtotal { get; set; } = 0;
        public DateTime? FechaLimitePago { get; set; }
        public List<CotizacionDetalleDto> DetallesProductos { get; set; } = new List<CotizacionDetalleDto>();
        public List<CotizacionDetalleDto> DetallesServicios { get; set; } = new List<CotizacionDetalleDto>();
    }

    public class CotizacionDetalleDto
    {
        public int IdItem { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioBase { get; set; }
    }

    public class ClienteDTO
    {
        public string Tipo { get; set; } = null!;
        public string NombreRazonSocial { get; set; } = null!;
        public string? NitCi { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
    }
}
