namespace MultiserviciosPiscinas.Models
{
    public class UsuarioListadoItemViewModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class PermisoModuloRolItemViewModel
    {
        public string Clave { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public bool Admin { get; set; }
        public bool Tecnico { get; set; }
        public bool Cliente { get; set; }
    }

    public class RolesIndexViewModel
    {
        public List<Rol> Roles { get; set; } = new();
        public List<UsuarioListadoItemViewModel> Usuarios { get; set; } = new();
        public List<PermisoModuloRolItemViewModel> MatrizPermisos { get; set; } = new();
        public DateTime? FechaModificacionFiltro { get; set; }
    }
}
