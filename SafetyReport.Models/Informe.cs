using System.Text.Json;

namespace SafetyReport.Models
{
    // ── Child input items ────────────────────────────────────────────────────────

    public class InformeBalanceItem
    {
        public int? IdInformeBalance { get; set; }
        public DateTime FechaBalance { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool FlgActualidad { get; set; }
        public decimal? TipoCambio { get; set; }
        public int IdMoneda { get; set; }
        public int IdTipoBalance { get; set; }
        public int? IdTipoEstadoFinanciero { get; set; }
    }

    public class InformeBalanceDesagregadoItem
    {
        public int Id { get; set; }
        public decimal? EfectivoEquivalente { get; set; }
        public decimal? OtrosActivosFinancierosCorriente { get; set; }
        public decimal? CuentasCobrarCorriente { get; set; }
        public decimal? InventariosCorriente { get; set; }
        public decimal? ActivosBiologicosCorriente { get; set; }
        public decimal? ActivosImpuestosGanancias { get; set; }
        public decimal? OtrosActivosNoFinancierosCorriente { get; set; }
        public decimal? TotalActivoCorriente { get; set; }
        public decimal? OtrosActivosFinancierosNoCorriente { get; set; }
        public decimal? InversionesSubsidiarias { get; set; }
        public decimal? CuentasCobrarNoCorriente { get; set; }
        public decimal? InventariosNoCorriente { get; set; }
        public decimal? ActivosBiologicosNoCorriente { get; set; }
        public decimal? PropiedadesInversion { get; set; }
        public decimal? PropiedadesPlantaEquipo { get; set; }
        public decimal? Intangibles { get; set; }
        public decimal? ActivosImpuestosDiferidos { get; set; }
        public decimal? ActivosImpuestosCorrientes { get; set; }
        public decimal? Plusvalia { get; set; }
        public decimal? OtrosActivosNoFinancierosNoCorriente { get; set; }
        public decimal? TotalActivoNoCorriente { get; set; }
        public decimal? TotalActivo { get; set; }
        public decimal? OtrosPasivosFinancierosCorriente { get; set; }
        public decimal? CuentasPagarCorriente { get; set; }
        public decimal? BeneficiosEmpleadosCorriente { get; set; }
        public decimal? OtrasProvisionesCorriente { get; set; }
        public decimal? ImpuestosGananciasCorriente { get; set; }
        public decimal? OtrosPasivosNoFinancierosCorriente { get; set; }
        public decimal? TotalPasivoCorriente { get; set; }
        public decimal? OtrosPasivosFinancierosNoCorriente { get; set; }
        public decimal? CuentasPagarNoCorriente { get; set; }
        public decimal? BeneficiosEmpleadosNoCorriente { get; set; }
        public decimal? OtrasProvisionesNoCorriente { get; set; }
        public decimal? ImpuestosDiferidosNoCorriente { get; set; }
        public decimal? ImpuestosCorrientesNoCorriente { get; set; }
        public decimal? OtrosPasivosNoFinancierosNoCorriente { get; set; }
        public decimal? TotalPasivoNoCorriente { get; set; }
        public decimal? TotalPasivos { get; set; }
        public decimal? CapitalEmitido { get; set; }
        public decimal? PrimasEmision { get; set; }
        public decimal? AccionesInversion { get; set; }
        public decimal? AccionesCartera { get; set; }
        public decimal? OtrasReservasCapital { get; set; }
        public decimal? ResultadosAcumulados { get; set; }
        public decimal? OtrasReservasPatrimonio { get; set; }
        public decimal? TotalPatrimonio { get; set; }
        public decimal? TotalPasivoPatrimonio { get; set; }
        public decimal? IngresosOrdinarios { get; set; }
        public decimal? CostoVentas { get; set; }
        public decimal? GananciaBruta { get; set; }
        public decimal? GastosVentas { get; set; }
        public decimal? GastosAdministracion { get; set; }
        public decimal? OtrosIngresosOperativos { get; set; }
        public decimal? OtrosGastosOperativos { get; set; }
        public decimal? OtrasGananciasPerdidas { get; set; }
        public decimal? GananciaOperativa { get; set; }
        public decimal? IngresosFinancieros { get; set; }
        public decimal? IngresosIntereses { get; set; }
        public decimal? GastosFinancieros { get; set; }
        public decimal? DeterioroValor { get; set; }
        public decimal? OtrosIngresosSubsidiarias { get; set; }
        public decimal? DiferenciasCambio { get; set; }
        public decimal? GananciaAntesImpuestos { get; set; }
        public decimal? IngresoGastoImpuesto { get; set; }
        public decimal? OperacionesDescontinuadas { get; set; }
        public decimal? GananciaNeta { get; set; }
        public decimal? IndiceLiquidez { get; set; }
        public decimal? CapitalTrabajo { get; set; }
        public decimal? RatioEndeudamiento { get; set; }
        public decimal? RatioRentabilidad { get; set; }
    }

