using System.Text.Json.Serialization;

namespace SafetyReport.Models
{
    public class PedidoArchivoRequest
    {
        public string NombreDocumento { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public long TamanoArchivo { get; set; }
    }

    public class Pedido
    {
        public string? Codigo { get; set; }
        public int IdCliente { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? NombreCliente { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public string InvestigarRazonSocialNombres { get; set; } = string.Empty;
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
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
        public string InvestigarRazonSocialNombres { get; set; } = string.Empty;
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
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
        public string? InvestigarRazonSocialNombres { get; set; }
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
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
        public string? Busqueda { get; set; }
        public int? IdCliente { get; set; }
        public int? IdEstado { get; set; }
        public int? NumPag { get; set; }
    }

    public class PedidoListaConsulta
    {
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public string? Cliente { get; set; }    
        public string? Investigado { get; set; }
        public int IdIdioma { get; set; }
        public string? Idioma { get; set; }
        public bool LogoImprimible { get; set; }
        public int Estado { get; set; }
        public string? DescripcionEstado { get; set; }
        public string? ColorLetra { get; set; }
        public string? ColorFondo { get; set; }
    }

    public class PedidoListaResult
    {
        public List<PedidoListaConsulta> lstPedido { get; set; } = new();
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