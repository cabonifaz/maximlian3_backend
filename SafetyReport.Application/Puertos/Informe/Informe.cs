using System.Text.Json;
using SafetyReport.Application.Puertos.InformeArchivo;
using SafetyReport.Application.Puertos.InformeLocalImagen;

namespace SafetyReport.Application.Puertos.Informe
{
    // ── Child input items ────────────────────────────────────────────────────────

    public class InformeLocalItem
    {
        public int? IdInformeLocal { get; set; }
        public int? IdTipoLocal { get; set; }
        public string? Comentario { get; set; }
        public List<InformeLocalImagenItem> Imagenes { get; set; } = new();
    }

    public class InformeBancoItem
    {
        public int? IdInformeBanco { get; set; }
        public int IdBanco { get; set; }
        public string? NumeroCuenta { get; set; }
        public int? IdSector { get; set; }
        public string? Sectorista { get; set; }
        public string? ReferenciaBanco { get; set; }
    }

    public class InformeCompaniaRelacionadaItem
    {
        public int? IdInformeCompaniaRelacionada { get; set; }
        public int IdCompania { get; set; }
    }

    public class InformeExportacionImportacionItem
    {
        public int? IdInformeExportacionImportacion { get; set; }
        public int Anio { get; set; }
        public int MesInicio { get; set; }
        public int MesFin { get; set; }
        public int IdMoneda { get; set; }
        public string? Paises { get; set; }
        public decimal? Monto { get; set; }
        public string? Productos { get; set; }
        public int IdTipoOperacion { get; set; }
        public int? NumOperaciones { get; set; }
    }

    public class InformeProveedorItem
    {
        public int? IdInformeProveedor { get; set; }
        public int? IdBancoProveedor { get; set; }
        public int IdTipoPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? IdPais { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int? IdMoneda { get; set; }
        public DateTime? FechaInicio { get; set; }
        public int? IdLimiteCredito { get; set; }
        public decimal? PromedioMensual { get; set; }
        public string? PlazoCredito { get; set; }
        public string? Productos { get; set; }
        public int? IdCalificacion { get; set; }
        public string? Comentarios { get; set; }
        public string? NombreContacto { get; set; }
        public string? Telefono { get; set; }
        public string? ComienzoNegociaciones { get; set; }
        public int? IdPlazoCredito { get; set; }
        public bool? EsTieneReferenciaComercial { get; set; }
        public decimal? TipoCambio { get; set; }
    }

    public class InformeDirectorioEjecutivoItem
    {
        public int? IdInformeDirectorioEjecutivo { get; set; }
        public int IdDirectorioEjecutivo { get; set; }
        public int? IdCargo { get; set; }
        public DateTime? VinculadoDesde { get; set; }
        public string? CompaniaAnterior { get; set; }
        public decimal? Participacion { get; set; }
        public int? Orden { get; set; }
        public bool? EsParticipanteDirectiva { get; set; }
        public bool? ApareceImpresoLista { get; set; }
        public bool? ImprimeDatosEjecutivos { get; set; }
    }

    // ── Create / Edit ────────────────────────────────────────────────────────────

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

    // ── Response / Result models ─────────────────────────────────────────────────

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

    // ── Filters ──────────────────────────────────────────────────────────────────

    public class FiltroInformeObtener
    {
        public int IdPedido { get; set; }
        public int IdInforme { get; set; }
    }

    public class FiltroInforme
    {
        public string? Busqueda { get; set; }
        public int? IdPedido { get; set; }
        public string? IdEstado { get; set; }
        public string? IdPlantilla { get; set; }
        public string? IdTipoTramite { get; set; }
        public int? NumPag { get; set; }
    }

    // ── List ─────────────────────────────────────────────────────────────────────

    public class InformeListaConsulta
    {
        public int IdInforme { get; set; }
        public int IdPedido { get; set; }
        public string? CodigoPedido { get; set; }
        public string? Cliente { get; set; }
        public int? IdFase { get; set; }
        public string? Plantilla { get; set; }
        public string? EstadoInforme { get; set; }
        public string? Investigado { get; set; }
        public string? Vigencia { get; set; }
        public string? TipoTramite { get; set; }
        public int? IdInformeOriginal { get; set; }
        public int? RequiereTraduccion { get; set; }
    }

    public class InformeListaResult
    {
        public List<InformeListaConsulta> lstInformes { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int Asignado { get; set; }
        public int Rechazado { get; set; }
        public int EnProceso { get; set; }
        public int Aprobado { get; set; }
        public int PendienteAprobacion { get; set; }
        public int Vigente { get; set; }
        public int Vencido { get; set; }
    }

    public class FiltroInformeIdPorCompania
    {
        public int IdCompania { get; set; }
        public DateTime? FchInicio { get; set; }
        public DateTime? FchFin { get; set; }
        public int? NumPag { get; set; }
    }

