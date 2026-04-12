namespace Sistema_ERP.Models
{
    public class RolPermisoViewModel
    {
        public Role Rol { get; set; } = null!;
        public List<PermisoCheckbox> Permisos { get; set; } = new();
    }

    public class PermisoCheckbox
    {
        public int IdPermiso { get; set; }
        public string NombrePermiso { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Seleccionado { get; set; }
    }
}
