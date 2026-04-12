using System.ComponentModel.DataAnnotations;

namespace Sistema_ERP.Models
{
    public class EnlaceCompartido
    {
        [Key]
        public int IdEnlace { get; set; }

        [Required]
        [MaxLength(100)]
        public string Token { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaExpiracion { get; set; }

        public bool EstaActivo { get; set; }

        public virtual ICollection<EnlaceProducto> EnlacesProductos { get; set; } = new List<EnlaceProducto>();
        public virtual ICollection<EnlaceServicio> EnlacesServicios { get; set; } = new List<EnlaceServicio>();
    }
}
