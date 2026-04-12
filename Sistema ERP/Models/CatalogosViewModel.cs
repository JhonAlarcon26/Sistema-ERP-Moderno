namespace Sistema_ERP.Models
{
    public class CatalogosViewModel
    {
        public IEnumerable<InventarioProducto> Productos { get; set; } = new List<InventarioProducto>();
        public IEnumerable<InventarioServicio> Servicios { get; set; } = new List<InventarioServicio>();
    }
}
