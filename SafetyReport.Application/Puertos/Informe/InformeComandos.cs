using SafetyReport.Application.Puertos.InformeLocalImagen;

namespace SafetyReport.Application.Puertos.Informe
{
    public class InformeCrear
    {
        public int? IdPedido { get; set; }
        public int? IdTipoPersona { get; set; }
        public string? Nombre { get; set; }
        public string? NombreComercial { get; set; }
        public int? IdPais { get; set; }
        public int? OperacionesTCMoneda { get; set; }
        public int? TaxIdType { get; set; }
        public string? TaxNum { get; set; }
        public string? Direccion { get; set; }
        public string? Ubigeo { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Telefono { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? PaginaWeb { get; set; }
        public int? IdEstadoManual { get; set; }
        public int? IdEstadoInforme { get; set; }
        public string? DatosAdicionales { get; set; }
        public string? ObservacionesIdentificacion { get; set; }
        public int? IdTipoEmpresa { get; set; }
        public DateTime? FechaConstitucion { get; set; }
        public int? IdCiudadRegistro { get; set; }
        public string? IdNotaria { get; set; }
        public string? IdNotario { get; set; }
        public string? IdRegistro { get; set; }
        public string? IdPlazo { get; set; }
        public int? IdOperacionesCambioDivisas { get; set; }
        public decimal? CapitalInicial { get; set; }
        public decimal? CapitalPagado { get; set; }
        public DateTime? FechaUltimoIncremento { get; set; }
        public int? IdTipoIncremento { get; set; }
        public decimal? PatrimonioNeto { get; set; }
        public string? TipoAcciones { get; set; }
        public decimal? ValorAcciones { get; set; }
        public bool? CotizaBolsa { get; set; }
        public decimal? TipoCambio { get; set; }
        public int? IdTipoCambio { get; set; }
        public string? Antecedentes { get; set; }
        public string? AspectosLegales { get; set; }
        public string? ComentariosAspectoLegal { get; set; }
        public int? IdSector { get; set; }
        public string? Actividad { get; set; }
        public int? IdIsicCategoria { get; set; }
        public int? IdIsicClase { get; set; }
        public string? ActividadPrincipal { get; set; }
        public decimal? VentasContado { get; set; }
        public string? VentasContadoText { get; set; }
        public decimal? VentasCredito { get; set; }
        public string? VentasCreditoText { get; set; }
        public int? IdVentasCreditoTiempo { get; set; }
        public decimal? VentasInternacionales { get; set; }
        public string? VentasInternacionalesText { get; set; }
        public decimal? VentasNacionales { get; set; }
        public string? VentasNacionalesText { get; set; }
        public decimal? ComprasNacionales { get; set; }
        public string? ComprasNacionalesText { get; set; }
        public decimal? ComprasInternacionales { get; set; }
        public string? ComprasInternacionalesText { get; set; }
        public decimal? ComprasContadoNacionales { get; set; }
        public string? ComprasContadoNacionalesText { get; set; }
        public decimal? ComprasCreditoNacionales { get; set; }
        public string? ComprasCreditoNacionalesText { get; set; }
        public int? IdComprasCreditoNacionalesTiempo { get; set; }
        public decimal? ComprasContadoInternacionales { get; set; }
        public string? ComprasContadoInternacionalesText { get; set; }
        public decimal? ComprasCreditoInternacionales { get; set; }
        public string? ComprasCreditoInternacionalesText { get; set; }
        public int? IdComprasCreditoInternacionalesTiempo { get; set; }
        public int? NumeroEmpleados { get; set; }
        public string? NumeroEmpleadosText { get; set; }
        public string? ComentariosOperaciones { get; set; }
        public string? ContenidoInformacionFinanciera { get; set; }
        public string? ComentarioInformacionFinanciera { get; set; }
        public string? ActivosFijos { get; set; }
        public string? Seguros { get; set; }
        public string? ComentarioProveedor { get; set; }
        public string? ReferenciaBanco { get; set; }
        public string? Litigios { get; set; }
        public string? RiesgoPrincipal { get; set; }
        public string? Superintendecia { get; set; }
        public string? InformacionGeneral { get; set; }
        public string? OpinionCredito { get; set; }
        public bool? FlgTieneInformacion { get; set; }
        public int? IdFormatoFecha { get; set; }

        // Child lists
        public List<InformeBalanceItem> lstBalances { get; set; } = new();
        public List<InformeBalanceDesagregadoItem> lstBalancesDesagregado { get; set; } = new();
        public List<InformeBalanceTotalizadoItem> lstBalancesTotalizado { get; set; } = new();
        public List<InformeBalanceBancoItem> lstBalancesBanco { get; set; } = new();
        public List<InformeBalanceSeguroItem> lstBalancesSeguro { get; set; } = new();
        public List<InformeBalanceTurquiaItem> lstBalancesTurquia { get; set; } = new();
        public List<InformeBancoItem> lstBancos { get; set; } = new();
        public List<InformeCompaniaRelacionadaItem> lstCompaniasRelacionadas { get; set; } = new();
        public List<InformeExportacionImportacionItem> lstExportacionesImportaciones { get; set; } = new();
        public List<InformeProveedorItem> lstProveedores { get; set; } = new();
        public List<InformeDirectorioEjecutivoItem> lstDirectoriosEjecutivos { get; set; } = new();
        public List<InformeLocalItem> lstLocales { get; set; } = new();
    }

    public class InformeEditar : InformeCrear
    {
        public int IdInforme { get; set; }
    }

    public class InformeCreado
    {
        public int IdInforme { get; set; }
        public List<InformeLocalImagenPendiente> ImagenesPendientes { get; set; } = new();
    }

    public class InformeEliminado
    {
        public int IdInforme { get; set; }
    }

    public class InformeIdRequest
    {
        public int IdInforme { get; set; }
    }

    public class InformeDocumentoResult
    {
        public string UrlDocumento { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class InformeActualizarEstadoRequest
    {
        public int IdInforme { get; set; }
        public int IdEstadoInforme { get; set; }
    }

    public class NotificacionInformeDatosConsulta
    {
        public string? Correo { get; set; }
        public int IdPedido { get; set; }
        public string CodigoPedido { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string CuerpoHtml { get; set; } = string.Empty;
        public List<string> Formatos { get; set; } = new();
    }

    public class InformeIdResult
    {
        public int IdInforme { get; set; }
    }
}
