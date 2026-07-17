using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class InformeDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeDAO> _logger;

        public InformeDAO(DbConfig dbConfig, ILogger<InformeDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // ── TVP builders ─────────────────────────────────────────────────────────

        private static DataTable ConstruirTablaBalances(List<InformeBalanceItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeBalance", typeof(int));
            t.Columns.Add("FechaBalance", typeof(DateTime));
            t.Columns.Add("FechaHasta", typeof(DateTime));
            t.Columns.Add("FlgActualidad", typeof(bool));
            t.Columns.Add("TipoCambio", typeof(decimal));
            t.Columns.Add("IdMoneda", typeof(int));
            t.Columns.Add("IdTipoBalance", typeof(int));
            t.Columns.Add("IdTipoEstadoFinanciero", typeof(int));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++,
                    (object?)x.IdInformeBalance ?? DBNull.Value,
                    x.FechaBalance,
                    (object?)x.FechaHasta ?? DBNull.Value,
                    x.FlgActualidad,
                    D4(x.TipoCambio),
                    x.IdMoneda, x.IdTipoBalance,
                    (object?)x.IdTipoEstadoFinanciero ?? DBNull.Value);
            return t;
        }

        private static DataTable ConstruirTablaBancos(List<InformeBancoItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeBanco", typeof(int));
            t.Columns.Add("IdBanco", typeof(int));
            t.Columns.Add("NumeroCuenta", typeof(string));
            t.Columns.Add("IdSector", typeof(int));
            t.Columns.Add("Sectorista", typeof(string));
            t.Columns.Add("ReferenciaBanco", typeof(string));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++, (object?)x.IdInformeBanco ?? DBNull.Value, x.IdBanco,
                    (object?)x.NumeroCuenta ?? DBNull.Value, (object?)x.IdSector ?? DBNull.Value,
                    (object?)x.Sectorista ?? DBNull.Value,
                    (object?)x.ReferenciaBanco ?? DBNull.Value);
            return t;
        }

        private static DataTable ConstruirTablaCompanias(List<InformeCompaniaRelacionadaItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeCompaniaRelacionada", typeof(int));
            t.Columns.Add("IdCompania", typeof(int));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++, (object?)x.IdInformeCompaniaRelacionada ?? DBNull.Value, x.IdCompania);
            return t;
        }

        private static DataTable ConstruirTablaExpImp(List<InformeExportacionImportacionItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeExportacionImportacion", typeof(int));
            t.Columns.Add("Anio", typeof(int));
            t.Columns.Add("MesInicio", typeof(int));
            t.Columns.Add("MesFin", typeof(int));
            t.Columns.Add("IdMoneda", typeof(int));
            t.Columns.Add("Paises", typeof(string));
            t.Columns.Add("Monto", typeof(decimal));
            t.Columns.Add("Productos", typeof(string));
            t.Columns.Add("IdTipoOperacion", typeof(int));
            t.Columns.Add("NumOperaciones", typeof(int));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++,
                    (object?)x.IdInformeExportacionImportacion ?? DBNull.Value,
                    x.Anio, x.MesInicio, x.MesFin, x.IdMoneda,
                    (object?)x.Paises ?? DBNull.Value, D2(x.Monto),
                    (object?)x.Productos ?? DBNull.Value, x.IdTipoOperacion,
                    (object?)x.NumOperaciones ?? DBNull.Value);
            return t;
        }

        private static DataTable ConstruirTablaProveedores(List<InformeProveedorItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeProveedor", typeof(int));
            t.Columns.Add("IdBancoProveedor", typeof(int));
            t.Columns.Add("IdTipoPersona", typeof(int));
            t.Columns.Add("Nombre", typeof(string));
            t.Columns.Add("IdPais", typeof(int));
            t.Columns.Add("IdTipoDocumento", typeof(int));
            t.Columns.Add("NumeroDocumento", typeof(string));
            t.Columns.Add("IdMoneda", typeof(int));
            t.Columns.Add("FechaInicio", typeof(DateTime));
            t.Columns.Add("IdLimiteCredito", typeof(int));
            t.Columns.Add("PromedioMensual", typeof(decimal));
            t.Columns.Add("PlazoCredito", typeof(string));
            t.Columns.Add("Productos", typeof(string));
            t.Columns.Add("IdCalificacion", typeof(int));
            t.Columns.Add("Comentarios", typeof(string));
            t.Columns.Add("NombreContacto", typeof(string));
            t.Columns.Add("Telefono", typeof(string));
            t.Columns.Add("ComienzoNegociaciones", typeof(string));
            t.Columns.Add("IdPlazoCredito", typeof(int));
            t.Columns.Add("EsTieneReferenciaComercial", typeof(bool));
            t.Columns.Add("TipoCambio", typeof(decimal));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++,
                    (object?)x.IdInformeProveedor ?? DBNull.Value,
                    (object?)x.IdBancoProveedor ?? DBNull.Value, x.IdTipoPersona, x.Nombre,
                    (object?)x.IdPais ?? DBNull.Value, (object?)x.IdTipoDocumento ?? DBNull.Value,
                    (object?)x.NumeroDocumento ?? DBNull.Value, (object?)x.IdMoneda ?? DBNull.Value,
                    (object?)x.FechaInicio ?? DBNull.Value, (object?)x.IdLimiteCredito ?? DBNull.Value,
                    D4(x.PromedioMensual), (object?)x.PlazoCredito ?? DBNull.Value,
                    (object?)x.Productos ?? DBNull.Value, (object?)x.IdCalificacion ?? DBNull.Value,
                    (object?)x.Comentarios ?? DBNull.Value,
                    (object?)x.NombreContacto ?? DBNull.Value,
                    (object?)x.Telefono ?? DBNull.Value,
                    (object?)x.ComienzoNegociaciones ?? DBNull.Value,
                    (object?)x.IdPlazoCredito ?? DBNull.Value,
                    (object?)x.EsTieneReferenciaComercial ?? DBNull.Value,
                    D6(x.TipoCambio));
            return t;
        }

        private static DataTable ConstruirTablaBalancesDesagregado(List<InformeBalanceDesagregadoItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("EfectivoEquivalente", typeof(decimal));
            t.Columns.Add("OtrosActivosFinancierosCorriente", typeof(decimal));
            t.Columns.Add("CuentasCobrarCorriente", typeof(decimal));
            t.Columns.Add("InventariosCorriente", typeof(decimal));
            t.Columns.Add("ActivosBiologicosCorriente", typeof(decimal));
            t.Columns.Add("ActivosImpuestosGanancias", typeof(decimal));
            t.Columns.Add("OtrosActivosNoFinancierosCorriente", typeof(decimal));
            t.Columns.Add("TotalActivoCorriente", typeof(decimal));
            t.Columns.Add("OtrosActivosFinancierosNoCorriente", typeof(decimal));
            t.Columns.Add("InversionesSubsidiarias", typeof(decimal));
            t.Columns.Add("CuentasCobrarNoCorriente", typeof(decimal));
            t.Columns.Add("InventariosNoCorriente", typeof(decimal));
            t.Columns.Add("ActivosBiologicosNoCorriente", typeof(decimal));
            t.Columns.Add("PropiedadesInversion", typeof(decimal));
            t.Columns.Add("PropiedadesPlantaEquipo", typeof(decimal));
            t.Columns.Add("Intangibles", typeof(decimal));
            t.Columns.Add("ActivosImpuestosDiferidos", typeof(decimal));
            t.Columns.Add("ActivosImpuestosCorrientes", typeof(decimal));
            t.Columns.Add("Plusvalia", typeof(decimal));
            t.Columns.Add("OtrosActivosNoFinancierosNoCorriente", typeof(decimal));
            t.Columns.Add("TotalActivoNoCorriente", typeof(decimal));
            t.Columns.Add("TotalActivo", typeof(decimal));
            t.Columns.Add("OtrosPasivosFinancierosCorriente", typeof(decimal));
            t.Columns.Add("CuentasPagarCorriente", typeof(decimal));
            t.Columns.Add("BeneficiosEmpleadosCorriente", typeof(decimal));
            t.Columns.Add("OtrasProvisionesCorriente", typeof(decimal));
            t.Columns.Add("ImpuestosGananciasCorriente", typeof(decimal));
            t.Columns.Add("OtrosPasivosNoFinancierosCorriente", typeof(decimal));
            t.Columns.Add("TotalPasivoCorriente", typeof(decimal));
            t.Columns.Add("OtrosPasivosFinancierosNoCorriente", typeof(decimal));
            t.Columns.Add("CuentasPagarNoCorriente", typeof(decimal));
            t.Columns.Add("BeneficiosEmpleadosNoCorriente", typeof(decimal));
            t.Columns.Add("OtrasProvisionesNoCorriente", typeof(decimal));
            t.Columns.Add("ImpuestosDiferidosNoCorriente", typeof(decimal));
            t.Columns.Add("ImpuestosCorrientesNoCorriente", typeof(decimal));
            t.Columns.Add("OtrosPasivosNoFinancierosNoCorriente", typeof(decimal));
            t.Columns.Add("TotalPasivoNoCorriente", typeof(decimal));
            t.Columns.Add("TotalPasivos", typeof(decimal));
            t.Columns.Add("CapitalEmitido", typeof(decimal));
            t.Columns.Add("PrimasEmision", typeof(decimal));
            t.Columns.Add("AccionesInversion", typeof(decimal));
            t.Columns.Add("AccionesCartera", typeof(decimal));
            t.Columns.Add("OtrasReservasCapital", typeof(decimal));
            t.Columns.Add("ResultadosAcumulados", typeof(decimal));
            t.Columns.Add("OtrasReservasPatrimonio", typeof(decimal));
            t.Columns.Add("TotalPatrimonio", typeof(decimal));
            t.Columns.Add("TotalPasivoPatrimonio", typeof(decimal));
            t.Columns.Add("IngresosOrdinarios", typeof(decimal));
            t.Columns.Add("CostoVentas", typeof(decimal));
            t.Columns.Add("GananciaBruta", typeof(decimal));
            t.Columns.Add("GastosVentas", typeof(decimal));
            t.Columns.Add("GastosAdministracion", typeof(decimal));
            t.Columns.Add("OtrosIngresosOperativos", typeof(decimal));
            t.Columns.Add("OtrosGastosOperativos", typeof(decimal));
            t.Columns.Add("OtrasGananciasPerdidas", typeof(decimal));
            t.Columns.Add("GananciaOperativa", typeof(decimal));
            t.Columns.Add("IngresosFinancieros", typeof(decimal));
            t.Columns.Add("IngresosIntereses", typeof(decimal));
            t.Columns.Add("GastosFinancieros", typeof(decimal));
            t.Columns.Add("DeterioroValor", typeof(decimal));
            t.Columns.Add("OtrosIngresosSubsidiarias", typeof(decimal));
            t.Columns.Add("DiferenciasCambio", typeof(decimal));
            t.Columns.Add("GananciaAntesImpuestos", typeof(decimal));
            t.Columns.Add("IngresoGastoImpuesto", typeof(decimal));
            t.Columns.Add("OperacionesDescontinuadas", typeof(decimal));
            t.Columns.Add("GananciaNeta", typeof(decimal));
            t.Columns.Add("IndiceLiquidez", typeof(decimal));
            t.Columns.Add("CapitalTrabajo", typeof(decimal));
            t.Columns.Add("RatioEndeudamiento", typeof(decimal));
            t.Columns.Add("RatioRentabilidad", typeof(decimal));
            foreach (var x in items)
                t.Rows.Add(
                    x.Id,
                    D2(x.EfectivoEquivalente),
                    D2(x.OtrosActivosFinancierosCorriente),
                    D2(x.CuentasCobrarCorriente),
                    D2(x.InventariosCorriente),
                    D2(x.ActivosBiologicosCorriente),
                    D2(x.ActivosImpuestosGanancias),
                    D2(x.OtrosActivosNoFinancierosCorriente),
                    D2(x.TotalActivoCorriente),
                    D2(x.OtrosActivosFinancierosNoCorriente),
                    D2(x.InversionesSubsidiarias),
                    D2(x.CuentasCobrarNoCorriente),
                    D2(x.InventariosNoCorriente),
                    D2(x.ActivosBiologicosNoCorriente),
                    D2(x.PropiedadesInversion),
                    D2(x.PropiedadesPlantaEquipo),
                    D2(x.Intangibles),
                    D2(x.ActivosImpuestosDiferidos),
                    D2(x.ActivosImpuestosCorrientes),
                    D2(x.Plusvalia),
                    D2(x.OtrosActivosNoFinancierosNoCorriente),
                    D2(x.TotalActivoNoCorriente),
                    D2(x.TotalActivo),
                    D2(x.OtrosPasivosFinancierosCorriente),
                    D2(x.CuentasPagarCorriente),
                    D2(x.BeneficiosEmpleadosCorriente),
                    D2(x.OtrasProvisionesCorriente),
                    D2(x.ImpuestosGananciasCorriente),
                    D2(x.OtrosPasivosNoFinancierosCorriente),
                    D2(x.TotalPasivoCorriente),
                    D2(x.OtrosPasivosFinancierosNoCorriente),
                    D2(x.CuentasPagarNoCorriente),
                    D2(x.BeneficiosEmpleadosNoCorriente),
                    D2(x.OtrasProvisionesNoCorriente),
                    D2(x.ImpuestosDiferidosNoCorriente),
                    D2(x.ImpuestosCorrientesNoCorriente),
                    D2(x.OtrosPasivosNoFinancierosNoCorriente),
                    D2(x.TotalPasivoNoCorriente),
                    D2(x.TotalPasivos),
                    D2(x.CapitalEmitido),
                    D2(x.PrimasEmision),
                    D2(x.AccionesInversion),
                    D2(x.AccionesCartera),
                    D2(x.OtrasReservasCapital),
                    D2(x.ResultadosAcumulados),
                    D2(x.OtrasReservasPatrimonio),
                    D2(x.TotalPatrimonio),
                    D2(x.TotalPasivoPatrimonio),
                    D2(x.IngresosOrdinarios),
                    D2(x.CostoVentas),
                    D2(x.GananciaBruta),
                    D2(x.GastosVentas),
                    D2(x.GastosAdministracion),
                    D2(x.OtrosIngresosOperativos),
                    D2(x.OtrosGastosOperativos),
                    D2(x.OtrasGananciasPerdidas),
                    D2(x.GananciaOperativa),
                    D2(x.IngresosFinancieros),
                    D2(x.IngresosIntereses),
                    D2(x.GastosFinancieros),
                    D2(x.DeterioroValor),
                    D2(x.OtrosIngresosSubsidiarias),
                    D2(x.DiferenciasCambio),
                    D2(x.GananciaAntesImpuestos),
                    D2(x.IngresoGastoImpuesto),
                    D2(x.OperacionesDescontinuadas),
                    D2(x.GananciaNeta),
                    D2(x.IndiceLiquidez),
                    D2(x.CapitalTrabajo),
                    D2(x.RatioEndeudamiento),
                    D2(x.RatioRentabilidad));
            return t;
        }

        private static DataTable ConstruirTablaBalancesTotalizado(List<InformeBalanceTotalizadoItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("TotalActivoCorriente", typeof(decimal));
            t.Columns.Add("TotalActivoNoCorriente", typeof(decimal));
            t.Columns.Add("TotalActivo", typeof(decimal));
            t.Columns.Add("TotalPasivoCorriente", typeof(decimal));
            t.Columns.Add("TotalPasivoNoCorriente", typeof(decimal));
            t.Columns.Add("TotalPasivos", typeof(decimal));
            t.Columns.Add("TotalPatrimonio", typeof(decimal));
            t.Columns.Add("TotalPasivoPatrimonio", typeof(decimal));
            t.Columns.Add("IngresosOrdinarios", typeof(decimal));
            t.Columns.Add("GananciaNeta", typeof(decimal));
            t.Columns.Add("IndiceLiquidez", typeof(decimal));
            t.Columns.Add("CapitalTrabajo", typeof(decimal));
            t.Columns.Add("RatioEndeudamiento", typeof(decimal));
            t.Columns.Add("RatioRentabilidad", typeof(decimal));
            foreach (var x in items)
                t.Rows.Add(
                    x.Id,
                    D2(x.TotalActivoCorriente),
                    D2(x.TotalActivoNoCorriente),
                    D2(x.TotalActivo),
                    D2(x.TotalPasivoCorriente),
                    D2(x.TotalPasivoNoCorriente),
                    D2(x.TotalPasivos),
                    D2(x.TotalPatrimonio),
                    D2(x.TotalPasivoPatrimonio),
                    D2(x.IngresosOrdinarios),
                    D2(x.GananciaNeta),
                    D2(x.IndiceLiquidez),
                    D2(x.CapitalTrabajo),
                    D2(x.RatioEndeudamiento),
                    D2(x.RatioRentabilidad));
            return t;
        }

        private static DataTable ConstruirTablaBalancesBanco(List<InformeBalanceBancoItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("Disponible", typeof(decimal));
            t.Columns.Add("FondosInterbancarios", typeof(decimal));
            t.Columns.Add("InversionesValorRazonable", typeof(decimal));
            t.Columns.Add("CarteraCreditos", typeof(decimal));
            t.Columns.Add("DerivadosNegociacionActivo", typeof(decimal));
            t.Columns.Add("DerivadosCoberturaActivo", typeof(decimal));
            t.Columns.Add("BienesRealizables", typeof(decimal));
            t.Columns.Add("ParticipacionesSubsidiarias", typeof(decimal));
            t.Columns.Add("InmuebleMobiliarioEquipo", typeof(decimal));
            t.Columns.Add("ImpuestoRentaDiferido", typeof(decimal));
            t.Columns.Add("OtrosActivos", typeof(decimal));
            t.Columns.Add("TotalActivos", typeof(decimal));
            t.Columns.Add("ObligacionesPublico", typeof(decimal));
            t.Columns.Add("FondosInterbancariosPasivo", typeof(decimal));
            t.Columns.Add("AdeudosFinancieras", typeof(decimal));
            t.Columns.Add("DerivadosNegociacionPasivo", typeof(decimal));
            t.Columns.Add("DerivadosCoberturaPasivo", typeof(decimal));
            t.Columns.Add("CuentasPagarProvisiones", typeof(decimal));
            t.Columns.Add("TotalPasivo", typeof(decimal));
            t.Columns.Add("CapitalSocial", typeof(decimal));
            t.Columns.Add("Reservas", typeof(decimal));
            t.Columns.Add("ResultadosNoRealizados", typeof(decimal));
            t.Columns.Add("ResultadoEjercicio", typeof(decimal));
            t.Columns.Add("TotalPatrimonio", typeof(decimal));
            t.Columns.Add("TotalPasivoPatrimonio", typeof(decimal));
            t.Columns.Add("IngresosIntereses", typeof(decimal));
            t.Columns.Add("UtilidadEjercicio", typeof(decimal));
            foreach (var x in items)
                t.Rows.Add(
                    x.Id,
                    D2(x.Disponible),
                    D2(x.FondosInterbancarios),
                    D2(x.InversionesValorRazonable),
                    D2(x.CarteraCreditos),
                    D2(x.DerivadosNegociacionActivo),
                    D2(x.DerivadosCoberturaActivo),
                    D2(x.BienesRealizables),
                    D2(x.ParticipacionesSubsidiarias),
                    D2(x.InmuebleMobiliarioEquipo),
                    D2(x.ImpuestoRentaDiferido),
                    D2(x.OtrosActivos),
                    D2(x.TotalActivos),
                    D2(x.ObligacionesPublico),
                    D2(x.FondosInterbancariosPasivo),
                    D2(x.AdeudosFinancieras),
                    D2(x.DerivadosNegociacionPasivo),
                    D2(x.DerivadosCoberturaPasivo),
                    D2(x.CuentasPagarProvisiones),
                    D2(x.TotalPasivo),
                    D2(x.CapitalSocial),
                    D2(x.Reservas),
                    D2(x.ResultadosNoRealizados),
                    D2(x.ResultadoEjercicio),
                    D2(x.TotalPatrimonio),
                    D2(x.TotalPasivoPatrimonio),
                    D2(x.IngresosIntereses),
                    D2(x.UtilidadEjercicio));
            return t;
        }

        private static DataTable ConstruirTablaBalancesSeguro(List<InformeBalanceSeguroItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("EfectivoDisponible", typeof(decimal));
            t.Columns.Add("InversionesFinancieras", typeof(decimal));
            t.Columns.Add("PrestamosInteresesNetos", typeof(decimal));
            t.Columns.Add("PrimasCobrar", typeof(decimal));
            t.Columns.Add("DeudasReaseguradores", typeof(decimal));
            t.Columns.Add("ActivosVenta", typeof(decimal));
            t.Columns.Add("PropiedadesInversion", typeof(decimal));
            t.Columns.Add("PropiedadPlantaEquipo", typeof(decimal));
            t.Columns.Add("OtrosActivos", typeof(decimal));
            t.Columns.Add("TotalActivos", typeof(decimal));
            t.Columns.Add("ObligacionesAsegurados", typeof(decimal));
            t.Columns.Add("ReservasSiniestros", typeof(decimal));
            t.Columns.Add("ReservasTecnicas", typeof(decimal));
            t.Columns.Add("ObligacionesReaseguradores", typeof(decimal));
            t.Columns.Add("ObligacionesFinancieras", typeof(decimal));
            t.Columns.Add("CuentasPagar", typeof(decimal));
            t.Columns.Add("OtrosPasivos", typeof(decimal));
            t.Columns.Add("TotalPasivo", typeof(decimal));
            t.Columns.Add("CapitalSocial", typeof(decimal));
            t.Columns.Add("AportesCapitalNoCapitalizados", typeof(decimal));
            t.Columns.Add("ResultadosAcumulados", typeof(decimal));
            t.Columns.Add("PatrimonioRestringido", typeof(decimal));
            t.Columns.Add("TotalPatrimonio", typeof(decimal));
            t.Columns.Add("TotalPasivoPatrimonio", typeof(decimal));
            t.Columns.Add("PrimasGanadasNetas", typeof(decimal));
            t.Columns.Add("UtilidadNeta", typeof(decimal));
            foreach (var x in items)
                t.Rows.Add(
                    x.Id,
                    D2(x.EfectivoDisponible),
                    D2(x.InversionesFinancieras),
                    D2(x.PrestamosInteresesNetos),
                    D2(x.PrimasCobrar),
                    D2(x.DeudasReaseguradores),
                    D2(x.ActivosVenta),
                    D2(x.PropiedadesInversion),
                    D2(x.PropiedadPlantaEquipo),
                    D2(x.OtrosActivos),
                    D2(x.TotalActivos),
                    D2(x.ObligacionesAsegurados),
                    D2(x.ReservasSiniestros),
                    D2(x.ReservasTecnicas),
                    D2(x.ObligacionesReaseguradores),
                    D2(x.ObligacionesFinancieras),
                    D2(x.CuentasPagar),
                    D2(x.OtrosPasivos),
                    D2(x.TotalPasivo),
                    D2(x.CapitalSocial),
                    D2(x.AportesCapitalNoCapitalizados),
                    D2(x.ResultadosAcumulados),
                    D2(x.PatrimonioRestringido),
                    D2(x.TotalPatrimonio),
                    D2(x.TotalPasivoPatrimonio),
                    D2(x.PrimasGanadasNetas),
                    D2(x.UtilidadNeta));
            return t;
        }

        private static DataTable ConstruirTablaBalancesTurquia(List<InformeBalanceTurquiaItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("Ano", typeof(int));
            t.Columns.Add("FechaBalance", typeof(DateTime));
            t.Columns.Add("IdMoneda", typeof(int));
            t.Columns.Add("DuracionPeriodo", typeof(int));
            t.Columns.Add("IdNivelConfiabilidad", typeof(int));
            t.Columns.Add("TipoCambio", typeof(decimal));
            t.Columns.Add("Efectivo", typeof(decimal));
            t.Columns.Add("Existencias", typeof(decimal));
            t.Columns.Add("Deudores", typeof(decimal));
            t.Columns.Add("TotalCorriente", typeof(decimal));
            t.Columns.Add("BienesTongibles", typeof(decimal));
            t.Columns.Add("ActivosIntangibles", typeof(decimal));
            t.Columns.Add("ActivoFijoNeto", typeof(decimal));
            t.Columns.Add("TotalActivos", typeof(decimal));
            t.Columns.Add("Prestamos", typeof(decimal));
            t.Columns.Add("Acreedores", typeof(decimal));
            t.Columns.Add("PasivosCorrientes", typeof(decimal));
            t.Columns.Add("PasivosNoCorrientes", typeof(decimal));
            t.Columns.Add("PasivosLargoPlazo", typeof(decimal));
            t.Columns.Add("TotalPasivosNoCorrientes", typeof(decimal));
            t.Columns.Add("TotalPasivos", typeof(decimal));
            t.Columns.Add("Capital", typeof(decimal));
            t.Columns.Add("Reservas", typeof(decimal));
            t.Columns.Add("ResultadosAcumulados", typeof(decimal));
            t.Columns.Add("ResultadoEjercicio", typeof(decimal));
            t.Columns.Add("OtrasCuentas", typeof(decimal));
            t.Columns.Add("Patrimonio", typeof(decimal));
            t.Columns.Add("TotalPatrimonio", typeof(decimal));
            t.Columns.Add("TotalPasivosPatrimonio", typeof(decimal));
            t.Columns.Add("VentasNetas", typeof(decimal));
            t.Columns.Add("CostoVentas", typeof(decimal));
            t.Columns.Add("CostoMateriales", typeof(decimal));
            t.Columns.Add("GananciaBruta", typeof(decimal));
            t.Columns.Add("OtrosGastosOperativos", typeof(decimal));
            t.Columns.Add("CostoEmpleados", typeof(decimal));
            t.Columns.Add("Depreciacion", typeof(decimal));
            t.Columns.Add("IngresosFinancieros", typeof(decimal));
            t.Columns.Add("GastosFinancieros", typeof(decimal));
            t.Columns.Add("InteresesPagados", typeof(decimal));
            t.Columns.Add("PlFinanciero", typeof(decimal));
            t.Columns.Add("IngresosExtraordinarios", typeof(decimal));
            t.Columns.Add("GastosExtraordinarios", typeof(decimal));
            t.Columns.Add("PlExtraordinario", typeof(decimal));
            t.Columns.Add("GananciaAntesImpuestos", typeof(decimal));
            t.Columns.Add("Impuestos", typeof(decimal));
            t.Columns.Add("GananciaNeta", typeof(decimal));
            t.Columns.Add("Ebit", typeof(decimal));
            t.Columns.Add("Ebitda", typeof(decimal));
            t.Columns.Add("Ganancia", typeof(decimal));
            t.Columns.Add("IndiceLiquidez", typeof(decimal));
            t.Columns.Add("CapitalTrabajo", typeof(decimal));
            t.Columns.Add("RatioEndeudamiento", typeof(decimal));
            t.Columns.Add("RatioRentabilidad", typeof(decimal));
            foreach (var x in items)
                t.Rows.Add(
                    x.Id,
                    (object?)x.Ano ?? DBNull.Value,
                    (object?)x.FechaBalance ?? DBNull.Value,
                    (object?)x.IdMoneda ?? DBNull.Value,
                    (object?)x.DuracionPeriodo ?? DBNull.Value,
                    (object?)x.IdNivelConfiabilidad ?? DBNull.Value,
                    D6(x.TipoCambio),
                    D2(x.Efectivo),
                    D2(x.Existencias),
                    D2(x.Deudores),
                    D2(x.TotalCorriente),
                    D2(x.BienesTongibles),
                    D2(x.ActivosIntangibles),
                    D2(x.ActivoFijoNeto),
                    D2(x.TotalActivos),
                    D2(x.Prestamos),
                    D2(x.Acreedores),
                    D2(x.PasivosCorrientes),
                    D2(x.PasivosNoCorrientes),
                    D2(x.PasivosLargoPlazo),
                    D2(x.TotalPasivosNoCorrientes),
                    D2(x.TotalPasivos),
                    D2(x.Capital),
                    D2(x.Reservas),
                    D2(x.ResultadosAcumulados),
                    D2(x.ResultadoEjercicio),
                    D2(x.OtrasCuentas),
                    D2(x.Patrimonio),
                    D2(x.TotalPatrimonio),
                    D2(x.TotalPasivosPatrimonio),
                    D2(x.VentasNetas),
                    D2(x.CostoVentas),
                    D2(x.CostoMateriales),
                    D2(x.GananciaBruta),
                    D2(x.OtrosGastosOperativos),
                    D2(x.CostoEmpleados),
                    D2(x.Depreciacion),
                    D2(x.IngresosFinancieros),
                    D2(x.GastosFinancieros),
                    D2(x.InteresesPagados),
                    D2(x.PlFinanciero),
                    D2(x.IngresosExtraordinarios),
                    D2(x.GastosExtraordinarios),
                    D2(x.PlExtraordinario),
                    D2(x.GananciaAntesImpuestos),
                    D2(x.Impuestos),
                    D2(x.GananciaNeta),
                    D2(x.Ebit),
                    D2(x.Ebitda),
                    D2(x.Ganancia),
                    D2(x.IndiceLiquidez),
                    D2(x.CapitalTrabajo),
                    D2(x.RatioEndeudamiento),
                    D2(x.RatioRentabilidad));
            return t;
        }

        private static DataTable ConstruirTablaDirectoriosEjecutivos(List<InformeDirectorioEjecutivoItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeDirectorioEjecutivo", typeof(int));
            t.Columns.Add("IdDirectorioEjecutivo", typeof(int));
            t.Columns.Add("IdCargo", typeof(string));
            t.Columns.Add("VinculadoDesde", typeof(DateTime));
            t.Columns.Add("CompaniaAnterior", typeof(string));
            t.Columns.Add("Participacion", typeof(decimal));
            t.Columns.Add("Orden", typeof(int));
            t.Columns.Add("EsParticipanteDirectiva", typeof(bool));
            t.Columns.Add("ApareceImpresoLista", typeof(bool));
            t.Columns.Add("ImprimeDatosEjecutivos", typeof(bool));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++,
                    (object?)x.IdInformeDirectorioEjecutivo ?? DBNull.Value,
                    x.IdDirectorioEjecutivo,
                    (object?)x.IdCargo ?? DBNull.Value,
                    (object?)x.VinculadoDesde ?? DBNull.Value,
                    (object?)x.CompaniaAnterior ?? DBNull.Value,
                    (object?)x.Participacion ?? DBNull.Value,
                    (object?)x.Orden ?? DBNull.Value,
                    (object?)x.EsParticipanteDirectiva ?? DBNull.Value,
                    (object?)x.ApareceImpresoLista ?? DBNull.Value,
                    (object?)x.ImprimeDatosEjecutivos ?? DBNull.Value);
            return t;
        }

        private static DataTable ConstruirTablaLocales(List<InformeLocalItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeLocal", typeof(int));
            t.Columns.Add("IdTipoLocal", typeof(int));
            t.Columns.Add("Comentario", typeof(string));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++,
                    (object?)x.IdInformeLocal ?? DBNull.Value,
                    (object?)x.IdTipoLocal ?? DBNull.Value,
                    (object?)x.Comentario ?? DBNull.Value);
            return t;
        }

        private static DataTable ConstruirTablaLocalImagenes(List<InformeLocalItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeLocalImagen", typeof(int));
            t.Columns.Add("IdLocal", typeof(int));
            t.Columns.Add("ImagenURL", typeof(string));
            t.Columns.Add("IdTipoArchivo", typeof(int));
            t.Columns.Add("Nombre", typeof(string));
            int img = 1;
            int localIdx = 1;
            foreach (var local in items)
            {
                foreach (var imagen in local.Imagenes)
                    t.Rows.Add(img++, (object?)imagen.IdInformeLocalImagen ?? DBNull.Value,
                        localIdx, imagen.ImagenURL, imagen.IdTipoArchivo,
                        (object?)imagen.Nombre ?? DBNull.Value);
                localIdx++;
            }
            return t;
        }

        private static DataTable ConstruirTablaIdentificacion(InformeCrear r)
        {
            var t = new DataTable();
            t.Columns.Add("IdTipoPersona", typeof(int));
            t.Columns.Add("Nombre", typeof(string));
            t.Columns.Add("NombreComercial", typeof(string));
            t.Columns.Add("IdPais", typeof(int));
            t.Columns.Add("OperacionesTCMoneda", typeof(int));
            t.Columns.Add("TaxIdType", typeof(int));
            t.Columns.Add("TaxNum", typeof(string));
            t.Columns.Add("Direccion", typeof(string));
            t.Columns.Add("Ubigeo", typeof(string));
            t.Columns.Add("CodigoPostal", typeof(string));
            t.Columns.Add("Telefono", typeof(string));
            t.Columns.Add("Fax", typeof(string));
            t.Columns.Add("Email", typeof(string));
            t.Columns.Add("PaginaWeb", typeof(string));
            t.Columns.Add("IdEstadoManual", typeof(int));
            t.Columns.Add("DatosAdicionales", typeof(string));
            t.Columns.Add("ObservacionesIdentificacion", typeof(string));
            t.Rows.Add(
                (object?)r.IdTipoPersona ?? DBNull.Value,
                (object?)r.Nombre ?? DBNull.Value,
                (object?)r.NombreComercial ?? DBNull.Value,
                (object?)r.IdPais ?? DBNull.Value,
                (object?)r.OperacionesTCMoneda ?? DBNull.Value,
                (object?)r.TaxIdType ?? DBNull.Value,
                (object?)r.TaxNum ?? DBNull.Value,
                (object?)r.Direccion ?? DBNull.Value,
                (object?)r.Ubigeo ?? DBNull.Value,
                (object?)r.CodigoPostal ?? DBNull.Value,
                (object?)r.Telefono ?? DBNull.Value,
                (object?)r.Fax ?? DBNull.Value,
                (object?)r.Email ?? DBNull.Value,
                (object?)r.PaginaWeb ?? DBNull.Value,
                (object?)r.IdEstadoManual ?? DBNull.Value,
                (object?)r.DatosAdicionales ?? DBNull.Value,
                (object?)r.ObservacionesIdentificacion ?? DBNull.Value
            );
            return t;
        }

        private static DataTable ConstruirTablaAspectosLegales(InformeCrear r)
        {
            var t = new DataTable();
            t.Columns.Add("IdTipoEmpresa", typeof(int));
            t.Columns.Add("FechaConstitucion", typeof(DateTime));
            t.Columns.Add("IdCiudadRegistro", typeof(int));
            t.Columns.Add("IdNotaria", typeof(string));
            t.Columns.Add("IdNotario", typeof(string));
            t.Columns.Add("IdRegistro", typeof(string));
            t.Columns.Add("IdPlazo", typeof(string));
            t.Columns.Add("IdOperacionesCambioDivisas", typeof(int));
            t.Columns.Add("CapitalInicial", typeof(decimal));
            t.Columns.Add("CapitalPagado", typeof(decimal));
            t.Columns.Add("FechaUltimoIncremento", typeof(DateTime));
            t.Columns.Add("IdTipoIncremento", typeof(int));
            t.Columns.Add("PatrimonioNeto", typeof(decimal));
            t.Columns.Add("TipoAcciones", typeof(string));
            t.Columns.Add("ValorAcciones", typeof(decimal));
            t.Columns.Add("CotizaBolsa", typeof(bool));
            t.Columns.Add("TipoCambio", typeof(decimal));
            t.Columns.Add("IdTipoCambio", typeof(int));
            t.Columns.Add("Antecedentes", typeof(string));
            t.Columns.Add("AspectosLegales", typeof(string));
            t.Columns.Add("ComentariosAspectoLegal", typeof(string));
            t.Rows.Add(
                (object?)r.IdTipoEmpresa ?? DBNull.Value,
                (object?)r.FechaConstitucion ?? DBNull.Value,
                (object?)r.IdCiudadRegistro ?? DBNull.Value,
                (object?)r.IdNotaria ?? DBNull.Value,
                (object?)r.IdNotario ?? DBNull.Value,
                (object?)r.IdRegistro ?? DBNull.Value,
                (object?)r.IdPlazo ?? DBNull.Value,
                (object?)r.IdOperacionesCambioDivisas ?? DBNull.Value,
                D2(r.CapitalInicial),
                D2(r.CapitalPagado),
                (object?)r.FechaUltimoIncremento ?? DBNull.Value,
                (object?)r.IdTipoIncremento ?? DBNull.Value,
                D2(r.PatrimonioNeto),
                (object?)r.TipoAcciones ?? DBNull.Value,
                D2(r.ValorAcciones),
                (object?)r.CotizaBolsa ?? DBNull.Value,
                D6(r.TipoCambio),
                (object?)r.IdTipoCambio ?? DBNull.Value,
                (object?)r.Antecedentes ?? DBNull.Value,
                (object?)r.AspectosLegales ?? DBNull.Value,
                (object?)r.ComentariosAspectoLegal ?? DBNull.Value
            );
            return t;
        }

        private static DataTable ConstruirTablaRamoOperaciones(InformeCrear r)
        {
            var t = new DataTable();
            t.Columns.Add("IdSector", typeof(int));
            t.Columns.Add("Actividad", typeof(string));
            t.Columns.Add("IdIsicCategoria", typeof(int));
            t.Columns.Add("IdIsicClase", typeof(int));
            t.Columns.Add("ActividadPrincipal", typeof(string));
            t.Columns.Add("VentasContado", typeof(decimal));
            t.Columns.Add("VentasContadoText", typeof(string));
            t.Columns.Add("VentasCredito", typeof(decimal));
            t.Columns.Add("VentasCreditoText", typeof(string));
            t.Columns.Add("IdVentasCreditoTiempo", typeof(int));
            t.Columns.Add("VentasInternacionales", typeof(decimal));
            t.Columns.Add("VentasInternacionalesText", typeof(string));
            t.Columns.Add("VentasNacionales", typeof(decimal));
            t.Columns.Add("VentasNacionalesText", typeof(string));
            t.Columns.Add("ComprasNacionales", typeof(decimal));
            t.Columns.Add("ComprasNacionalesText", typeof(string));
            t.Columns.Add("ComprasInternacionales", typeof(decimal));
            t.Columns.Add("ComprasInternacionalesText", typeof(string));
            t.Columns.Add("ComprasContadoNacionales", typeof(decimal));
            t.Columns.Add("ComprasContadoNacionalesText", typeof(string));
            t.Columns.Add("ComprasCreditoNacionales", typeof(decimal));
            t.Columns.Add("ComprasCreditoNacionalesText", typeof(string));
            t.Columns.Add("IdComprasCreditoNacionalesTiempo", typeof(int));
            t.Columns.Add("ComprasContadoInternacionales", typeof(decimal));
            t.Columns.Add("ComprasContadoInternacionalesText", typeof(string));
            t.Columns.Add("ComprasCreditoInternacionales", typeof(decimal));
            t.Columns.Add("ComprasCreditoInternacionalesText", typeof(string));
            t.Columns.Add("IdComprasCreditoInternacionalesTiempo", typeof(int));
            t.Columns.Add("NumeroEmpleados", typeof(int));
            t.Columns.Add("NumeroEmpleadosText", typeof(string));
            t.Columns.Add("ComentariosOperaciones", typeof(string));
            t.Rows.Add(
                (object?)r.IdSector ?? DBNull.Value,
                (object?)r.Actividad ?? DBNull.Value,
                (object?)r.IdIsicCategoria ?? DBNull.Value,
                (object?)r.IdIsicClase ?? DBNull.Value,
                (object?)r.ActividadPrincipal ?? DBNull.Value,
                D2(r.VentasContado),
                (object?)r.VentasContadoText ?? DBNull.Value,
                D2(r.VentasCredito),
                (object?)r.VentasCreditoText ?? DBNull.Value,
                (object?)r.IdVentasCreditoTiempo ?? DBNull.Value,
                D2(r.VentasInternacionales),
                (object?)r.VentasInternacionalesText ?? DBNull.Value,
                D2(r.VentasNacionales),
                (object?)r.VentasNacionalesText ?? DBNull.Value,
                D2(r.ComprasNacionales),
                (object?)r.ComprasNacionalesText ?? DBNull.Value,
                D2(r.ComprasInternacionales),
                (object?)r.ComprasInternacionalesText ?? DBNull.Value,
                D2(r.ComprasContadoNacionales),
                (object?)r.ComprasContadoNacionalesText ?? DBNull.Value,
                D2(r.ComprasCreditoNacionales),
                (object?)r.ComprasCreditoNacionalesText ?? DBNull.Value,
                (object?)r.IdComprasCreditoNacionalesTiempo ?? DBNull.Value,
                D2(r.ComprasContadoInternacionales),
                (object?)r.ComprasContadoInternacionalesText ?? DBNull.Value,
                D2(r.ComprasCreditoInternacionales),
                (object?)r.ComprasCreditoInternacionalesText ?? DBNull.Value,
                (object?)r.IdComprasCreditoInternacionalesTiempo ?? DBNull.Value,
                (object?)r.NumeroEmpleados ?? DBNull.Value,
                (object?)r.NumeroEmpleadosText ?? DBNull.Value,
                (object?)r.ComentariosOperaciones ?? DBNull.Value
            );
            return t;
        }

        private static DataTable ConstruirTablaInformacionFinanciera(InformeCrear r)
        {
            var t = new DataTable();
            t.Columns.Add("ContenidoInformacionFinanciera", typeof(string));
            t.Columns.Add("ComentarioInformacionFinanciera", typeof(string));
            t.Columns.Add("ActivosFijos", typeof(string));
            t.Columns.Add("Seguros", typeof(string));
            t.Rows.Add(
                (object?)r.ContenidoInformacionFinanciera ?? DBNull.Value,
                (object?)r.ComentarioInformacionFinanciera ?? DBNull.Value,
                (object?)r.ActivosFijos ?? DBNull.Value,
                (object?)r.Seguros ?? DBNull.Value
            );
            return t;
        }

        private static DataTable ConstruirTablaBancosProveedores(InformeCrear r)
        {
            var t = new DataTable();
            t.Columns.Add("ComentarioProveedor", typeof(string));
            t.Columns.Add("ReferenciaBanco", typeof(string));
            t.Columns.Add("Litigios", typeof(string));
            t.Columns.Add("RiesgoPrincipal", typeof(string));
            t.Columns.Add("Superintendecia", typeof(string));
            t.Rows.Add(
                (object?)r.ComentarioProveedor ?? DBNull.Value,
                (object?)r.ReferenciaBanco ?? DBNull.Value,
                (object?)r.Litigios ?? DBNull.Value,
                (object?)r.RiesgoPrincipal ?? DBNull.Value,
                (object?)r.Superintendecia ?? DBNull.Value
            );
            return t;
        }

        private static DataTable ConstruirTablaDatosGenerales(InformeCrear r)
        {
            var t = new DataTable();
            t.Columns.Add("InformacionGeneral", typeof(string));
            t.Columns.Add("OpinionCredito", typeof(string));
            t.Rows.Add(
                (object?)r.InformacionGeneral ?? DBNull.Value,
                (object?)r.OpinionCredito ?? DBNull.Value
            );
            return t;
        }

        // ── Decimal rounding helpers (match SQL TVP column precision) ────────────

        private static object D2(decimal? v) => v.HasValue ? (object)Math.Round(v.Value, 2, MidpointRounding.AwayFromZero) : DBNull.Value;
        private static object D4(decimal? v) => v.HasValue ? (object)Math.Round(v.Value, 4, MidpointRounding.AwayFromZero) : DBNull.Value;
        private static object D6(decimal? v) => v.HasValue ? (object)Math.Round(v.Value, 6, MidpointRounding.AwayFromZero) : DBNull.Value;

        // ── Reader helper ─────────────────────────────────────────────────────────

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje. Sin columna Result.
        private async Task<Respuesta> LeerCabeceraAsync(SqlDataReader dr, string procedimiento)
        {
            var respuesta = new Respuesta();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 3;
                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
            }
            else
            {
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", procedimiento);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
            }

            return respuesta;
        }

        private static int? GetNullableInt(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static bool? GetNullableBool(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToBoolean(dr[columna]);

        private static DateTime? GetNullableDateTime(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDateTime(dr[columna]);

        private static string? GetNullableString(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        // Lee el result set de imagenes pendientes (IdInformeLocalImagen, ImagenURL, Nombre)
        // que Informe_Insertar/Informe_Actualizar emiten como su ultimo result set en exito.
        private static async Task<List<InformeLocalImagenPendiente>> LeerImagenesPendientesAsync(SqlDataReader dr)
        {
            var imagenes = new List<InformeLocalImagenPendiente>();
            while (await dr.ReadAsync())
                imagenes.Add(new InformeLocalImagenPendiente
                {
                    IdInformeLocalImagen = Convert.ToInt32(dr["IdInformeLocalImagen"]),
                    Nombre               = dr["Nombre"]?.ToString() ?? string.Empty,
                    S3Key                = dr["ImagenURL"]?.ToString() ?? string.Empty
                });

            return imagenes;
        }

        // ── Helpers para agregar TVPs ─────────────────────────────────────────────

        private static void AgregarTvp(SqlCommand cmd, string paramName, DataTable table, string typeName)
        {
            var p = cmd.Parameters.AddWithValue(paramName, table);
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = typeName;
        }

        private static void AgregarParametrosAuditoria(SqlCommand cmd, UsuarioGeneral u)
        {
            cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = u.IdUsuario;
            cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = u.Usuario;
            cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = u.IdEmpresa;
            cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = u.IdRol;
        }

        private static void AgregarParametrosCampos(SqlCommand cmd, InformeCrear r)
        {
            cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = (object?)r.IdPedido ?? DBNull.Value;
            cmd.Parameters.Add("@bitFlgTieneInformacion", SqlDbType.Bit).Value = (object?)r.FlgTieneInformacion ?? DBNull.Value;
            cmd.Parameters.Add("@intIdEstadoInforme", SqlDbType.Int).Value = (object?)r.IdEstadoInforme ?? DBNull.Value;
            cmd.Parameters.Add("@intIdFormatoFecha", SqlDbType.Int).Value = (object?)r.IdFormatoFecha ?? DBNull.Value;
        }

        private static void AgregarTvpsCampos(SqlCommand cmd, InformeCrear r)
        {
            AgregarTvp(cmd, "@tvpIdentificacion", ConstruirTablaIdentificacion(r), "INFORME_IDENTIFICACION");
            AgregarTvp(cmd, "@tvpAspectosLegales", ConstruirTablaAspectosLegales(r), "INFORME_ASPECTOS_LEGALES");
            AgregarTvp(cmd, "@tvpRamoOperaciones", ConstruirTablaRamoOperaciones(r), "INFORME_RAMO_OPERACIONES");
            AgregarTvp(cmd, "@tvpInformacionFinanciera", ConstruirTablaInformacionFinanciera(r), "INFORME_INFORMACION_FINANCIERA");
            AgregarTvp(cmd, "@tvpBancosProveedores", ConstruirTablaBancosProveedores(r), "INFORME_BANCOS_PROVEEDORES");
            AgregarTvp(cmd, "@tvpDatosGenerales", ConstruirTablaDatosGenerales(r), "INFORME_DATOS_GENERALES");

            AgregarTvp(cmd, "@lstBalances", ConstruirTablaBalances(r.lstBalances), "LISTA_INFORME_BALANCE");
            AgregarTvp(cmd, "@lstBalancesDesagregado", ConstruirTablaBalancesDesagregado(r.lstBalancesDesagregado), "LISTA_INFORME_BALANCE_DESAGREGADO");
            AgregarTvp(cmd, "@lstBalancesTotalizado", ConstruirTablaBalancesTotalizado(r.lstBalancesTotalizado), "LISTA_INFORME_BALANCE_TOTALIZADO");
            AgregarTvp(cmd, "@lstBalancesBanco", ConstruirTablaBalancesBanco(r.lstBalancesBanco), "LISTA_INFORME_BALANCE_BANCO");
            AgregarTvp(cmd, "@lstBalancesSeguro", ConstruirTablaBalancesSeguro(r.lstBalancesSeguro), "LISTA_INFORME_BALANCE_SEGURO");
            AgregarTvp(cmd, "@lstBalancesTurquia", ConstruirTablaBalancesTurquia(r.lstBalancesTurquia), "LISTA_INFORME_BALANCE_TURQUIA");
            AgregarTvp(cmd, "@lstBancos", ConstruirTablaBancos(r.lstBancos), "LISTA_INFORME_BANCO");
            AgregarTvp(cmd, "@lstCompanias", ConstruirTablaCompanias(r.lstCompaniasRelacionadas), "LISTA_INFORME_COMPANIA_RELACIONADA");
            AgregarTvp(cmd, "@lstExpImp", ConstruirTablaExpImp(r.lstExportacionesImportaciones), "LISTA_INFORME_EXPORTACION_IMPORTACION");
            AgregarTvp(cmd, "@lstProveedores", ConstruirTablaProveedores(r.lstProveedores), "LISTA_INFORME_PROVEEDOR");
            AgregarTvp(cmd, "@lstDirectoriosEjecutivos", ConstruirTablaDirectoriosEjecutivos(r.lstDirectoriosEjecutivos), "LISTA_INFORME_DIRECTORIO_EJECUTIVO");
            AgregarTvp(cmd, "@lstLocales", ConstruirTablaLocales(r.lstLocales), "LISTA_INFORME_LOCAL");
            AgregarTvp(cmd, "@lstLocalImagenes", ConstruirTablaLocalImagenes(r.lstLocales), "LISTA_INFORME_LOCAL_IMAGEN");
        }

        // ── CRUD ─────────────────────────────────────────────────────────────────

        public async Task<(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)> InsertarAsync(UsuarioGeneral u, InformeCrear request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarParametrosCampos(cmd, request);
                AgregarTvpsCampos(cmd, request);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var creados = new List<InformeCreado>();
                var imagenes = new List<InformeLocalImagenPendiente>();
                if (respuesta.IdTipoMensaje == 2)
                {
                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                        creados.Add(new InformeCreado { IdInforme = Convert.ToInt32(dr["IdInforme"]) });

                    if (await dr.NextResultAsync())
                        imagenes = await LeerImagenesPendientesAsync(dr);
                }

                respuesta.Result = creados;
                return (respuesta, imagenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return (new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() }, new());
            }
        }

        public async Task<(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)> ActualizarAsync(UsuarioGeneral u, InformeEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = request.IdInforme;
                AgregarParametrosCampos(cmd, request);
                AgregarTvpsCampos(cmd, request);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var creados = new List<InformeCreado>();
                var imagenes = new List<InformeLocalImagenPendiente>();
                if (respuesta.IdTipoMensaje == 2)
                {
                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                        creados.Add(new InformeCreado { IdInforme = Convert.ToInt32(dr["IdInforme"]) });

                    if (await dr.NextResultAsync())
                        imagenes = await LeerImagenesPendientesAsync(dr);
                }

                respuesta.Result = creados;
                return (respuesta, imagenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return (new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() }, new());
            }
        }


        // Lee un result set de detalle de balance (una fila por IdInformeBalance) hacia un
        // diccionario IdInformeBalance -> JsonElement con el resto de columnas, para poblar
        // InformeBalanceConsulta.CuentaBalance sin depender de JSON_QUERY en el SP.
        private static async Task<Dictionary<int, JsonElement>> LeerDetalleBalanceAsync(SqlDataReader dr)
        {
            var resultado = new Dictionary<int, JsonElement>();
            var columnas = Enumerable.Range(0, dr.FieldCount)
                .Select(dr.GetName)
                .Where(c => !c.Equals("IdInformeBalance", StringComparison.OrdinalIgnoreCase))
                .ToList();

            while (await dr.ReadAsync())
            {
                var idInformeBalance = Convert.ToInt32(dr["IdInformeBalance"]);
                var fila = new Dictionary<string, object?>();
                foreach (var col in columnas)
                    fila[col] = dr[col] == DBNull.Value ? null : dr[col];

                resultado[idInformeBalance] = JsonSerializer.SerializeToElement(fila);
            }

            return resultado;
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral u, int idPedido, int idInforme)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeConsulta>();
                if (respuesta.IdTipoMensaje == 2)
                {
                    InformeConsulta? informe = null;

                    // RS2: cabecera del informe
                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                    {
                        informe = new InformeConsulta
                        {
                            IdInforme = Convert.ToInt32(dr["IdInforme"]),
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            IdIdioma = GetNullableInt(dr, "IdIdioma"),
                            IdTipoPersona = GetNullableInt(dr, "IdTipoPersona"),
                            Nombre = GetNullableString(dr, "Nombre"),
                            NombreComercial = GetNullableString(dr, "NombreComercial"),
                            IdPais = GetNullableInt(dr, "IdPais"),
                            OperacionesTCMoneda = GetNullableInt(dr, "OperacionesTCMoneda"),
                            TaxIdType = GetNullableInt(dr, "TaxIdType"),
                            TaxNum = GetNullableString(dr, "TaxNum"),
                            Direccion = GetNullableString(dr, "Direccion"),
                            Ubigeo = GetNullableString(dr, "Ubigeo"),
                            CodigoPostal = GetNullableString(dr, "CodigoPostal"),
                            Telefono = GetNullableString(dr, "Telefono"),
                            Fax = GetNullableString(dr, "Fax"),
                            Email = GetNullableString(dr, "Email"),
                            PaginaWeb = GetNullableString(dr, "PaginaWeb"),
                            IdEstadoManual = GetNullableInt(dr, "IdEstadoManual"),
                            DatosAdicionales = GetNullableString(dr, "DatosAdicionales"),
                            ObservacionesIdentificacion = GetNullableString(dr, "ObservacionesIdentificacion"),
                            IdTipoEmpresa = GetNullableInt(dr, "IdTipoEmpresa"),
                            FechaConstitucion = GetNullableDateTime(dr, "FechaConstitucion"),
                            IdCiudadRegistro = GetNullableInt(dr, "IdCiudadRegistro"),
                            IdNotaria = GetNullableString(dr, "IdNotaria"),
                            IdNotario = GetNullableString(dr, "IdNotario"),
                            IdRegistro = GetNullableString(dr, "IdRegistro"),
                            IdPlazo = GetNullableString(dr, "IdPlazo"),
                            IdOperacionesCambioDivisas = GetNullableInt(dr, "IdOperacionesCambioDivisas"),
                            CapitalInicial = GetNullableDecimal(dr, "CapitalInicial"),
                            CapitalPagado = GetNullableDecimal(dr, "CapitalPagado"),
                            FechaUltimoIncremento = GetNullableDateTime(dr, "FechaUltimoIncremento"),
                            IdTipoIncremento = GetNullableInt(dr, "IdTipoIncremento"),
                            PatrimonioNeto = GetNullableDecimal(dr, "PatrimonioNeto"),
                            TipoAcciones = GetNullableString(dr, "TipoAcciones"),
                            ValorAcciones = GetNullableDecimal(dr, "ValorAcciones"),
                            CotizaBolsa = GetNullableBool(dr, "CotizaBolsa"),
                            TipoCambio = GetNullableDecimal(dr, "TipoCambio"),
                            IdTipoCambio = GetNullableInt(dr, "IdTipoCambio"),
                            Antecedentes = GetNullableString(dr, "Antecedentes"),
                            AspectosLegales = GetNullableString(dr, "AspectosLegales"),
                            ComentariosAspectoLegal = GetNullableString(dr, "ComentariosAspectoLegal"),
                            IdSector = GetNullableInt(dr, "IdSector"),
                            Actividad = GetNullableString(dr, "Actividad"),
                            IdIsicCategoria = GetNullableInt(dr, "IdIsicCategoria"),
                            IdIsicClase = GetNullableInt(dr, "IdIsicClase"),
                            ActividadPrincipal = GetNullableString(dr, "ActividadPrincipal"),
                            VentasContado = GetNullableDecimal(dr, "VentasContado"),
                            VentasContadoText = GetNullableString(dr, "VentasContadoText"),
                            VentasCredito = GetNullableDecimal(dr, "VentasCredito"),
                            VentasCreditoText = GetNullableString(dr, "VentasCreditoText"),
                            IdVentasCreditoTiempo = GetNullableInt(dr, "IdVentasCreditoTiempo"),
                            VentasInternacionales = GetNullableDecimal(dr, "VentasInternacionales"),
                            VentasInternacionalesText = GetNullableString(dr, "VentasInternacionalesText"),
                            VentasNacionales = GetNullableDecimal(dr, "VentasNacionales"),
                            VentasNacionalesText = GetNullableString(dr, "VentasNacionalesText"),
                            ComprasNacionales = GetNullableDecimal(dr, "ComprasNacionales"),
                            ComprasNacionalesText = GetNullableString(dr, "ComprasNacionalesText"),
                            ComprasInternacionales = GetNullableDecimal(dr, "ComprasInternacionales"),
                            ComprasInternacionalesText = GetNullableString(dr, "ComprasInternacionalesText"),
                            ComprasContadoNacionales = GetNullableDecimal(dr, "ComprasContadoNacionales"),
                            ComprasContadoNacionalesText = GetNullableString(dr, "ComprasContadoNacionalesText"),
                            ComprasCreditoNacionales = GetNullableDecimal(dr, "ComprasCreditoNacionales"),
                            ComprasCreditoNacionalesText = GetNullableString(dr, "ComprasCreditoNacionalesText"),
                            IdComprasCreditoNacionalesTiempo = GetNullableInt(dr, "IdComprasCreditoNacionalesTiempo"),
                            ComprasContadoInternacionales = GetNullableDecimal(dr, "ComprasContadoInternacionales"),
                            ComprasContadoInternacionalesText = GetNullableString(dr, "ComprasContadoInternacionalesText"),
                            ComprasCreditoInternacionales = GetNullableDecimal(dr, "ComprasCreditoInternacionales"),
                            ComprasCreditoInternacionalesText = GetNullableString(dr, "ComprasCreditoInternacionalesText"),
                            IdComprasCreditoInternacionalesTiempo = GetNullableInt(dr, "IdComprasCreditoInternacionalesTiempo"),
                            NumeroEmpleados = GetNullableInt(dr, "NumeroEmpleados"),
                            NumeroEmpleadosText = GetNullableString(dr, "NumeroEmpleadosText"),
                            ComentariosOperaciones = GetNullableString(dr, "ComentariosOperaciones"),
                            ContenidoInformacionFinanciera = GetNullableString(dr, "ContenidoInformacionFinanciera"),
                            ComentarioInformacionFinanciera = GetNullableString(dr, "ComentarioInformacionFinanciera"),
                            ActivosFijos = GetNullableString(dr, "ActivosFijos"),
                            Seguros = GetNullableString(dr, "Seguros"),
                            ComentarioProveedor = GetNullableString(dr, "ComentarioProveedor"),
                            ReferenciaBanco = GetNullableString(dr, "ReferenciaBanco"),
                            Litigios = GetNullableString(dr, "Litigios"),
                            RiesgoPrincipal = GetNullableString(dr, "RiesgoPrincipal"),
                            Superintendecia = GetNullableString(dr, "Superintendecia"),
                            InformacionGeneral = GetNullableString(dr, "InformacionGeneral"),
                            OpinionCredito = GetNullableString(dr, "OpinionCredito"),
                            FlgTieneInformacion = GetNullableBool(dr, "FlgTieneInformacion"),
                            IdEstadoInforme = GetNullableInt(dr, "IdEstadoInforme"),
                            IdFormatoFecha = GetNullableInt(dr, "IdFormatoFecha")
                        };
                    }

                    if (informe != null)
                    {
                        // RS3: cabecera de balances
                        var balances = new List<InformeBalanceConsulta>();
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                balances.Add(new InformeBalanceConsulta
                                {
                                    IdInformeBalance = Convert.ToInt32(dr["IdInformeBalance"]),
                                    FechaBalance = Convert.ToDateTime(dr["FechaBalance"]),
                                    FechaHasta = GetNullableDateTime(dr, "FechaHasta"),
                                    FlgActualidad = Convert.ToBoolean(dr["FlgActualidad"]),
                                    TipoCambio = GetNullableDecimal(dr, "TipoCambio"),
                                    IdMoneda = Convert.ToInt32(dr["IdMoneda"]),
                                    IdTipoBalance = Convert.ToInt32(dr["IdTipoBalance"]),
                                    IdTipoEstadoFinanciero = GetNullableInt(dr, "IdTipoEstadoFinanciero")
                                });
                            }
                        }

                        // RS4-RS8: detalle de balance por tipo (desagregado/totalizado/banco/seguro/turquia)
                        var detalleDesagregado = await dr.NextResultAsync() ? await LeerDetalleBalanceAsync(dr) : new();
                        var detalleTotalizado = await dr.NextResultAsync() ? await LeerDetalleBalanceAsync(dr) : new();
                        var detalleBanco = await dr.NextResultAsync() ? await LeerDetalleBalanceAsync(dr) : new();
                        var detalleSeguro = await dr.NextResultAsync() ? await LeerDetalleBalanceAsync(dr) : new();
                        var detalleTurquia = await dr.NextResultAsync() ? await LeerDetalleBalanceAsync(dr) : new();

                        foreach (var balance in balances)
                        {
                            var detalle = balance.IdTipoEstadoFinanciero switch
                            {
                                1 => detalleDesagregado,
                                2 => detalleTotalizado,
                                3 => detalleBanco,
                                4 => detalleSeguro,
                                5 => detalleTurquia,
                                _ => null
                            };
                            if (detalle != null && detalle.TryGetValue(balance.IdInformeBalance, out var cuenta))
                                balance.CuentaBalance = cuenta;
                        }
                        informe.Balances = balances;

                        // RS9: bancos
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                                informe.Bancos.Add(new InformeBancoConsulta
                                {
                                    IdIformeBanco = Convert.ToInt32(dr["IdInformeBanco"]),
                                    IdBanco = Convert.ToInt32(dr["IdBanco"]),
                                    NumeroCuenta = GetNullableString(dr, "NumeroCuenta"),
                                    IdSector = GetNullableInt(dr, "IdSector"),
                                    Sectorista = GetNullableString(dr, "Sectorista"),
                                    ReferenciaBanco = GetNullableString(dr, "ReferenciaBanco")
                                });
                        }

                        // RS10: companias relacionadas
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                                informe.CompaniasRelacionadas.Add(new InformeCompaniaRelacionadaConsulta
                                {
                                    IdInformeCompaniaRelacionada = Convert.ToInt32(dr["IdInformeCompaniaRelacionada"]),
                                    IdCompania = Convert.ToInt32(dr["IdCompania"])
                                });
                        }

                        // RS11: exportaciones / importaciones
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                                informe.ExportacionesImportaciones.Add(new InformeExportacionImportacionConsulta
                                {
                                    IdInformeExportacionImportacion = Convert.ToInt32(dr["IdInformeExportacionImportacion"]),
                                    Anio = Convert.ToInt32(dr["Anio"]),
                                    MesInicio = Convert.ToInt32(dr["MesInicio"]),
                                    MesFin = Convert.ToInt32(dr["MesFin"]),
                                    IdMoneda = Convert.ToInt32(dr["IdMoneda"]),
                                    Paises = GetNullableString(dr, "Paises"),
                                    Monto = GetNullableDecimal(dr, "Monto"),
                                    Productos = GetNullableString(dr, "Productos"),
                                    IdTipoOperacion = Convert.ToInt32(dr["IdTipoOperacion"]),
                                    NumOperaciones = GetNullableInt(dr, "NumOperaciones")
                                });
                        }

                        // RS12: proveedores
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                                informe.Proveedores.Add(new InformeProveedorConsulta
                                {
                                    IdInformeProveedor = Convert.ToInt32(dr["IdInformeProveedor"]),
                                    IdBancoProveedor = GetNullableInt(dr, "IdBancoProveedor"),
                                    IdTipoPersona = Convert.ToInt32(dr["IdTipoPersona"]),
                                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                                    IdPais = GetNullableInt(dr, "IdPais"),
                                    IdTipoDocumento = GetNullableInt(dr, "IdTipoDocumento"),
                                    NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                                    IdMoneda = GetNullableInt(dr, "IdMoneda"),
                                    FechaInicio = GetNullableDateTime(dr, "FechaInicio"),
                                    IdLimiteCredito = GetNullableInt(dr, "IdLimiteCredito"),
                                    PromedioMensual = GetNullableDecimal(dr, "PromedioMensual"),
                                    PlazoCredito = GetNullableString(dr, "PlazoCredito"),
                                    Productos = GetNullableString(dr, "Productos"),
                                    IdCalificacion = GetNullableInt(dr, "IdCalificacion"),
                                    Comentarios = GetNullableString(dr, "Comentarios"),
                                    NombreContacto = GetNullableString(dr, "NombreContacto"),
                                    Telefono = GetNullableString(dr, "Telefono"),
                                    ComienzoNegociaciones = GetNullableString(dr, "ComienzoNegociaciones"),
                                    IdPlazoCredito = GetNullableInt(dr, "IdPlazoCredito"),
                                    EsTieneReferenciaComercial = GetNullableBool(dr, "EsTieneReferenciaComercial"),
                                    TipoCambio = GetNullableDecimal(dr, "TipoCambio")
                                });
                        }

                        // RS13: directorio ejecutivo
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                                informe.DirectoriosEjecutivos.Add(new InformeDirectorioEjecutivoConsulta
                                {
                                    IdInformeDirectorioEjecutivo = Convert.ToInt32(dr["IdInformeDirectorioEjecutivo"]),
                                    IdCargo = GetNullableInt(dr, "IdCargo"),
                                    VinculadoDesde = GetNullableDateTime(dr, "VinculadoDesde"),
                                    CompaniaAnterior = GetNullableString(dr, "CompaniaAnterior"),
                                    Participacion = GetNullableDecimal(dr, "Participacion"),
                                    Orden = GetNullableInt(dr, "Orden"),
                                    EsParticipanteDirectiva = GetNullableBool(dr, "EsParticipanteDirectiva"),
                                    ApareceImpresoLista = GetNullableBool(dr, "ApareceImpresoLista"),
                                    ImprimeDatosEjecutivos = GetNullableBool(dr, "ImprimeDatosEjecutivos"),
                                    IdDirectorioEjecutivo = Convert.ToInt32(dr["IdDirectorioEjecutivo"]),
                                    IdTipoPersona = GetNullableInt(dr, "IdTipoPersona"),
                                    NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                                    IdPais = GetNullableInt(dr, "IdPais"),
                                    Direccion = GetNullableString(dr, "Direccion"),
                                    Ubigeo = GetNullableString(dr, "Ubigeo"),
                                    CodigoPostal = GetNullableString(dr, "CodigoPostal"),
                                    IdTipoDocumento = GetNullableInt(dr, "IdTipoDocumento"),
                                    NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                                    TaxIdType = GetNullableInt(dr, "TaxIdType"),
                                    TaxNum = GetNullableString(dr, "TaxNum"),
                                    IdNacionalidad = GetNullableInt(dr, "IdNacionalidad"),
                                    FechaNacimiento = GetNullableDateTime(dr, "FechaNacimiento"),
                                    IdEstadoCivil = GetNullableInt(dr, "IdEstadoCivil"),
                                    IdProfesion = GetNullableInt(dr, "IdProfesion"),
                                    Referencias = GetNullableString(dr, "Referencias")
                                });
                        }

                        // RS14: archivos adjuntos
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                                informe.Archivos.Add(new InformeArchivoResumen
                                {
                                    IdInformeArchivo = Convert.ToInt32(dr["IdInformeArchivo"]),
                                    Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                                    Extension = dr["Extension"]?.ToString() ?? string.Empty,
                                    TamanoBytes = Convert.ToInt64(dr["TamanoBytes"]),
                                    IdTipoArchivo = Convert.ToInt32(dr["IdTipoArchivo"]),
                                    IdFaseEvidencia = Convert.ToInt32(dr["IdFaseEvidencia"])
                                });
                        }

                        // RS15: locales
                        var localesPorId = new Dictionary<int, InformeLocalConsulta>();
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                var local = new InformeLocalConsulta
                                {
                                    IdInformeLocal = Convert.ToInt32(dr["IdInformeLocal"]),
                                    IdTipoLocal = GetNullableInt(dr, "IdTipoLocal"),
                                    Comentario = GetNullableString(dr, "Comentario")
                                };
                                localesPorId[local.IdInformeLocal] = local;
                                informe.Locales.Add(local);
                            }
                        }

                        // RS16: imagenes de locales (correlacionadas por IdInformeLocal)
                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                var idInformeLocal = Convert.ToInt32(dr["IdInformeLocal"]);
                                if (localesPorId.TryGetValue(idInformeLocal, out var local))
                                    local.Imagenes.Add(new InformeLocalImagenConsulta
                                    {
                                        IdInformeLocalImagen = Convert.ToInt32(dr["IdInformeLocalImagen"]),
                                        ImagenURL = dr["ImagenURL"]?.ToString() ?? string.Empty,
                                        IdTipoArchivo = Convert.ToInt32(dr["IdTipoArchivo"]),
                                        Nombre = GetNullableString(dr, "Nombre")
                                    });
                            }
                        }

                        lista.Add(informe);
                    }
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeConsulta>() };
            }
        }

        public async Task<(Respuesta respuesta, string? nombreInforme)> GenerarDocumentoAsync(UsuarioGeneral u, int idInforme, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_GenerarDocumento", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                await cn.OpenAsync();

                var respuesta = new Respuesta();
                string? nombreInforme = null;
                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 3;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                    respuesta.Result = dr["Result"]?.ToString();
                    nombreInforme = dr["NombreInforme"]?.ToString();
                }
                else
                {
                    _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", cmd.CommandText);

                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                }
                return (respuesta, nombreInforme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return (new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message }, null);
            }
        }

        public async Task<(Respuesta respuesta, string? nombreInforme)> GenerarDocumentoXmlAsync(UsuarioGeneral u, int idInforme, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_GenerarDocumentoXml", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                await cn.OpenAsync();

                var respuesta = new Respuesta();
                string? nombreInforme = null;
                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 3;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                    respuesta.Result = dr["Result"]?.ToString();
                    nombreInforme = dr["NombreInforme"]?.ToString();
                }
                else
                {
                    _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", cmd.CommandText);

                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                }
                return (respuesta, nombreInforme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return (new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message }, null);
            }
        }

        public async Task<Respuesta> ObtenerDocumentoAsync(UsuarioGeneral u, int idInforme, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_ObtenerDocumento", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeDocumentoResult>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    lista.Add(new InformeDocumentoResult
                    {
                        UrlDocumento = dr["UrlDocumento"]?.ToString() ?? string.Empty,
                        Nombre = dr["Nombre"]?.ToString() ?? string.Empty
                    });
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeDocumentoResult>() };
            }
        }

        public async Task<Respuesta> ActualizarEstadoAsync(UsuarioGeneral u, int idInforme, int idEstadoInforme)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_ActualizarEstado", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdEstadoInforme", SqlDbType.Int).Value = idEstadoInforme;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                return await LeerCabeceraAsync(dr, cmd.CommandText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ObtenerRutaDocumentoAsync(UsuarioGeneral u, int idInforme, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_ObtenerRutaDocumento", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    respuesta.Result = dr["UrlDocumento"]?.ToString();

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = null };
            }
        }

        public async Task<Respuesta> ActualizarDocumentoAsync(UsuarioGeneral u, int idInforme, string urlDocumento)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_ActualizarDocumento", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@vchUrlDocumento", SqlDbType.VarChar, 500).Value = urlDocumento;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                return await LeerCabeceraAsync(dr, cmd.CommandText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral u, FiltroInforme filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Listar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = (object?)filtro.IdPedido ?? DBNull.Value;
                cmd.Parameters.Add("@vchIdEstado", SqlDbType.VarChar, 255).Value = (object?)filtro.IdEstado ?? DBNull.Value;
                cmd.Parameters.Add("@vchIdPlantilla", SqlDbType.VarChar, 255).Value = (object?)filtro.IdPlantilla ?? DBNull.Value;
                cmd.Parameters.Add("@vchIdTipoTramite", SqlDbType.VarChar, 255).Value = (object?)filtro.IdTipoTramite ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)filtro.NumPag ?? DBNull.Value;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new InformeListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                        resultado.Asignado = Convert.ToInt32(dr["Asignado"]);
                        resultado.Rechazado = Convert.ToInt32(dr["Rechazado"]);
                        resultado.EnProceso = Convert.ToInt32(dr["EnProceso"]);
                        resultado.Aprobado = Convert.ToInt32(dr["Aprobado"]);
                        resultado.PendienteAprobacion = Convert.ToInt32(dr["PendienteAprobacion"]);
                        resultado.Vigente = Convert.ToInt32(dr["Vigente"]);
                        resultado.Vencido = Convert.ToInt32(dr["Vencido"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstInformes.Add(new InformeListaConsulta
                            {
                                IdInforme = Convert.ToInt32(dr["IdInforme"]),
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                IdFase = GetNullableInt(dr, "IdFase"),
                                IdPlantilla = GetNullableInt(dr, "IdPlantilla"),
                                EstadoInforme = GetNullableString(dr, "EstadoInforme"),
                                Investigado = GetNullableString(dr, "Investigado"),
                                Vigencia = GetNullableString(dr, "Vigencia"),
                                TipoTramite = GetNullableString(dr, "TipoTramite"),
                                IdInformeOriginal = GetNullableInt(dr, "IdInformeOriginal"),
                                RequiereTraduccion = GetNullableInt(dr, "RequiereTraduccion")
                            });
                        }
                    }
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new InformeListaResult() };
            }
        }

        public async Task<Respuesta> ListarIdPorCompaniaAsync(UsuarioGeneral u, FiltroInformeIdPorCompania filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_ListarIdPorCompania", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = filtro.IdCompania;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)filtro.NumPag ?? DBNull.Value;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new InformeIdPorCompaniaListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstInformes.Add(new InformeIdPorCompaniaConsulta
                            {
                                IdInforme = Convert.ToInt32(dr["IdInforme"]),
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                Idioma = GetNullableString(dr, "Idioma"),
                                Nombre = GetNullableString(dr, "Nombre"),
                                FchCre = GetNullableString(dr, "FchCre")
                            });
                        }
                    }
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new InformeIdPorCompaniaListaResult() };
            }
        }

        public async Task<Respuesta> CalcularBalanceDesagregadoAsync(UsuarioGeneral u, InformeBalanceDesagregadoCalcularRequest r)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Balance_Desagregado_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario",                              SqlDbType.Int).Value     = u.IdUsuario;
                cmd.Parameters.Add("@vchUsuario",                                SqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@intIdEmpresa",                              SqlDbType.Int).Value     = u.IdEmpresa;
                cmd.Parameters.Add("@intIdRol",                                  SqlDbType.Int).Value     = u.IdRol;
                cmd.Parameters.Add("@decEfectivoEquivalente",                    SqlDbType.Decimal).Value = D2(r.EfectivoEquivalente);
                cmd.Parameters.Add("@decOtrosActivosFinancierosCorriente",       SqlDbType.Decimal).Value = D2(r.OtrosActivosFinancierosCorriente);
                cmd.Parameters.Add("@decCuentasCobrarCorriente",                 SqlDbType.Decimal).Value = D2(r.CuentasCobrarCorriente);
                cmd.Parameters.Add("@decInventariosCorriente",                   SqlDbType.Decimal).Value = D2(r.InventariosCorriente);
                cmd.Parameters.Add("@decActivosBiologicosCorriente",             SqlDbType.Decimal).Value = D2(r.ActivosBiologicosCorriente);
                cmd.Parameters.Add("@decActivosImpuestosGanancias",              SqlDbType.Decimal).Value = D2(r.ActivosImpuestosGanancias);
                cmd.Parameters.Add("@decOtrosActivosNoFinancierosCorriente",     SqlDbType.Decimal).Value = D2(r.OtrosActivosNoFinancierosCorriente);
                cmd.Parameters.Add("@decOtrosActivosFinancierosNoCorriente",     SqlDbType.Decimal).Value = D2(r.OtrosActivosFinancierosNoCorriente);
                cmd.Parameters.Add("@decInversionesSubsidiarias",                SqlDbType.Decimal).Value = D2(r.InversionesSubsidiarias);
                cmd.Parameters.Add("@decCuentasCobrarNoCorriente",               SqlDbType.Decimal).Value = D2(r.CuentasCobrarNoCorriente);
                cmd.Parameters.Add("@decInventariosNoCorriente",                 SqlDbType.Decimal).Value = D2(r.InventariosNoCorriente);
                cmd.Parameters.Add("@decActivosBiologicosNoCorriente",           SqlDbType.Decimal).Value = D2(r.ActivosBiologicosNoCorriente);
                cmd.Parameters.Add("@decPropiedadesInversion",                   SqlDbType.Decimal).Value = D2(r.PropiedadesInversion);
                cmd.Parameters.Add("@decPropiedadesPlantaEquipo",                SqlDbType.Decimal).Value = D2(r.PropiedadesPlantaEquipo);
                cmd.Parameters.Add("@decIntangibles",                            SqlDbType.Decimal).Value = D2(r.Intangibles);
                cmd.Parameters.Add("@decActivosImpuestosDiferidos",              SqlDbType.Decimal).Value = D2(r.ActivosImpuestosDiferidos);
                cmd.Parameters.Add("@decActivosImpuestosCorrientes",             SqlDbType.Decimal).Value = D2(r.ActivosImpuestosCorrientes);
                cmd.Parameters.Add("@decPlusvalia",                              SqlDbType.Decimal).Value = D2(r.Plusvalia);
                cmd.Parameters.Add("@decOtrosActivosNoFinancierosNoCorriente",   SqlDbType.Decimal).Value = D2(r.OtrosActivosNoFinancierosNoCorriente);
                cmd.Parameters.Add("@decOtrosPasivosFinancierosCorriente",       SqlDbType.Decimal).Value = D2(r.OtrosPasivosFinancierosCorriente);
                cmd.Parameters.Add("@decCuentasPagarCorriente",                  SqlDbType.Decimal).Value = D2(r.CuentasPagarCorriente);
                cmd.Parameters.Add("@decBeneficiosEmpleadosCorriente",           SqlDbType.Decimal).Value = D2(r.BeneficiosEmpleadosCorriente);
                cmd.Parameters.Add("@decOtrasProvisionesCorriente",              SqlDbType.Decimal).Value = D2(r.OtrasProvisionesCorriente);
                cmd.Parameters.Add("@decImpuestosGananciasCorriente",            SqlDbType.Decimal).Value = D2(r.ImpuestosGananciasCorriente);
                cmd.Parameters.Add("@decOtrosPasivosNoFinancierosCorriente",     SqlDbType.Decimal).Value = D2(r.OtrosPasivosNoFinancierosCorriente);
                cmd.Parameters.Add("@decOtrosPasivosFinancierosNoCorriente",     SqlDbType.Decimal).Value = D2(r.OtrosPasivosFinancierosNoCorriente);
                cmd.Parameters.Add("@decCuentasPagarNoCorriente",                SqlDbType.Decimal).Value = D2(r.CuentasPagarNoCorriente);
                cmd.Parameters.Add("@decBeneficiosEmpleadosNoCorriente",         SqlDbType.Decimal).Value = D2(r.BeneficiosEmpleadosNoCorriente);
                cmd.Parameters.Add("@decOtrasProvisionesNoCorriente",            SqlDbType.Decimal).Value = D2(r.OtrasProvisionesNoCorriente);
                cmd.Parameters.Add("@decImpuestosDiferidosNoCorriente",          SqlDbType.Decimal).Value = D2(r.ImpuestosDiferidosNoCorriente);
                cmd.Parameters.Add("@decImpuestosCorrientesNoCorriente",         SqlDbType.Decimal).Value = D2(r.ImpuestosCorrientesNoCorriente);
                cmd.Parameters.Add("@decOtrosPasivosNoFinancierosNoCorriente",   SqlDbType.Decimal).Value = D2(r.OtrosPasivosNoFinancierosNoCorriente);
                cmd.Parameters.Add("@decCapitalEmitido",                         SqlDbType.Decimal).Value = D2(r.CapitalEmitido);
                cmd.Parameters.Add("@decPrimasEmision",                          SqlDbType.Decimal).Value = D2(r.PrimasEmision);
                cmd.Parameters.Add("@decAccionesInversion",                      SqlDbType.Decimal).Value = D2(r.AccionesInversion);
                cmd.Parameters.Add("@decAccionesCartera",                        SqlDbType.Decimal).Value = D2(r.AccionesCartera);
                cmd.Parameters.Add("@decOtrasReservasCapital",                   SqlDbType.Decimal).Value = D2(r.OtrasReservasCapital);
                cmd.Parameters.Add("@decResultadosAcumulados",                   SqlDbType.Decimal).Value = D2(r.ResultadosAcumulados);
                cmd.Parameters.Add("@decOtrasReservasPatrimonio",                SqlDbType.Decimal).Value = D2(r.OtrasReservasPatrimonio);
                cmd.Parameters.Add("@decIngresosOrdinarios",                     SqlDbType.Decimal).Value = D2(r.IngresosOrdinarios);
                cmd.Parameters.Add("@decCostoVentas",                            SqlDbType.Decimal).Value = D2(r.CostoVentas);
                cmd.Parameters.Add("@decGastosVentas",                           SqlDbType.Decimal).Value = D2(r.GastosVentas);
                cmd.Parameters.Add("@decGastosAdministracion",                   SqlDbType.Decimal).Value = D2(r.GastosAdministracion);
                cmd.Parameters.Add("@decOtrosIngresosOperativos",                SqlDbType.Decimal).Value = D2(r.OtrosIngresosOperativos);
                cmd.Parameters.Add("@decOtrosGastosOperativos",                  SqlDbType.Decimal).Value = D2(r.OtrosGastosOperativos);
                cmd.Parameters.Add("@decOtrasGananciasPerdidas",                 SqlDbType.Decimal).Value = D2(r.OtrasGananciasPerdidas);
                cmd.Parameters.Add("@decIngresosFinancieros",                    SqlDbType.Decimal).Value = D2(r.IngresosFinancieros);
                cmd.Parameters.Add("@decIngresosIntereses",                      SqlDbType.Decimal).Value = D2(r.IngresosIntereses);
                cmd.Parameters.Add("@decGastosFinancieros",                      SqlDbType.Decimal).Value = D2(r.GastosFinancieros);
                cmd.Parameters.Add("@decDeterioroValor",                         SqlDbType.Decimal).Value = D2(r.DeterioroValor);
                cmd.Parameters.Add("@decOtrosIngresosSubsidiarias",              SqlDbType.Decimal).Value = D2(r.OtrosIngresosSubsidiarias);
                cmd.Parameters.Add("@decDiferenciasCambio",                      SqlDbType.Decimal).Value = D2(r.DiferenciasCambio);
                cmd.Parameters.Add("@decIngresoGastoImpuesto",                   SqlDbType.Decimal).Value = D2(r.IngresoGastoImpuesto);
                cmd.Parameters.Add("@decOperacionesDescontinuadas",              SqlDbType.Decimal).Value = D2(r.OperacionesDescontinuadas);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new List<InformeBalanceDesagregadoCalculado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    resultado.Add(new InformeBalanceDesagregadoCalculado
                    {
                        TotalActivoCorriente = Convert.ToDecimal(dr["TotalActivoCorriente"]),
                        TotalActivoNoCorriente = Convert.ToDecimal(dr["TotalActivoNoCorriente"]),
                        TotalActivo = Convert.ToDecimal(dr["TotalActivo"]),
                        TotalPasivoCorriente = Convert.ToDecimal(dr["TotalPasivoCorriente"]),
                        TotalPasivoNoCorriente = Convert.ToDecimal(dr["TotalPasivoNoCorriente"]),
                        TotalPasivos = Convert.ToDecimal(dr["TotalPasivos"]),
                        TotalPatrimonio = Convert.ToDecimal(dr["TotalPatrimonio"]),
                        TotalPasivoPatrimonio = Convert.ToDecimal(dr["TotalPasivoPatrimonio"]),
                        GananciaBruta = Convert.ToDecimal(dr["GananciaBruta"]),
                        GananciaOperativa = Convert.ToDecimal(dr["GananciaOperativa"]),
                        GananciaAntesImpuestos = Convert.ToDecimal(dr["GananciaAntesImpuestos"]),
                        GananciaNeta = Convert.ToDecimal(dr["GananciaNeta"]),
                        IndiceLiquidez = GetNullableDecimal(dr, "IndiceLiquidez"),
                        CapitalTrabajo = Convert.ToDecimal(dr["CapitalTrabajo"]),
                        RatioEndeudamiento = GetNullableDecimal(dr, "RatioEndeudamiento"),
                        RatioRentabilidad = GetNullableDecimal(dr, "RatioRentabilidad")
                    });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeBalanceDesagregadoCalculado>() };
            }
        }

        public async Task<Respuesta> CalcularBalanceSeguroAsync(UsuarioGeneral u, InformeBalanceSeguroCalcularRequest r)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Balance_Seguro_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario",                          SqlDbType.Int).Value        = u.IdUsuario;
                cmd.Parameters.Add("@vchUsuario",                            SqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@intIdEmpresa",                          SqlDbType.Int).Value        = u.IdEmpresa;
                cmd.Parameters.Add("@intIdRol",                              SqlDbType.Int).Value        = u.IdRol;
                cmd.Parameters.Add("@decEfectivoDisponible",                 SqlDbType.Decimal).Value    = D2(r.EfectivoDisponible);
                cmd.Parameters.Add("@decInversionesFinancieras",             SqlDbType.Decimal).Value    = D2(r.InversionesFinancieras);
                cmd.Parameters.Add("@decPrestamosInteresesNetos",            SqlDbType.Decimal).Value    = D2(r.PrestamosInteresesNetos);
                cmd.Parameters.Add("@decPrimasCobrar",                       SqlDbType.Decimal).Value    = D2(r.PrimasCobrar);
                cmd.Parameters.Add("@decDeudasReaseguradores",               SqlDbType.Decimal).Value    = D2(r.DeudasReaseguradores);
                cmd.Parameters.Add("@decActivosVenta",                       SqlDbType.Decimal).Value    = D2(r.ActivosVenta);
                cmd.Parameters.Add("@decPropiedadesInversion",               SqlDbType.Decimal).Value    = D2(r.PropiedadesInversion);
                cmd.Parameters.Add("@decPropiedadPlantaEquipo",              SqlDbType.Decimal).Value    = D2(r.PropiedadPlantaEquipo);
                cmd.Parameters.Add("@decOtrosActivos",                       SqlDbType.Decimal).Value    = D2(r.OtrosActivos);
                cmd.Parameters.Add("@decObligacionesAsegurados",             SqlDbType.Decimal).Value    = D2(r.ObligacionesAsegurados);
                cmd.Parameters.Add("@decReservasSiniestros",                 SqlDbType.Decimal).Value    = D2(r.ReservasSiniestros);
                cmd.Parameters.Add("@decReservasTecnicas",                   SqlDbType.Decimal).Value    = D2(r.ReservasTecnicas);
                cmd.Parameters.Add("@decObligacionesReaseguradores",         SqlDbType.Decimal).Value    = D2(r.ObligacionesReaseguradores);
                cmd.Parameters.Add("@decObligacionesFinancieras",            SqlDbType.Decimal).Value    = D2(r.ObligacionesFinancieras);
                cmd.Parameters.Add("@decCuentasPagar",                       SqlDbType.Decimal).Value    = D2(r.CuentasPagar);
                cmd.Parameters.Add("@decOtrosPasivos",                       SqlDbType.Decimal).Value    = D2(r.OtrosPasivos);
                cmd.Parameters.Add("@decCapitalSocial",                      SqlDbType.Decimal).Value    = D2(r.CapitalSocial);
                cmd.Parameters.Add("@decAportesCapitalNoCapitalizados",      SqlDbType.Decimal).Value    = D2(r.AportesCapitalNoCapitalizados);
                cmd.Parameters.Add("@decResultadosAcumulados",               SqlDbType.Decimal).Value    = D2(r.ResultadosAcumulados);
                cmd.Parameters.Add("@decPatrimonioRestringido",              SqlDbType.Decimal).Value    = D2(r.PatrimonioRestringido);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new List<InformeBalanceSeguroCalculado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    resultado.Add(new InformeBalanceSeguroCalculado
                    {
                        TotalActivos = Convert.ToDecimal(dr["TotalActivos"]),
                        TotalPasivo = Convert.ToDecimal(dr["TotalPasivo"]),
                        TotalPatrimonio = Convert.ToDecimal(dr["TotalPatrimonio"]),
                        TotalPasivoPatrimonio = Convert.ToDecimal(dr["TotalPasivoPatrimonio"])
                    });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeBalanceSeguroCalculado>() };
            }
        }

        public async Task<Respuesta> CalcularBalanceBancoAsync(UsuarioGeneral u, InformeBalanceBancoCalcularRequest r)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Balance_Banco_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario",                      SqlDbType.Int).Value        = u.IdUsuario;
                cmd.Parameters.Add("@vchUsuario",                        SqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@intIdEmpresa",                      SqlDbType.Int).Value        = u.IdEmpresa;
                cmd.Parameters.Add("@intIdRol",                          SqlDbType.Int).Value        = u.IdRol;
                cmd.Parameters.Add("@decDisponible",                     SqlDbType.Decimal).Value    = D2(r.Disponible);
                cmd.Parameters.Add("@decFondosInterbancarios",           SqlDbType.Decimal).Value    = D2(r.FondosInterbancarios);
                cmd.Parameters.Add("@decInversionesValorRazonable",      SqlDbType.Decimal).Value    = D2(r.InversionesValorRazonable);
                cmd.Parameters.Add("@decCarteraCreditos",                SqlDbType.Decimal).Value    = D2(r.CarteraCreditos);
                cmd.Parameters.Add("@decDerivadosNegociacionActivo",     SqlDbType.Decimal).Value    = D2(r.DerivadosNegociacionActivo);
                cmd.Parameters.Add("@decDerivadosCoberturaActivo",       SqlDbType.Decimal).Value    = D2(r.DerivadosCoberturaActivo);
                cmd.Parameters.Add("@decBienesRealizables",              SqlDbType.Decimal).Value    = D2(r.BienesRealizables);
                cmd.Parameters.Add("@decParticipacionesSubsidiarias",    SqlDbType.Decimal).Value    = D2(r.ParticipacionesSubsidiarias);
                cmd.Parameters.Add("@decInmuebleMobiliarioEquipo",       SqlDbType.Decimal).Value    = D2(r.InmuebleMobiliarioEquipo);
                cmd.Parameters.Add("@decImpuestoRentaDiferido",          SqlDbType.Decimal).Value    = D2(r.ImpuestoRentaDiferido);
                cmd.Parameters.Add("@decOtrosActivos",                   SqlDbType.Decimal).Value    = D2(r.OtrosActivos);
                cmd.Parameters.Add("@decObligacionesPublico",            SqlDbType.Decimal).Value    = D2(r.ObligacionesPublico);
                cmd.Parameters.Add("@decFondosInterbancariosPasivo",     SqlDbType.Decimal).Value    = D2(r.FondosInterbancariosPasivo);
                cmd.Parameters.Add("@decAdeudosFinancieras",             SqlDbType.Decimal).Value    = D2(r.AdeudosFinancieras);
                cmd.Parameters.Add("@decDerivadosNegociacionPasivo",     SqlDbType.Decimal).Value    = D2(r.DerivadosNegociacionPasivo);
                cmd.Parameters.Add("@decDerivadosCoberturaPasivo",       SqlDbType.Decimal).Value    = D2(r.DerivadosCoberturaPasivo);
                cmd.Parameters.Add("@decCuentasPagarProvisiones",        SqlDbType.Decimal).Value    = D2(r.CuentasPagarProvisiones);
                cmd.Parameters.Add("@decCapitalSocial",                  SqlDbType.Decimal).Value    = D2(r.CapitalSocial);
                cmd.Parameters.Add("@decReservas",                       SqlDbType.Decimal).Value    = D2(r.Reservas);
                cmd.Parameters.Add("@decResultadosNoRealizados",         SqlDbType.Decimal).Value    = D2(r.ResultadosNoRealizados);
                cmd.Parameters.Add("@decResultadoEjercicio",             SqlDbType.Decimal).Value    = D2(r.ResultadoEjercicio);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new List<InformeBalanceBancoCalculado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    resultado.Add(new InformeBalanceBancoCalculado
                    {
                        TotalActivos = Convert.ToDecimal(dr["TotalActivos"]),
                        TotalPasivo = Convert.ToDecimal(dr["TotalPasivo"]),
                        TotalPatrimonio = Convert.ToDecimal(dr["TotalPatrimonio"]),
                        TotalPasivoPatrimonio = Convert.ToDecimal(dr["TotalPasivoPatrimonio"])
                    });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeBalanceBancoCalculado>() };
            }
        }

        public async Task<Respuesta> CalcularBalanceTurquiaAsync(UsuarioGeneral u, InformeBalanceTurquiaCalcularRequest r)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Balance_Turquia_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario",                SqlDbType.Int).Value         = u.IdUsuario;
                cmd.Parameters.Add("@vchUsuario",                  SqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@intIdEmpresa",                SqlDbType.Int).Value         = u.IdEmpresa;
                cmd.Parameters.Add("@intIdRol",                    SqlDbType.Int).Value         = u.IdRol;
                cmd.Parameters.Add("@decEfectivo",                 SqlDbType.Decimal).Value     = D2(r.Efectivo);
                cmd.Parameters.Add("@decExistencias",              SqlDbType.Decimal).Value     = D2(r.Existencias);
                cmd.Parameters.Add("@decDeudores",                 SqlDbType.Decimal).Value     = D2(r.Deudores);
                cmd.Parameters.Add("@decBienesTongibles",          SqlDbType.Decimal).Value     = D2(r.BienesTongibles);
                cmd.Parameters.Add("@decActivosIntangibles",       SqlDbType.Decimal).Value     = D2(r.ActivosIntangibles);
                cmd.Parameters.Add("@decPrestamos",                SqlDbType.Decimal).Value     = D2(r.Prestamos);
                cmd.Parameters.Add("@decAcreedores",               SqlDbType.Decimal).Value     = D2(r.Acreedores);
                cmd.Parameters.Add("@decPasivosNoCorrientes",      SqlDbType.Decimal).Value     = D2(r.PasivosNoCorrientes);
                cmd.Parameters.Add("@decPasivosLargoPlazo",        SqlDbType.Decimal).Value     = D2(r.PasivosLargoPlazo);
                cmd.Parameters.Add("@decPatrimonio",               SqlDbType.Decimal).Value     = D2(r.Patrimonio);
                cmd.Parameters.Add("@decReservas",                 SqlDbType.Decimal).Value     = D2(r.Reservas);
                cmd.Parameters.Add("@decResultadosAcumulados",     SqlDbType.Decimal).Value     = D2(r.ResultadosAcumulados);
                cmd.Parameters.Add("@decPerdidaGanancias",         SqlDbType.Decimal).Value     = D2(r.PerdidaGanancias);
                cmd.Parameters.Add("@decOtrasCuentas",             SqlDbType.Decimal).Value     = D2(r.OtrasCuentas);
                cmd.Parameters.Add("@decVentasNetas",              SqlDbType.Decimal).Value     = D2(r.VentasNetas);
                cmd.Parameters.Add("@decCostoVentas",              SqlDbType.Decimal).Value     = D2(r.CostoVentas);
                cmd.Parameters.Add("@decOtrosGastosOperativos",    SqlDbType.Decimal).Value     = D2(r.OtrosGastosOperativos);
                cmd.Parameters.Add("@decCostoEmpleados",           SqlDbType.Decimal).Value     = D2(r.CostoEmpleados);
                cmd.Parameters.Add("@decDepreciacion",             SqlDbType.Decimal).Value     = D2(r.Depreciacion);
                cmd.Parameters.Add("@decIngresosFinancieros",      SqlDbType.Decimal).Value     = D2(r.IngresosFinancieros);
                cmd.Parameters.Add("@decGastosFinancieros",        SqlDbType.Decimal).Value     = D2(r.GastosFinancieros);
                cmd.Parameters.Add("@decIngresosExtraordinarios",  SqlDbType.Decimal).Value     = D2(r.IngresosExtraordinarios);
                cmd.Parameters.Add("@decGastosExtraordinarios",    SqlDbType.Decimal).Value     = D2(r.GastosExtraordinarios);
                cmd.Parameters.Add("@decImpuestos",                SqlDbType.Decimal).Value     = D2(r.Impuestos);
                cmd.Parameters.Add("@decCostoMateriales",          SqlDbType.Decimal).Value     = D2(r.CostoMateriales);
                cmd.Parameters.Add("@decInteresesPagados",         SqlDbType.Decimal).Value     = D2(r.InteresesPagados);
                cmd.Parameters.Add("@decCapital",                  SqlDbType.Decimal).Value     = D2(r.Capital);
                cmd.Parameters.Add("@decEbit",                     SqlDbType.Decimal).Value     = D2(r.Ebit);
                cmd.Parameters.Add("@decEbitda",                   SqlDbType.Decimal).Value     = D2(r.Ebitda);
                cmd.Parameters.Add("@decGanancia",                 SqlDbType.Decimal).Value     = D2(r.Ganancia);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new List<InformeBalanceTurquiaCalculado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    resultado.Add(new InformeBalanceTurquiaCalculado
                    {
                        TotalCorriente = GetNullableDecimal(dr, "TotalCorriente"),
                        ActivoFijoNeto = GetNullableDecimal(dr, "ActivoFijoNeto"),
                        TotalActivos = GetNullableDecimal(dr, "TotalActivos"),
                        PasivosCorrientes = GetNullableDecimal(dr, "PasivosCorrientes"),
                        TotalPasivosNoCorrientes = GetNullableDecimal(dr, "TotalPasivosNoCorrientes"),
                        TotalPasivos = GetNullableDecimal(dr, "TotalPasivos"),
                        TotalPatrimonio = GetNullableDecimal(dr, "TotalPatrimonio"),
                        TotalPasivosPatrimonio = GetNullableDecimal(dr, "TotalPasivosPatrimonio"),
                        GananciaBruta = GetNullableDecimal(dr, "GananciaBruta"),
                        PlFinanciero = GetNullableDecimal(dr, "PlFinanciero"),
                        PlExtraordinario = GetNullableDecimal(dr, "PlExtraordinario"),
                        GananciaAntesImpuestos = GetNullableDecimal(dr, "GananciaAntesImpuestos"),
                        GananciaNeta = GetNullableDecimal(dr, "GananciaNeta"),
                        Ebit = GetNullableDecimal(dr, "Ebit"),
                        Ebitda = GetNullableDecimal(dr, "Ebitda"),
                        IndiceLiquidez = GetNullableDecimal(dr, "IndiceLiquidez"),
                        CapitalTrabajo = GetNullableDecimal(dr, "CapitalTrabajo"),
                        RatioEndeudamiento = GetNullableDecimal(dr, "RatioEndeudamiento"),
                        RatioRentabilidad = GetNullableDecimal(dr, "RatioRentabilidad")
                    });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeBalanceTurquiaCalculado>() };
            }
        }

        public async Task<Respuesta> CalcularBalanceTotalizadoAsync(UsuarioGeneral u, InformeBalanceTotalizadoCalcularRequest r)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Balance_Totalizado_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario",              SqlDbType.Int).Value        = u.IdUsuario;
                cmd.Parameters.Add("@vchUsuario",                SqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@intIdEmpresa",              SqlDbType.Int).Value        = u.IdEmpresa;
                cmd.Parameters.Add("@intIdRol",                  SqlDbType.Int).Value        = u.IdRol;
                cmd.Parameters.Add("@decTotalActivoCorriente",   SqlDbType.Decimal).Value    = Math.Round(r.TotalActivoCorriente,   2, MidpointRounding.AwayFromZero);
                cmd.Parameters.Add("@decTotalActivoNoCorriente", SqlDbType.Decimal).Value    = Math.Round(r.TotalActivoNoCorriente, 2, MidpointRounding.AwayFromZero);
                cmd.Parameters.Add("@decTotalPasivoCorriente",   SqlDbType.Decimal).Value    = Math.Round(r.TotalPasivoCorriente,   2, MidpointRounding.AwayFromZero);
                cmd.Parameters.Add("@decTotalPasivoNoCorriente", SqlDbType.Decimal).Value    = Math.Round(r.TotalPasivoNoCorriente, 2, MidpointRounding.AwayFromZero);
                cmd.Parameters.Add("@decTotalPatrimonio",        SqlDbType.Decimal).Value    = Math.Round(r.TotalPatrimonio,        2, MidpointRounding.AwayFromZero);
                cmd.Parameters.Add("@decIngresosOrdinarios",     SqlDbType.Decimal).Value    = Math.Round(r.IngresosOrdinarios,     2, MidpointRounding.AwayFromZero);
                cmd.Parameters.Add("@decGananciaNeta",           SqlDbType.Decimal).Value    = Math.Round(r.GananciaNeta,           2, MidpointRounding.AwayFromZero);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new List<InformeBalanceTotalizadoCalculado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    resultado.Add(new InformeBalanceTotalizadoCalculado
                    {
                        TotalActivo = Convert.ToDecimal(dr["TotalActivo"]),
                        TotalPasivos = Convert.ToDecimal(dr["TotalPasivos"]),
                        TotalPasivoPatrimonio = Convert.ToDecimal(dr["TotalPasivoPatrimonio"]),
                        IndiceLiquidez = GetNullableDecimal(dr, "IndiceLiquidez"),
                        CapitalTrabajo = Convert.ToDecimal(dr["CapitalTrabajo"]),
                        RatioEndeudamiento = GetNullableDecimal(dr, "RatioEndeudamiento"),
                        RatioRentabilidad = GetNullableDecimal(dr, "RatioRentabilidad")
                    });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeBalanceTotalizadoCalculado>() };
            }
        }

        public async Task<Respuesta> ObtenerOCrearInformeAsync(UsuarioGeneral u, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_ObtenerOCrear", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeIdResult>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new InformeIdResult { IdInforme = Convert.ToInt32(dr["IdInforme"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeIdResult>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral u, int idInforme)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Informe_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeEliminado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new InformeEliminado { IdInforme = Convert.ToInt32(dr["IdInforme"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeEliminado>() };
            }
        }


    }
}
