namespace Sistema_ERP.Models
{
    public class DashboardRentabilidadViewModel
    {
        public decimal IngresosTotalesBrutos { get; set; }
        public decimal CostoTotalVentas { get; set; }
        public decimal GananciaNeta { get; set; }
        public decimal MargenPorcentaje => IngresosTotalesBrutos > 0 ? (GananciaNeta / IngresosTotalesBrutos) * 100 : 0;

        public List<VentaDetalleReporteDto> DesgloseVentas { get; set; } = new List<VentaDetalleReporteDto>();
    }

    public class VentaDetalleReporteDto
    {
        public int IdCotizacion { get; set; }
        public string Cliente { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string ItemsVendidos { get; set; } = "";
        public decimal IngresoBruto { get; set; }
        public decimal CostoBase { get; set; }
        public decimal Ganancia => IngresoBruto - CostoBase;
        public decimal Margen => IngresoBruto > 0 ? (Ganancia / IngresoBruto) * 100 : 0;
    }

    public class ReporteRentabilidadViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = null!;
        public decimal PrecioVentaConFactura { get; set; }
        public decimal CostoPromedio { get; set; }
    }



    public class ReporteCompraViewModel
    {
        public decimal TotalGasto { get; set; }
        public int CantidadCompras { get; set; }
        public string ProveedorPrincipal { get; set; } = "";
        public decimal GastoPromedioPorCompra => CantidadCompras > 0 ? TotalGasto / CantidadCompras : 0;

        public List<CompraDetalleDto> Detalles { get; set; } = new();
        public List<ChartDataDto> ChartData { get; set; } = new();
        public string Periodo { get; set; } = "Mes";
    }

    public class CompraDetalleDto
    {
        public int IdCompra { get; set; }
        public string Producto { get; set; } = "";
        public string Proveedor { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Total => Cantidad * CostoUnitario;
    }

    public class ReporteVentaViewModel
    {
        public decimal TotalIngresos { get; set; }
        public decimal TotalCostos { get; set; }
        public decimal GananciaTotal => TotalIngresos - TotalCostos;
        public decimal MargenPromedio => TotalIngresos > 0 ? (GananciaTotal / TotalIngresos) * 100 : 0;

        public List<VentaDetalleDto> Detalles { get; set; } = new();
        public List<ChartDataDto> ChartData { get; set; } = new();
        public string Periodo { get; set; } = "Mes";
    }

    public class VentaDetalleDto
    {
        public int IdVenta { get; set; }
        public string Cliente { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = "";
        public decimal ImporteBruto { get; set; }
        public decimal CostoEstimado { get; set; }
        public decimal Margen => ImporteBruto - CostoEstimado;
    }

    public class ReporteStockViewModel
    {
        public decimal ValorTotalInventario { get; set; }
        public int ItemsEnCritico { get; set; }
        public int TotalProductos { get; set; }
        public string CategoriaDominante { get; set; } = "";

        public List<StockDetalleDto> Detalles { get; set; } = new();
        public List<ChartDataDto> ChartData { get; set; } = new();
    }

    public class StockDetalleDto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public int Cantidadactual { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal ValorInventario => Cantidadactual * PrecioVenta;
        public string Estado => Cantidadactual <= StockMinimo ? "Crítico" : (Cantidadactual <= StockMinimo * 1.5 ? "Bajo" : "Normal");
    }

    public class ChartDataDto
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
        public decimal? SecondaryValue { get; set; }
    }
}
