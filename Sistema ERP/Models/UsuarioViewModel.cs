using System.ComponentModel.DataAnnotations;

namespace Sistema_ERP.Models
{
    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = null!;

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }



        [Required(ErrorMessage = "El rol es obligatorio")]
        public int IdRol { get; set; }

        [Display(Name = "Estado (Activo)")]
        public bool Estado { get; set; } = true;
    }
}