    public class InformeBalanceTotalizadoItem
    {
        public int Id { get; set; }
        public decimal? TotalActivoCorriente { get; set; }
        public decimal? TotalActivoNoCorriente { get; set; }
        public decimal? TotalActivo { get; set; }
        public decimal? TotalPasivoCorriente { get; set; }
        public decimal? TotalPasivoNoCorriente { get; set; }
        public decimal? TotalPasivos { get; set; }
        public decimal? TotalPatrimonio { get; set; }
        public decimal? TotalPasivoPatrimonio { get; set; }
        public decimal? IngresosOrdinarios { get; set; }
        public decimal? GananciaNeta { get; set; }
        public decimal? IndiceLiquidez { get; set; }
        public decimal? CapitalTrabajo { get; set; }
        public decimal? RatioEndeudamiento { get; set; }
        public decimal? RatioRentabilidad { get; set; }
    }

    public class InformeBalanceBancoItem
    {
        public int Id { get; set; }
        public decimal? Disponible { get; set; }
        public decimal? FondosInterbancarios { get; set; }
        public decimal? InversionesValorRazonable { get; set; }
        public decimal? CarteraCreditos { get; set; }
        public decimal? DerivadosNegociacionActivo { get; set; }
        public decimal? DerivadosCoberturaActivo { get; set; }
        public decimal? BienesRealizables { get; set; }
        public decimal? ParticipacionesSubsidiarias { get; set; }
        public decimal? InmuebleMobiliarioEquipo { get; set; }
        public decimal? ImpuestoRentaDiferido { get; set; }
        public decimal? OtrosActivos { get; set; }
        public decimal? TotalActivos { get; set; }
        public decimal? ObligacionesPublico { get; set; }
        public decimal? FondosInterbancariosPasivo { get; set; }
        public decimal? AdeudosFinancieras { get; set; }
        public decimal? DerivadosNegociacionPasivo { get; set; }
        public decimal? DerivadosCoberturaPasivo { get; set; }
        public decimal? CuentasPagarProvisiones { get; set; }
        public decimal? TotalPasivo { get; set; }
        public decimal? CapitalSocial { get; set; }
        public decimal? Reservas { get; set; }
        public decimal? ResultadosNoRealizados { get; set; }
        public decimal? ResultadoEjercicio { get; set; }
        public decimal? TotalPatrimonio { get; set; }
        public decimal? TotalPasivoPatrimonio { get; set; }
        public decimal? IngresosIntereses { get; set; }
        public decimal? UtilidadEjercicio { get; set; }
    }

