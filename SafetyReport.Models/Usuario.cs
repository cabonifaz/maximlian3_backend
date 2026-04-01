namespace SafetyReport.Models
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

    public class UsuarioCrear
    {
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string Email { get; set; } = string.Empty;
        public string usuarioCreacion {get; set;} = string.Empty;
        public List<int> Roles { get; set; } = new();
        public List<int> Idiomas { get; set; } = new();
    }

    public class InfoUsuarioEditar
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<int> Roles { get; set; } = new();
        public List<int> Idiomas { get; set; } = new();
    }

    public class UsuarioCreado
    {
        public int IdUsuario { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class UsuarioConsulta
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public List<int> Roles { get; set; } = new();
        public List<int> Idiomas { get; set; } = new();
    }

    public class UsuarioListaResult
    {
        public List<UsuarioListaConsulta> lstUsuarios { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class UsuarioListaConsulta
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Roles { get; set; }
        public string? Estado { get; set; }
    }

    public class UsuarioLoginResponse
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public List<Roles> Roles { get; set; } = new();
    }

    public class EliminarUsuario
    {
        public int IdUsuarioEliminar { get; set; }
    }

    public class EliminarUsuarioResult
    {
        public int IdUsuarioEliminar { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class FiltroUsuario
    {
        public int? numPag { get; set; }
        public string? Filtro { get; set; }
    }

    public class UsuarioListaCortaItem
    {
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}