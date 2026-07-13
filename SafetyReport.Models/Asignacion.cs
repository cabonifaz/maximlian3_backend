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
        public int IdPedido { get; set; }
        public List<AsignacionUsuario> Asignados { get; set; } = new();
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

    public class AsignacionPersona
    {
        public int? IdAsignacion { get; set; }
        public string? Nombre { get; set; }
    }

    public class AsignacionListaConsulta
    {
        public int IdPedido { get; set; }
        public string? Cliente { get; set; }
        public string? Investigado { get; set; }
        public AsignacionPersona? Analista { get; set; }
        public AsignacionPersona? Traductor { get; set; }
        public int? IdEstado { get; set; }
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

    public class FiltroAsignacionBandeja
    {
        public string? Busqueda { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class AsignacionBandejaItem
    {
        public int? IdPedido { get; set; }
        public string? CodigoPedido { get; set; }
        public int? IdInforme { get; set; }
        public int? IdInformeOriginal { get; set; }
        public string? Investigado { get; set; }
        public string? Pais { get; set; }
        public DateTime? Fecha { get; set; }
        public string? TipoTramite { get; set; }
        public string? EstadoAsignacion { get; set; }
        public int? IdPlantilla { get; set; }
        public int? IdEstado { get; set; }
        public string? Estado { get; set; }
        public string? ColorLetra { get; set; }
        public string? ColorFondo { get; set; }
    }

    public class AsignacionBandejaResumen
    {
        public int Total { get; set; }
        public int EnProceso { get; set; }
        public int Aprobadas { get; set; }
        public int Rechazadas { get; set; }
    }

    public class AsignacionBandejaResult
    {
        public List<AsignacionBandejaItem> lstAsignaciones { get; set; } = new();
        public AsignacionBandejaResumen Resumen { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}