    public class InformeBalanceSeguroItem
    {
        public int Id { get; set; }
        public decimal? EfectivoDisponible { get; set; }
        public decimal? InversionesFinancieras { get; set; }
        public decimal? PrestamosInteresesNetos { get; set; }
        public decimal? PrimasCobrar { get; set; }
        public decimal? DeudasReaseguradores { get; set; }
        public decimal? ActivosVenta { get; set; }
        public decimal? PropiedadesInversion { get; set; }
        public decimal? PropiedadPlantaEquipo { get; set; }
        public decimal? OtrosActivos { get; set; }
        public decimal? TotalActivos { get; set; }
        public decimal? ObligacionesAsegurados { get; set; }
        public decimal? ReservasSiniestros { get; set; }
        public decimal? ReservasTecnicas { get; set; }
        public decimal? ObligacionesReaseguradores { get; set; }
        public decimal? ObligacionesFinancieras { get; set; }
        public decimal? CuentasPagar { get; set; }
        public decimal? OtrosPasivos { get; set; }
        public decimal? TotalPasivo { get; set; }
        public decimal? CapitalSocial { get; set; }
        public decimal? AportesCapitalNoCapitalizados { get; set; }
        public decimal? ResultadosAcumulados { get; set; }
        public decimal? PatrimonioRestringido { get; set; }
        public decimal? TotalPatrimonio { get; set; }
        public decimal? TotalPasivoPatrimonio { get; set; }
        public decimal? PrimasGanadasNetas { get; set; }
        public decimal? UtilidadNeta { get; set; }
    }

    public class InformeBalanceTurquiaItem
    {
        public int Id { get; set; }
        public int? Ano { get; set; }
        public DateTime? FechaBalance { get; set; }
        public int? IdMoneda { get; set; }
        public int? DuracionPeriodo { get; set; }
        public int? IdNivelConfiabilidad { get; set; }
        public decimal? TipoCambio { get; set; }
        public decimal? Efectivo { get; set; }
        public decimal? Existencias { get; set; }
        public decimal? Deudores { get; set; }
        public decimal? TotalCorriente { get; set; }
        public decimal? BienesTongibles { get; set; }
        public decimal? ActivosIntangibles { get; set; }
        public decimal? ActivoFijoNeto { get; set; }
        public decimal? TotalActivos { get; set; }
        public decimal? Prestamos { get; set; }
        public decimal? Acreedores { get; set; }
        public decimal? PasivosCorrientes { get; set; }
        public decimal? PasivosNoCorrientes { get; set; }
        public decimal? PasivosLargoPlazo { get; set; }
        public decimal? TotalPasivosNoCorrientes { get; set; }
        public decimal? TotalPasivos { get; set; }
        public decimal? Capital { get; set; }
        public decimal? Reservas { get; set; }
        public decimal? ResultadosAcumulados { get; set; }
        public decimal? ResultadoEjercicio { get; set; }
        public decimal? OtrasCuentas { get; set; }
        public decimal? Patrimonio { get; set; }
        public decimal? TotalPatrimonio { get; set; }
        public decimal? TotalPasivosPatrimonio { get; set; }
        public decimal? VentasNetas { get; set; }
        public decimal? CostoVentas { get; set; }
        public decimal? CostoMateriales { get; set; }
        public decimal? GananciaBruta { get; set; }
        public decimal? OtrosGastosOperativos { get; set; }
        public decimal? CostoEmpleados { get; set; }
        public decimal? Depreciacion { get; set; }
        public decimal? IngresosFinancieros { get; set; }
        public decimal? GastosFinancieros { get; set; }
        public decimal? InteresesPagados { get; set; }
        public decimal? PlFinanciero { get; set; }
        public decimal? IngresosExtraordinarios { get; set; }
        public decimal? GastosExtraordinarios { get; set; }
        public decimal? PlExtraordinario { get; set; }
        public decimal? GananciaAntesImpuestos { get; set; }
        public decimal? Impuestos { get; set; }
        public decimal? GananciaNeta { get; set; }
        public decimal? Ebit { get; set; }
        public decimal? Ebitda { get; set; }
        public decimal? Ganancia { get; set; }
        public decimal? IndiceLiquidez { get; set; }
        public decimal? CapitalTrabajo { get; set; }
        public decimal? RatioEndeudamiento { get; set; }
        public decimal? RatioRentabilidad { get; set; }
    }

