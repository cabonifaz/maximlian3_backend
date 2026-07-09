using System.Text.Json.Serialization;

namespace SafetyReport.Models
{
    public class PedidoArchivoRequest
    {
        public string NombreDocumento { get; set; } = string.Empty;
        public string FormatoArchivo { get; set; } = string.Empty;
        public long TamanoArchivo { get; set; }
        public int IdTipoArchivo { get; set; }
    }

    public class Pedido
    {
        public string? Codigo { get; set; }
        public int IdCliente { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCliente { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public string? NumeroDocumentoInvestigado { get; set; }
        public string InvestigarRazonSocialNombres { get; set; } = string.Empty;
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public int? IdTipoPlazoCredito { get; set; }
        public string? TipoPlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
        public bool ImprimeLogoSafety { get; set; }
        public List<PedidoArchivoRequest> Archivos { get; set; } = new();
    }

    public class EditarPedido
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCliente { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public string? NumeroDocumentoInvestigado { get; set; }
        public string InvestigarRazonSocialNombres { get; set; } = string.Empty;
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public int? IdTipoPlazoCredito { get; set; }
        public string? TipoPlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
        public bool ImprimeLogoSafety { get; set; }
    }

    public class PedidoConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCliente { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public string? NumeroDocumentoInvestigado { get; set; }
        public string? InvestigarRazonSocialNombres { get; set; }
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public int? IdTipoPlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
        public bool ImprimeLogoSafety { get; set; }
    }

    public class PedidoCreado
    {
        public int IdPedido { get; set; }
        public string RutaBaseArchivos { get; set; } = string.Empty;
    }

    public class PedidoCreadoResponse
    {
        public int IdPedido { get; set; }
        public List<PedidoArchivoPresignado> Archivos { get; set; } = new();
    }

    public class PedidoEliminado
    {
        public int IdPedido { get; set; }
    }

    public class PedidoIdRequest
    {
        public int IdPedido { get; set; }
    }

    public class FiltroPedido
    {
        public string? busqueda { get; set; }
        public int? idCliente { get; set; }
        public string? idEstado { get; set; }
        public int? numPag { get; set; }
    }

    public class FiltroPedidoObtener
    {
        public int? idPedido { get; set; }
        public int? idCliente { get; set; }
        public int? idTarifario { get; set; }
        public string? nombreInvestigado { get; set; }
        public string? numRef { get; set; }
        public List<int>? idEstado { get; set; }
    }

    public class PedidoAsignacionResumen
    {
        public int IdEstadoAsignacion { get; set; }
        public int? IdEstadoInforme { get; set; }
        public string? Descripcion { get; set; }
    }

    public class PedidoListaConsulta
    {
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public string? Cliente { get; set; }
        public string? Investigado { get; set; }
        public int IdIdioma { get; set; }
        public string? Idioma { get; set; }
        public int? RequiereTraduccion { get; set; }
        public bool LogoImprimible { get; set; }
        public int Estado { get; set; }
        public string? DescripcionEstado { get; set; }
        public string? ColorLetra { get; set; }
        public string? ColorFondo { get; set; }
        public List<PedidoAsignacionResumen>? Asignaciones { get; set; }
        public string? FechaMod { get; set; }
    }

    public class PedidoListaResult
    {
        public List<PedidoListaConsulta> lstPedido { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int Pendiente { get; set; }
        public int EnRevision { get; set; }
        public int Aprobado { get; set; }
        public int Observado { get; set; }
        public int Cancelado { get; set; }
    }

    public class FiltroPedidoAsignacion
    {
        public string? busqueda { get; set; }
        public int? idPedido { get; set; }
        public string? idEstado { get; set; }
        public int? idEstadoAsignacion { get; set; }
        public int? numPag { get; set; }
    }

    public class PedidoAsignacionListaConsulta
    {
        public int IdPedido { get; set; }
        public string? Nombre { get; set; }
        public string? Investigado { get; set; }
        public string? Idioma { get; set; }
        public string? TipoTramite { get; set; }
        public int? DiasMin { get; set; }
        public int? DiasMax { get; set; }
        public string? Vigencia { get; set; }
        public List<PedidoAsignacionResumen>? Asignaciones { get; set; }
    }

    public class PedidoAsignacionListaResult
    {
        public List<PedidoAsignacionListaConsulta> lstPedido { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class PedidoArchivoPresignado
    {
        public string NombreDocumento { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public string UploadUrl { get; set; } = string.Empty;
    }

    public class PedidoCreadoConArchivos
    {
        public int IdPedido { get; set; }
        public List<PedidoArchivoPresignado> Archivos { get; set; } = new();
    }
}
