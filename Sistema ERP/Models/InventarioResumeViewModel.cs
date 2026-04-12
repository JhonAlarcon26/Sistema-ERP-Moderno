namespace Sistema_ERP.Models
{
    public class InventarioResumeViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = null!;
        public int StockActual { get; set; }
        public decimal CostoCompraPromedio { get; set; }
        public decimal ValorTotalStock { get; set; }
    }
}