    public class InformeIdPorCompaniaConsulta
    {
        public int IdInforme { get; set; }
        public int IdPedido { get; set; }
        public string? Idioma { get; set; }
        public string? Nombre { get; set; }
        public string? Fecha { get; set; }
    }

    public class InformeIdPorCompaniaListaResult
    {
        public List<InformeIdPorCompaniaConsulta> lstInformes { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    // ── Detail (Obtener) ─────────────────────────────────────────────────────────

    public class InformeDirectorioEjecutivoConsulta
    {
        // De INFORME_DIRECTORIO_EJECUTIVO
        public int IdInformeDirectorioEjecutivo { get; set; }
        public int? IdCargo { get; set; }
        public DateTime? VinculadoDesde { get; set; }
        public string? CompaniaAnterior { get; set; }
        public decimal? Participacion { get; set; }
        public int? Orden { get; set; }
        public bool? EsParticipanteDirectiva { get; set; }
        public bool? ApareceImpresoLista { get; set; }
        public bool? ImprimeDatosEjecutivos { get; set; }
        // De DIRECTORIO_EJECUTIVO
        public int IdDirectorioEjecutivo { get; set; }
        public int? IdTipoPersona { get; set; }
        public string? NombreCompleto { get; set; }
        public int? IdPais { get; set; }
        public string? Direccion { get; set; }
        public string? Ubigeo { get; set; }
        public string? CodigoPostal { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int? TaxIdType { get; set; }
        public string? TaxNum { get; set; }
        public int? IdNacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int? IdEstadoCivil { get; set; }
        public int? IdProfesion { get; set; }
        public string? Referencias { get; set; }
    }

    public class InformeLocalConsulta
    {
        public int IdInformeLocal { get; set; }
        public int? IdTipoLocal { get; set; }
        public string? Comentario { get; set; }
        public List<InformeLocalImagenConsulta> Imagenes { get; set; } = new();
    }

    public class InformeBancoConsulta
    {
        public int IdIformeBanco { get; set; }
        public int IdBanco { get; set; }
        public string? NumeroCuenta { get; set; }
        public int? IdSector { get; set; }
        public string? Sectorista { get; set; }
        public string? ReferenciaBanco { get; set; }
    }

    public class InformeCompaniaRelacionadaConsulta
    {
        public int IdInformeCompaniaRelacionada { get; set; }
        public int IdCompania { get; set; }
    }

    public class InformeExportacionImportacionConsulta
    {
        public int IdInformeExportacionImportacion { get; set; }
        public int Anio { get; set; }
        public int MesInicio { get; set; }
        public int MesFin { get; set; }
        public int IdMoneda { get; set; }
        public string? Paises { get; set; }
        public decimal? Monto { get; set; }
        public string? Productos { get; set; }
        public int IdTipoOperacion { get; set; }
        public int? NumOperaciones { get; set; }
    }

    public class InformeProveedorConsulta
    {
        public int IdInformeProveedor { get; set; }
        public int? IdBancoProveedor { get; set; }
        public int IdTipoPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? IdPais { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int? IdMoneda { get; set; }
        public DateTime? FechaInicio { get; set; }
        public int? IdLimiteCredito { get; set; }
        public decimal? PromedioMensual { get; set; }
        public string? PlazoCredito { get; set; }
        public string? Productos { get; set; }
        public int? IdCalificacion { get; set; }
        public string? Comentarios { get; set; }
        public string? NombreContacto { get; set; }
        public string? Telefono { get; set; }
        public string? ComienzoNegociaciones { get; set; }
        public int? IdPlazoCredito { get; set; }
        public bool? EsTieneReferenciaComercial { get; set; }
        public decimal? TipoCambio { get; set; }
    }

    public class InformeConsulta
    {
        public int IdInforme { get; set; }
        public int IdPedido { get; set; }
        public int? IdIdioma { get; set; }
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
        public int? IdEstadoInforme { get; set; }
        public int? IdFormatoFecha { get; set; }

        // Child arrays
        public List<InformeBalanceConsulta> Balances { get; set; } = new();
        public List<InformeBancoConsulta> Bancos { get; set; } = new();
        public List<InformeCompaniaRelacionadaConsulta> CompaniasRelacionadas { get; set; } = new();
        public List<InformeExportacionImportacionConsulta> ExportacionesImportaciones { get; set; } = new();
        public List<InformeProveedorConsulta> Proveedores { get; set; } = new();
        public List<InformeDirectorioEjecutivoConsulta> DirectoriosEjecutivos { get; set; } = new();
        public List<InformeLocalConsulta> Locales { get; set; } = new();
        public List<InformeArchivoResumen> Archivos { get; set; } = new();
    }

    public class InformeUrlPrefirmadaRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
    }

    public class InformeAutocompletar
    {
        public string FileKey { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public object? Secciones { get; set; }
        public string? Prompt { get; set; }
    }

    public class InformeUrlPrefirmada
    {
        public string UploadUrl { get; set; } = string.Empty;
        public string FileKey { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}
