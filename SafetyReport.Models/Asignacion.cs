namespace SafetyReport.Models
{
    public class AsignacionUsuario
    {
        public int IdUsuarioAsignado { get; set; }
        public int IdRolAsignado { get; set; }
        public int IdEstado { get; set; }
    }

    public class AsignacionCrear
    {
        public List<int> IdsPedido { get; set; } = new();
        public List<AsignacionUsuario> Asignados { get; set; } = new();
    }

    public class AsignacionActualizar
    {
        public int IdUsuarioAsignado { get; set; }
        public int IdRolAsignado { get; set; }
        public int IdEstado { get; set; }
        public List<int> IdsPedido { get; set; } = new();
    }

    public class EliminarAsignacion
    {
        public int IdAsignacion { get; set; }
    }

    public class AsignacionCreada
    {
        public int IdAsignacion { get; set; }
    }

    public class EliminarAsignacionResult
    {
        public int IdAsignacion { get; set; }
    }

    public class AsignacionConsulta
    {
        public int IdAsignacion { get; set; }
        public int IdPedido { get; set; }
        public string CodigoPedido { get; set; } = string.Empty;
        public string? Investigado { get; set; }
        public int IdUsuarioAsignado { get; set; }
        public string NombreUsuarioAsignado { get; set; } = string.Empty;
        public string Iniciales { get; set; } = string.Empty;
        public int IdRolAsignado { get; set; }
        public string? DescripcionRolAsignado { get; set; }
        public int IdEstado { get; set; }
        public string? DescripcionEstado { get; set; }
        public DateTime FechaAsignacion { get; set; }
    }

    public class AsignacionListaConsulta
    {
        public int IdAsignacion { get; set; }
        public int IdPedido { get; set; }
        public string? Cliente { get; set; }
        public string? Investigado { get; set; }
        public string? Analista { get; set; }
        public string? Traductor { get; set; }
        public int IdEstado { get; set; }
        public string? DescripcionEstado { get; set; }
        public string? Vigencia { get; set; }
    }

    public class AsignacionListaResult
    {
        public List<AsignacionListaConsulta> lstAsignaciones { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class FiltroAsignacion
    {
        public string? busqueda { get; set; }
        public int? idEstado { get; set; }
        public int? numPag { get; set; }
    }
}
