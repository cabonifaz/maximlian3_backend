using System.Text.Json;

namespace SafetyReport.Application.Puertos.Informe
{
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
}