    public class InformeLocalImagenItem
    {
        public int? IdInformeLocalImagen { get; set; }
        public int IdTipoArchivo { get; set; }
        public string? Nombre { get; set; }
        public string ImagenURL { get; set; } = string.Empty;
    }

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

    public class InformeLocalImagenPendiente
    {
        public int IdInformeLocalImagen { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string UploadUrl { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string S3Key { get; set; } = string.Empty;
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

    public class PrefacturaDatosConsulta
    {
        public string? Correo { get; set; }
        public string CodigoPedido { get; set; } = string.Empty;
        public string? NombreInvestigado { get; set; }
        public string? Pais { get; set; }
        public string? Moneda { get; set; }
        public string? TipoTramite { get; set; }
        public string? DiasMinMax { get; set; }
        public decimal Precio { get; set; }
        public decimal Penalidad { get; set; }
        public bool EsIngles { get; set; }
    }

    public class InformeBalanceTotalizadoCalcularRequest
    {
        public decimal? TotalActivoCorriente { get; set; }
        public decimal? TotalActivoNoCorriente { get; set; }
        public decimal? TotalPasivoCorriente { get; set; }
        public decimal? TotalPasivoNoCorriente { get; set; }
        public decimal? TotalPatrimonio { get; set; }
        public decimal? IngresosOrdinarios { get; set; }
        public decimal? GananciaNeta { get; set; }
    }

    public class InformeBalanceTotalizadoCalculado
    {
        public decimal TotalActivo { get; set; }
        public decimal TotalPasivos { get; set; }
        public decimal TotalPasivoPatrimonio { get; set; }
        public decimal? IndiceLiquidez { get; set; }
        public decimal CapitalTrabajo { get; set; }
        public decimal? RatioEndeudamiento { get; set; }
        public decimal? RatioRentabilidad { get; set; }
    }

    public class InformeBalanceDesagregadoCalcularRequest
    {
        // Activo Corriente
        public decimal? EfectivoEquivalente { get; set; }
        public decimal? OtrosActivosFinancierosCorriente { get; set; }
        public decimal? CuentasCobrarCorriente { get; set; }
        public decimal? InventariosCorriente { get; set; }
        public decimal? ActivosBiologicosCorriente { get; set; }
        public decimal? ActivosImpuestosGanancias { get; set; }
        public decimal? OtrosActivosNoFinancierosCorriente { get; set; }
        // Activo No Corriente
        public decimal? OtrosActivosFinancierosNoCorriente { get; set; }
        public decimal? InversionesSubsidiarias { get; set; }
        public decimal? CuentasCobrarNoCorriente { get; set; }
        public decimal? InventariosNoCorriente { get; set; }
        public decimal? ActivosBiologicosNoCorriente { get; set; }
        public decimal? PropiedadesInversion { get; set; }
        public decimal? PropiedadesPlantaEquipo { get; set; }
        public decimal? Intangibles { get; set; }
        public decimal? ActivosImpuestosDiferidos { get; set; }
        public decimal? ActivosImpuestosCorrientes { get; set; }
        public decimal? Plusvalia { get; set; }
        public decimal? OtrosActivosNoFinancierosNoCorriente { get; set; }
        // Pasivo Corriente
        public decimal? OtrosPasivosFinancierosCorriente { get; set; }
        public decimal? CuentasPagarCorriente { get; set; }
        public decimal? BeneficiosEmpleadosCorriente { get; set; }
        public decimal? OtrasProvisionesCorriente { get; set; }
        public decimal? ImpuestosGananciasCorriente { get; set; }
        public decimal? OtrosPasivosNoFinancierosCorriente { get; set; }
        // Pasivo No Corriente
        public decimal? OtrosPasivosFinancierosNoCorriente { get; set; }
        public decimal? CuentasPagarNoCorriente { get; set; }
        public decimal? BeneficiosEmpleadosNoCorriente { get; set; }
        public decimal? OtrasProvisionesNoCorriente { get; set; }
        public decimal? ImpuestosDiferidosNoCorriente { get; set; }
        public decimal? ImpuestosCorrientesNoCorriente { get; set; }
        public decimal? OtrosPasivosNoFinancierosNoCorriente { get; set; }
        // Patrimonio
        public decimal? CapitalEmitido { get; set; }
        public decimal? PrimasEmision { get; set; }
        public decimal? AccionesInversion { get; set; }
        public decimal? AccionesCartera { get; set; }
        public decimal? OtrasReservasCapital { get; set; }
        public decimal? ResultadosAcumulados { get; set; }
        public decimal? OtrasReservasPatrimonio { get; set; }
        // Estado de Resultados
        public decimal? IngresosOrdinarios { get; set; }
        public decimal? CostoVentas { get; set; }
        public decimal? GastosVentas { get; set; }
        public decimal? GastosAdministracion { get; set; }
        public decimal? OtrosIngresosOperativos { get; set; }
        public decimal? OtrosGastosOperativos { get; set; }
        public decimal? OtrasGananciasPerdidas { get; set; }
        public decimal? IngresosFinancieros { get; set; }
        public decimal? IngresosIntereses { get; set; }
        public decimal? GastosFinancieros { get; set; }
        public decimal? DeterioroValor { get; set; }
        public decimal? OtrosIngresosSubsidiarias { get; set; }
        public decimal? DiferenciasCambio { get; set; }
        public decimal? IngresoGastoImpuesto { get; set; }
        public decimal? OperacionesDescontinuadas { get; set; }
    }

    public class InformeBalanceDesagregadoCalculado
    {
        public decimal TotalActivoCorriente { get; set; }
        public decimal TotalActivoNoCorriente { get; set; }
        public decimal TotalActivo { get; set; }
        public decimal TotalPasivoCorriente { get; set; }
        public decimal TotalPasivoNoCorriente { get; set; }
        public decimal TotalPasivos { get; set; }
        public decimal TotalPatrimonio { get; set; }
        public decimal TotalPasivoPatrimonio { get; set; }
        public decimal GananciaBruta { get; set; }
        public decimal GananciaOperativa { get; set; }
        public decimal GananciaAntesImpuestos { get; set; }
        public decimal GananciaNeta { get; set; }
        public decimal? IndiceLiquidez { get; set; }
        public decimal CapitalTrabajo { get; set; }
        public decimal? RatioEndeudamiento { get; set; }
        public decimal? RatioRentabilidad { get; set; }
    }

    public class InformeBalanceSeguroCalcularRequest
    {
        // Activo
        public decimal? EfectivoDisponible { get; set; }
        public decimal? InversionesFinancieras { get; set; }
        public decimal? PrestamosInteresesNetos { get; set; }
        public decimal? PrimasCobrar { get; set; }
        public decimal? DeudasReaseguradores { get; set; }
        public decimal? ActivosVenta { get; set; }
        public decimal? PropiedadesInversion { get; set; }
        public decimal? PropiedadPlantaEquipo { get; set; }
        public decimal? OtrosActivos { get; set; }
        // Pasivo
        public decimal? ObligacionesAsegurados { get; set; }
        public decimal? ReservasSiniestros { get; set; }
        public decimal? ReservasTecnicas { get; set; }
        public decimal? ObligacionesReaseguradores { get; set; }
        public decimal? ObligacionesFinancieras { get; set; }
        public decimal? CuentasPagar { get; set; }
        public decimal? OtrosPasivos { get; set; }
        // Patrimonio
        public decimal? CapitalSocial { get; set; }
        public decimal? AportesCapitalNoCapitalizados { get; set; }
        public decimal? ResultadosAcumulados { get; set; }
        public decimal? PatrimonioRestringido { get; set; }
    }

    public class InformeBalanceSeguroCalculado
    {
        public decimal TotalActivos { get; set; }
        public decimal TotalPasivo { get; set; }
        public decimal TotalPatrimonio { get; set; }
        public decimal TotalPasivoPatrimonio { get; set; }
    }

    public class InformeBalanceBancoCalcularRequest
    {
        // Activo
        public decimal? Disponible { get; set; }
        public decimal? FondosInterbancarios { get; set; }
        public decimal? InversionesValorRazonable { get; set; }
        public decimal? CarteraCreditos { get; set; }
        public decimal? DerivadosNegociacionActivo { get; set; }
        public decimal? DerivadosCoberturaActivo { get; set; }
        public decimal? BienesRealizables { get; set; }
        public decimal? ParticipacionesSubsidiarias { get; set; }
        public decimal? InmuebleMobiliarioEquipo { get; set; }
        public decimal? ImpuestoRentaDiferido { get; set; }
        public decimal? OtrosActivos { get; set; }
        // Pasivo
        public decimal? ObligacionesPublico { get; set; }
        public decimal? FondosInterbancariosPasivo { get; set; }
        public decimal? AdeudosFinancieras { get; set; }
        public decimal? DerivadosNegociacionPasivo { get; set; }
        public decimal? DerivadosCoberturaPasivo { get; set; }
        public decimal? CuentasPagarProvisiones { get; set; }
        // Patrimonio
        public decimal? CapitalSocial { get; set; }
        public decimal? Reservas { get; set; }
        public decimal? ResultadosNoRealizados { get; set; }
        public decimal? ResultadoEjercicio { get; set; }
    }

    public class InformeBalanceBancoCalculado
    {
        public decimal TotalActivos { get; set; }
        public decimal TotalPasivo { get; set; }
        public decimal TotalPatrimonio { get; set; }
        public decimal TotalPasivoPatrimonio { get; set; }
    }

    public class InformeBalanceTurquiaCalcularRequest
    {
        public decimal? Efectivo { get; set; }
        public decimal? Existencias { get; set; }
        public decimal? Deudores { get; set; }
        public decimal? BienesTongibles { get; set; }
        public decimal? ActivosIntangibles { get; set; }
        public decimal? Prestamos { get; set; }
        public decimal? Acreedores { get; set; }
        public decimal? PasivosNoCorrientes { get; set; }
        public decimal? PasivosLargoPlazo { get; set; }
        public decimal? Patrimonio { get; set; }
        public decimal? Reservas { get; set; }
        public decimal? ResultadosAcumulados { get; set; }
        public decimal? PerdidaGanancias { get; set; }
        public decimal? OtrasCuentas { get; set; }
        public decimal? VentasNetas { get; set; }
        public decimal? CostoVentas { get; set; }
        public decimal? OtrosGastosOperativos { get; set; }
        public decimal? CostoEmpleados { get; set; }
        public decimal? Depreciacion { get; set; }
        public decimal? IngresosFinancieros { get; set; }
        public decimal? GastosFinancieros { get; set; }
        public decimal? IngresosExtraordinarios { get; set; }
        public decimal? GastosExtraordinarios { get; set; }
        public decimal? Impuestos { get; set; }
        public decimal? CostoMateriales { get; set; }
        public decimal? InteresesPagados { get; set; }
        public decimal? Capital { get; set; }
        public decimal? Ebit { get; set; }
        public decimal? Ebitda { get; set; }
        public decimal? Ganancia { get; set; }
    }

    public class InformeBalanceTurquiaCalculado
    {
        public decimal? TotalCorriente { get; set; }
        public decimal? ActivoFijoNeto { get; set; }
        public decimal? TotalActivos { get; set; }
        public decimal? PasivosCorrientes { get; set; }
        public decimal? TotalPasivosNoCorrientes { get; set; }
        public decimal? TotalPasivos { get; set; }
        public decimal? TotalPatrimonio { get; set; }
        public decimal? TotalPasivosPatrimonio { get; set; }
        public decimal? GananciaBruta { get; set; }
        public decimal? PlFinanciero { get; set; }
        public decimal? PlExtraordinario { get; set; }
        public decimal? GananciaAntesImpuestos { get; set; }
        public decimal? GananciaNeta { get; set; }
        public decimal? Ebit { get; set; }
        public decimal? Ebitda { get; set; }
        public decimal? IndiceLiquidez { get; set; }
        public decimal? CapitalTrabajo { get; set; }
        public decimal? RatioEndeudamiento { get; set; }
        public decimal? RatioRentabilidad { get; set; }
    }

    public class InformeLocalImagenEstadoCargaRequest
    {
        public List<int> Ids { get; set; } = new();
    }

    public class InformeLocalImagenUrl
    {
        public int IdInformeLocalImagen { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ImagenURL { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class InformeArchivoItem
    {
        public string Nombre { get; set; } = string.Empty;
        public string ArchivoUrl { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public int IdTipoArchivo { get; set; }
        public int? IdFaseEvidencia { get; set; }
    }

    public class InformeArchivoInsertarRequest
    {
        public int IdInforme { get; set; }
        public int IdPedido { get; set; }
        public List<InformeArchivoItem> Archivos { get; set; } = new();
    }

    public class InformeArchivoActualizarRequest
    {
        public int IdInformeArchivo { get; set; }
        public int? IdTipoArchivo { get; set; }
        public int? IdFaseEvidencia { get; set; }
    }

    public class InformeArchivoIdRequest
    {
        public int IdInformeArchivo { get; set; }
    }

    public class InformeArchivoConsulta
    {
        public int IdInformeArchivo { get; set; }
        public int IdInforme { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ArchivoUrl { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public int IdTipoArchivo { get; set; }
        public int IdFaseEvidencia { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class InformeArchivoResumen
    {
        public int IdInformeArchivo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public int IdTipoArchivo { get; set; }
        public int? IdFaseEvidencia { get; set; }
    }

    public class InformeArchivoUrlRequest
    {
        public int IdPedido { get; set; }
        public int IdInforme { get; set; }
        public List<string> Nombres { get; set; } = new();
    }

    public class InformeArchivoPendiente
    {
        public string Nombre { get; set; } = string.Empty;
        public string ArchivoUrl { get; set; } = string.Empty;
        public string UploadUrl { get; set; } = string.Empty;
    }

    public class InformeArchivoUrlResult
    {
        public int IdInforme { get; set; }
        public List<InformeArchivoPendiente> Archivos { get; set; } = new();
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

    public class InformeBalanceConsulta
    {
        public int IdInformeBalance { get; set; }
        public DateTime FechaBalance { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool FlgActualidad { get; set; }
        public decimal? TipoCambio { get; set; }
        public int IdMoneda { get; set; }
        public int IdTipoBalance { get; set; }
        public int? IdTipoEstadoFinanciero { get; set; }
        public JsonElement? CuentaBalance { get; set; }
    }

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

    public class InformeLocalImagenConsulta
    {
        public int IdInformeLocalImagen { get; set; }
        public string ImagenURL { get; set; } = string.Empty;
        public int IdTipoArchivo { get; set; }
        public string? Nombre { get; set; }
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

    public class InformeObservacionItem
    {
        public string? Observacion { get; set; }
        public bool Checked { get; set; }
    }

    public class InformeObservacionInsertarRequest
    {
        public int IdInforme { get; set; }
        public int IdPedido { get; set; }
        public List<InformeObservacionItem> Observaciones { get; set; } = new();
    }

    public class InformeObservacionEditarRequest
    {
        public int IdInformeObservacion { get; set; }
        public string? Observacion { get; set; }
        public bool Checked { get; set; }
    }

    public class InformeObservacionListarRequest
    {
        public int IdPedido { get; set; }
    }

    public class InformeObservacionConsulta
    {
        public int IdInformeObservacion { get; set; }
        public int IdInforme { get; set; }
        public int IdPedido { get; set; }
        public string? Observacion { get; set; }
        public bool Checked { get; set; }
    }

    public class InformeObservacionIdRequest
    {
        public int IdInformeObservacion { get; set; }
    }

    public class InformeObservacionIdResult
    {
        public int IdInformeObservacion { get; set; }
    }
}
