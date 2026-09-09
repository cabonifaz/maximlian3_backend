namespace SafetyReport.Application.Comun
{
    public class Roles
    {
        public int IdRol { get; set; }
        public string? Rol { get; set; }
        public string? Descripcion { get; set; }
    }

    public class UsuarioGeneral
    {
        public int IdUsuario { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;
        public int IdEmpresa { get; set; }
        public int IdRol { get; set; }
    }

    public class UsuarioLoginResponse
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public List<Roles> Roles { get; set; } = new();
    }
}
