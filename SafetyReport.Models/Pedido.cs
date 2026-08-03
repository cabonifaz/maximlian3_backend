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
        public int IdEmpresaAtencion { get; set; }
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
        public int IdCompania { get; set; }
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

    public class GuardarBorradorFacturaRequest
    {
        public int idTipoDocumentoMaestro { get; set; }
        public string? numeroReferencia { get; set; }
        public int idMonedaMaestro { get; set; }
        public int idTipoOperacionMaestro { get; set; }
        public int idFormaPago { get; set; }
        public List<GuardarBorradorFacturaCuota>? cuotas { get; set; }
        public int idCliente { get; set; }
        public GuardarBorradorFacturaDocumentoAfectado? documentoAfectado { get; set; }
        public List<GuardarBorradorFacturaLinea> lineas { get; set; } = new();
    }

    public class GuardarBorradorFacturaCuota
    {
        public int numeroCuota { get; set; }
        public DateOnly fechaVencimiento { get; set; }
        public decimal monto { get; set; }
    }

    public class GuardarBorradorFacturaDocumentoAfectado
    {
        public int idDocumentoElectronicoRelacionado { get; set; }
        public string tipoReferenciaCodigo { get; set; } = string.Empty;
        public string motivoCodigo { get; set; } = string.Empty;
        public string motivoDescripcion { get; set; } = string.Empty;
    }

    // productoCodigo/descripcion no vienen del front: se resuelven desde el propio Pedido (Codigo/NombreCliente), mismo criterio que idCliente.
    public class GuardarBorradorFacturaLinea
    {
        public int idPedido { get; set; }
        public string? productoSunatCodigo { get; set; }
        public string unidadMedidaCodigo { get; set; } = string.Empty;
        public decimal cantidad { get; set; }
        public decimal valorUnitario { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal montoDescuento { get; set; }
        public string afectacionIgvCodigo { get; set; } = string.Empty;
        public decimal porcentajeIgv { get; set; }
    }

    // Resultado de SP_Facturacion_ObtenerDatosBorrador — exactamente los campos que necesita el payload de facturación.
    public class PedidoParaFacturacionConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? NombreCliente { get; set; }
        public string? NumReferencia { get; set; }
    }

    // Resultado de SP_Facturacion_ObtenerDatosBorrador (PedidoFacturaDAO.ObtenerDatosBorradorAsync).
    public class DatosBorradorFacturaConsulta
    {
        public ClienteParaFacturacionConsulta Cliente { get; set; } = new();
        public List<PedidoParaFacturacionConsulta> Pedidos { get; set; } = new();
    }

    // Query params de SP_Pedido_ListarParaFacturacion.
    public class ListarPedidosFacturacionRequest
    {
        public int idCliente { get; set; }
        public int? idTipoTramite { get; set; }
        public DateOnly? fechaInicio { get; set; }
        public DateOnly? fechaFin { get; set; }
        public int numPag { get; set; } = 1;
    }

    // Resultado de SP_Pedido_ListarParaFacturacion.
    public class PedidoListaFacturacionConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? Investigado { get; set; }
        public string? AplicaPenalidad { get; set; }
        public string? TipoTramite { get; set; }
        public DateTime Fecha { get; set; }
        public decimal? Penalidad { get; set; }
        public decimal? Precio { get; set; }
        public string? DescuentoPorcentaje { get; set; }
    }

    public class PedidoListaFacturacionResult
    {
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public List<PedidoListaFacturacionConsulta> Pedidos { get; set; } = new();
    }

    public class PedidoAsignacionResumen
    {
        public int IdEstadoAsignacion { get; set; }
        public string? DescripcionAsignacion { get; set; }
        public int? IdEstadoInforme { get; set; }
        public string? DescripcionEstadoInforme { get; set; }
    }

    public class PedidoListaConsulta
    {
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public string? Cliente { get; set; }
        public string? Investigado { get; set; }
        public string? Idioma { get; set; }
        public int? RequiereTraduccion { get; set; }
        public string? LogoImprimible { get; set; }
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
        public int Aprobado { get; set; }
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

    public class PedidoEstadoResumenItem
    {
        public int IdEstado { get; set; }
        public string? DescripcionEstado { get; set; }
        public string? ColorLetra { get; set; }
        public string? ColorFondo { get; set; }
        public int Cantidad { get; set; }
    }
}
