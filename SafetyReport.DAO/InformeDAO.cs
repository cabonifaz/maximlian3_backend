using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class InformeDAO
    {
        private readonly DbConfig _dbConfig;

        public InformeDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
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
                    (object?)x.TipoCambio ?? DBNull.Value,
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
                    (object?)x.Paises ?? DBNull.Value, (object?)x.Monto ?? DBNull.Value,
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
                    (object?)x.PromedioMensual ?? DBNull.Value, (object?)x.PlazoCredito ?? DBNull.Value,
                    (object?)x.Productos ?? DBNull.Value, (object?)x.IdCalificacion ?? DBNull.Value,
                    (object?)x.Comentarios ?? DBNull.Value,
                    (object?)x.NombreContacto ?? DBNull.Value,
                    (object?)x.Telefono ?? DBNull.Value,
                    (object?)x.ComienzoNegociaciones ?? DBNull.Value,
                    (object?)x.IdPlazoCredito ?? DBNull.Value,
                    (object?)x.EsTieneReferenciaComercial ?? DBNull.Value,
                    (object?)x.TipoCambio ?? DBNull.Value);
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
                    (object?)x.EfectivoEquivalente ?? DBNull.Value,
                    (object?)x.OtrosActivosFinancierosCorriente ?? DBNull.Value,
                    (object?)x.CuentasCobrarCorriente ?? DBNull.Value,
                    (object?)x.InventariosCorriente ?? DBNull.Value,
                    (object?)x.ActivosBiologicosCorriente ?? DBNull.Value,
                    (object?)x.ActivosImpuestosGanancias ?? DBNull.Value,
                    (object?)x.OtrosActivosNoFinancierosCorriente ?? DBNull.Value,
                    (object?)x.TotalActivoCorriente ?? DBNull.Value,
                    (object?)x.OtrosActivosFinancierosNoCorriente ?? DBNull.Value,
                    (object?)x.InversionesSubsidiarias ?? DBNull.Value,
                    (object?)x.CuentasCobrarNoCorriente ?? DBNull.Value,
                    (object?)x.InventariosNoCorriente ?? DBNull.Value,
                    (object?)x.ActivosBiologicosNoCorriente ?? DBNull.Value,
                    (object?)x.PropiedadesInversion ?? DBNull.Value,
                    (object?)x.PropiedadesPlantaEquipo ?? DBNull.Value,
                    (object?)x.Intangibles ?? DBNull.Value,
                    (object?)x.ActivosImpuestosDiferidos ?? DBNull.Value,
                    (object?)x.ActivosImpuestosCorrientes ?? DBNull.Value,
                    (object?)x.Plusvalia ?? DBNull.Value,
                    (object?)x.OtrosActivosNoFinancierosNoCorriente ?? DBNull.Value,
                    (object?)x.TotalActivoNoCorriente ?? DBNull.Value,
                    (object?)x.TotalActivo ?? DBNull.Value,
                    (object?)x.OtrosPasivosFinancierosCorriente ?? DBNull.Value,
                    (object?)x.CuentasPagarCorriente ?? DBNull.Value,
                    (object?)x.BeneficiosEmpleadosCorriente ?? DBNull.Value,
                    (object?)x.OtrasProvisionesCorriente ?? DBNull.Value,
                    (object?)x.ImpuestosGananciasCorriente ?? DBNull.Value,
                    (object?)x.OtrosPasivosNoFinancierosCorriente ?? DBNull.Value,
                    (object?)x.TotalPasivoCorriente ?? DBNull.Value,
                    (object?)x.OtrosPasivosFinancierosNoCorriente ?? DBNull.Value,
                    (object?)x.CuentasPagarNoCorriente ?? DBNull.Value,
                    (object?)x.BeneficiosEmpleadosNoCorriente ?? DBNull.Value,
                    (object?)x.OtrasProvisionesNoCorriente ?? DBNull.Value,
                    (object?)x.ImpuestosDiferidosNoCorriente ?? DBNull.Value,
                    (object?)x.ImpuestosCorrientesNoCorriente ?? DBNull.Value,
                    (object?)x.OtrosPasivosNoFinancierosNoCorriente ?? DBNull.Value,
                    (object?)x.TotalPasivoNoCorriente ?? DBNull.Value,
                    (object?)x.TotalPasivos ?? DBNull.Value,
                    (object?)x.CapitalEmitido ?? DBNull.Value,
                    (object?)x.PrimasEmision ?? DBNull.Value,
                    (object?)x.AccionesInversion ?? DBNull.Value,
                    (object?)x.AccionesCartera ?? DBNull.Value,
                    (object?)x.OtrasReservasCapital ?? DBNull.Value,
                    (object?)x.ResultadosAcumulados ?? DBNull.Value,
                    (object?)x.OtrasReservasPatrimonio ?? DBNull.Value,
                    (object?)x.TotalPatrimonio ?? DBNull.Value,
                    (object?)x.TotalPasivoPatrimonio ?? DBNull.Value,
                    (object?)x.IngresosOrdinarios ?? DBNull.Value,
                    (object?)x.CostoVentas ?? DBNull.Value,
                    (object?)x.GananciaBruta ?? DBNull.Value,
                    (object?)x.GastosVentas ?? DBNull.Value,
                    (object?)x.GastosAdministracion ?? DBNull.Value,
                    (object?)x.OtrosIngresosOperativos ?? DBNull.Value,
                    (object?)x.OtrosGastosOperativos ?? DBNull.Value,
                    (object?)x.OtrasGananciasPerdidas ?? DBNull.Value,
                    (object?)x.GananciaOperativa ?? DBNull.Value,
                    (object?)x.IngresosFinancieros ?? DBNull.Value,
                    (object?)x.IngresosIntereses ?? DBNull.Value,
                    (object?)x.GastosFinancieros ?? DBNull.Value,
                    (object?)x.DeterioroValor ?? DBNull.Value,
                    (object?)x.OtrosIngresosSubsidiarias ?? DBNull.Value,
                    (object?)x.DiferenciasCambio ?? DBNull.Value,
                    (object?)x.GananciaAntesImpuestos ?? DBNull.Value,
                    (object?)x.IngresoGastoImpuesto ?? DBNull.Value,
                    (object?)x.OperacionesDescontinuadas ?? DBNull.Value,
                    (object?)x.GananciaNeta ?? DBNull.Value,
                    (object?)x.IndiceLiquidez ?? DBNull.Value,
                    (object?)x.CapitalTrabajo ?? DBNull.Value,
                    (object?)x.RatioEndeudamiento ?? DBNull.Value,
                    (object?)x.RatioRentabilidad ?? DBNull.Value);
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
                    (object?)x.TotalActivoCorriente ?? DBNull.Value,
                    (object?)x.TotalActivoNoCorriente ?? DBNull.Value,
                    (object?)x.TotalActivo ?? DBNull.Value,
                    (object?)x.TotalPasivoCorriente ?? DBNull.Value,
                    (object?)x.TotalPasivoNoCorriente ?? DBNull.Value,
                    (object?)x.TotalPasivos ?? DBNull.Value,
                    (object?)x.TotalPatrimonio ?? DBNull.Value,
                    (object?)x.TotalPasivoPatrimonio ?? DBNull.Value,
                    (object?)x.IngresosOrdinarios ?? DBNull.Value,
                    (object?)x.GananciaNeta ?? DBNull.Value,
                    (object?)x.IndiceLiquidez ?? DBNull.Value,
                    (object?)x.CapitalTrabajo ?? DBNull.Value,
                    (object?)x.RatioEndeudamiento ?? DBNull.Value,
                    (object?)x.RatioRentabilidad ?? DBNull.Value);
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
                    (object?)x.Disponible ?? DBNull.Value,
                    (object?)x.FondosInterbancarios ?? DBNull.Value,
                    (object?)x.InversionesValorRazonable ?? DBNull.Value,
                    (object?)x.CarteraCreditos ?? DBNull.Value,
                    (object?)x.DerivadosNegociacionActivo ?? DBNull.Value,
                    (object?)x.DerivadosCoberturaActivo ?? DBNull.Value,
                    (object?)x.BienesRealizables ?? DBNull.Value,
                    (object?)x.ParticipacionesSubsidiarias ?? DBNull.Value,
                    (object?)x.InmuebleMobiliarioEquipo ?? DBNull.Value,
                    (object?)x.ImpuestoRentaDiferido ?? DBNull.Value,
                    (object?)x.OtrosActivos ?? DBNull.Value,
                    (object?)x.TotalActivos ?? DBNull.Value,
                    (object?)x.ObligacionesPublico ?? DBNull.Value,
                    (object?)x.FondosInterbancariosPasivo ?? DBNull.Value,
                    (object?)x.AdeudosFinancieras ?? DBNull.Value,
                    (object?)x.DerivadosNegociacionPasivo ?? DBNull.Value,
                    (object?)x.DerivadosCoberturaPasivo ?? DBNull.Value,
                    (object?)x.CuentasPagarProvisiones ?? DBNull.Value,
                    (object?)x.TotalPasivo ?? DBNull.Value,
                    (object?)x.CapitalSocial ?? DBNull.Value,
                    (object?)x.Reservas ?? DBNull.Value,
                    (object?)x.ResultadosNoRealizados ?? DBNull.Value,
                    (object?)x.ResultadoEjercicio ?? DBNull.Value,
                    (object?)x.TotalPatrimonio ?? DBNull.Value,
                    (object?)x.TotalPasivoPatrimonio ?? DBNull.Value,
                    (object?)x.IngresosIntereses ?? DBNull.Value,
                    (object?)x.UtilidadEjercicio ?? DBNull.Value);
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
                    (object?)x.EfectivoDisponible ?? DBNull.Value,
                    (object?)x.InversionesFinancieras ?? DBNull.Value,
                    (object?)x.PrestamosInteresesNetos ?? DBNull.Value,
                    (object?)x.PrimasCobrar ?? DBNull.Value,
                    (object?)x.DeudasReaseguradores ?? DBNull.Value,
                    (object?)x.ActivosVenta ?? DBNull.Value,
                    (object?)x.PropiedadesInversion ?? DBNull.Value,
                    (object?)x.PropiedadPlantaEquipo ?? DBNull.Value,
                    (object?)x.OtrosActivos ?? DBNull.Value,
                    (object?)x.TotalActivos ?? DBNull.Value,
                    (object?)x.ObligacionesAsegurados ?? DBNull.Value,
                    (object?)x.ReservasSiniestros ?? DBNull.Value,
                    (object?)x.ReservasTecnicas ?? DBNull.Value,
                    (object?)x.ObligacionesReaseguradores ?? DBNull.Value,
                    (object?)x.ObligacionesFinancieras ?? DBNull.Value,
                    (object?)x.CuentasPagar ?? DBNull.Value,
                    (object?)x.OtrosPasivos ?? DBNull.Value,
                    (object?)x.TotalPasivo ?? DBNull.Value,
                    (object?)x.CapitalSocial ?? DBNull.Value,
                    (object?)x.AportesCapitalNoCapitalizados ?? DBNull.Value,
                    (object?)x.ResultadosAcumulados ?? DBNull.Value,
                    (object?)x.PatrimonioRestringido ?? DBNull.Value,
                    (object?)x.TotalPatrimonio ?? DBNull.Value,
                    (object?)x.TotalPasivoPatrimonio ?? DBNull.Value,
                    (object?)x.PrimasGanadasNetas ?? DBNull.Value,
                    (object?)x.UtilidadNeta ?? DBNull.Value);
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
                    (object?)x.TipoCambio ?? DBNull.Value,
                    (object?)x.Efectivo ?? DBNull.Value,
                    (object?)x.Existencias ?? DBNull.Value,
                    (object?)x.Deudores ?? DBNull.Value,
                    (object?)x.TotalCorriente ?? DBNull.Value,
                    (object?)x.BienesTongibles ?? DBNull.Value,
                    (object?)x.ActivosIntangibles ?? DBNull.Value,
                    (object?)x.ActivoFijoNeto ?? DBNull.Value,
                    (object?)x.TotalActivos ?? DBNull.Value,
                    (object?)x.Prestamos ?? DBNull.Value,
                    (object?)x.Acreedores ?? DBNull.Value,
                    (object?)x.PasivosCorrientes ?? DBNull.Value,
                    (object?)x.PasivosNoCorrientes ?? DBNull.Value,
                    (object?)x.PasivosLargoPlazo ?? DBNull.Value,
                    (object?)x.TotalPasivosNoCorrientes ?? DBNull.Value,
                    (object?)x.TotalPasivos ?? DBNull.Value,
                    (object?)x.Capital ?? DBNull.Value,
                    (object?)x.Patrimonio ?? DBNull.Value,
                    (object?)x.TotalPatrimonio ?? DBNull.Value,
                    (object?)x.TotalPasivosPatrimonio ?? DBNull.Value,
                    (object?)x.VentasNetas ?? DBNull.Value,
                    (object?)x.CostoVentas ?? DBNull.Value,
                    (object?)x.CostoMateriales ?? DBNull.Value,
                    (object?)x.GananciaBruta ?? DBNull.Value,
                    (object?)x.OtrosGastosOperativos ?? DBNull.Value,
                    (object?)x.CostoEmpleados ?? DBNull.Value,
                    (object?)x.Depreciacion ?? DBNull.Value,
                    (object?)x.IngresosFinancieros ?? DBNull.Value,
                    (object?)x.GastosFinancieros ?? DBNull.Value,
                    (object?)x.InteresesPagados ?? DBNull.Value,
                    (object?)x.PlFinanciero ?? DBNull.Value,
                    (object?)x.IngresosExtraordinarios ?? DBNull.Value,
                    (object?)x.GastosExtraordinarios ?? DBNull.Value,
                    (object?)x.PlExtraordinario ?? DBNull.Value,
                    (object?)x.GananciaAntesImpuestos ?? DBNull.Value,
                    (object?)x.Impuestos ?? DBNull.Value,
                    (object?)x.GananciaNeta ?? DBNull.Value,
                    (object?)x.Ebit ?? DBNull.Value,
                    (object?)x.Ebitda ?? DBNull.Value,
                    (object?)x.Ganancia ?? DBNull.Value,
                    (object?)x.IndiceLiquidez ?? DBNull.Value,
                    (object?)x.CapitalTrabajo ?? DBNull.Value,
                    (object?)x.RatioEndeudamiento ?? DBNull.Value,
                    (object?)x.RatioRentabilidad ?? DBNull.Value);
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

        // ── Reader helper ─────────────────────────────────────────────────────────

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();
            using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 0;
                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                var json = dr["Result"]?.ToString();
                respuesta.Result = !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>()
                    : new List<T>();
            }
            else
            {
                respuesta.IdTipoMensaje = 1;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                respuesta.Result = new List<T>();
            }
            return respuesta;
        }

        private static async Task<(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)> LeerRespuestaConImagenesAsync<T>(SqlCommand cmd)
        {
            var imagenes = new List<InformeLocalImagenPendiente>();
            var respuesta = new Respuesta();

            using var dr = await cmd.ExecuteReaderAsync();
            do
            {
                var columnas = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (columnas.Contains("IdTipoMensaje"))
                {
                    if (await dr.ReadAsync())
                    {
                        respuesta.IdTipoMensaje = Convert.ToInt32(dr["IdTipoMensaje"]);
                        respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                        var json = dr["Result"]?.ToString();
                        respuesta.Result = !string.IsNullOrWhiteSpace(json)
                            ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>()
                            : new List<T>();
                    }
                }
                else if (columnas.Contains("ImagenURL"))
                {
                    while (await dr.ReadAsync())
                        imagenes.Add(new InformeLocalImagenPendiente
                        {
                            IdInformeLocalImagen = Convert.ToInt32(dr["IdInformeLocalImagen"]),
                            Nombre               = dr["Nombre"]?.ToString() ?? string.Empty,
                            S3Key                = dr["ImagenURL"]?.ToString() ?? string.Empty
                        });
                }
            }
            while (await dr.NextResultAsync());

            return (respuesta, imagenes);
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
            cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = (object?)r.IdTipoPersona ?? DBNull.Value;
            cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar, 255).Value = (object?)r.Nombre ?? DBNull.Value;
            cmd.Parameters.Add("@vchNombreComercial", SqlDbType.VarChar, 255).Value = (object?)r.NombreComercial ?? DBNull.Value;
            cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = (object?)r.IdPais ?? DBNull.Value;
            cmd.Parameters.Add("@intOperacionesTCMoneda", SqlDbType.Int).Value = (object?)r.OperacionesTCMoneda ?? DBNull.Value;
            cmd.Parameters.Add("@intTaxIdType", SqlDbType.Int).Value = (object?)r.TaxIdType ?? DBNull.Value;
            cmd.Parameters.Add("@vchTaxNum", SqlDbType.VarChar, 100).Value = (object?)r.TaxNum ?? DBNull.Value;
            cmd.Parameters.Add("@vchDireccion", SqlDbType.VarChar, 1024).Value = (object?)r.Direccion ?? DBNull.Value;
            cmd.Parameters.Add("@vchUbigeo", SqlDbType.VarChar, 150).Value = (object?)r.Ubigeo ?? DBNull.Value;
            cmd.Parameters.Add("@vchCodigoPostal", SqlDbType.VarChar, 50).Value = (object?)r.CodigoPostal ?? DBNull.Value;
            cmd.Parameters.Add("@vchTelefono", SqlDbType.VarChar, 2560).Value = (object?)r.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@vchFax", SqlDbType.VarChar, 2560).Value = (object?)r.Fax ?? DBNull.Value;
            cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 2560).Value = (object?)r.Email ?? DBNull.Value;
            cmd.Parameters.Add("@vchPaginaWeb", SqlDbType.VarChar, 512).Value = (object?)r.PaginaWeb ?? DBNull.Value;
            cmd.Parameters.Add("@intIdEstadoManual", SqlDbType.Int).Value = (object?)r.IdEstadoManual ?? DBNull.Value;
            cmd.Parameters.Add("@intIdEstadoInforme", SqlDbType.Int).Value = (object?)r.IdEstadoInforme ?? DBNull.Value;
            cmd.Parameters.Add("@vchDatosAdicionales", SqlDbType.VarChar, -1).Value = (object?)r.DatosAdicionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchObservacionesIdentificacion", SqlDbType.VarChar, -1).Value = (object?)r.ObservacionesIdentificacion ?? DBNull.Value;
            cmd.Parameters.Add("@intIdTipoEmpresa", SqlDbType.Int).Value = (object?)r.IdTipoEmpresa ?? DBNull.Value;
            cmd.Parameters.Add("@dtFechaConstitucion", SqlDbType.DateTime).Value = (object?)r.FechaConstitucion ?? DBNull.Value;
            cmd.Parameters.Add("@intIdCiudadRegistro", SqlDbType.Int).Value = (object?)r.IdCiudadRegistro ?? DBNull.Value;
            cmd.Parameters.Add("@vchIdNotaria", SqlDbType.VarChar, 255).Value = (object?)r.IdNotaria ?? DBNull.Value;
            cmd.Parameters.Add("@vchIdNotario", SqlDbType.VarChar, 255).Value = (object?)r.IdNotario ?? DBNull.Value;
            cmd.Parameters.Add("@vchIdRegistro", SqlDbType.VarChar, 255).Value = (object?)r.IdRegistro ?? DBNull.Value;
            cmd.Parameters.Add("@vchIdPlazo", SqlDbType.VarChar, 50).Value = (object?)r.IdPlazo ?? DBNull.Value;
            cmd.Parameters.Add("@intIdOperacionesCambioDivisas", SqlDbType.Int).Value = (object?)r.IdOperacionesCambioDivisas ?? DBNull.Value;
            cmd.Parameters.Add("@decCapitalInicial", SqlDbType.Decimal).Value = (object?)r.CapitalInicial ?? DBNull.Value;
            cmd.Parameters.Add("@decCapitalPagado", SqlDbType.Decimal).Value = (object?)r.CapitalPagado ?? DBNull.Value;
            cmd.Parameters.Add("@dtFechaUltimoIncremento", SqlDbType.DateTime).Value = (object?)r.FechaUltimoIncremento ?? DBNull.Value;
            cmd.Parameters.Add("@intIdTipoIncremento", SqlDbType.Int).Value = (object?)r.IdTipoIncremento ?? DBNull.Value;
            cmd.Parameters.Add("@decPatrimonioNeto", SqlDbType.Decimal).Value = (object?)r.PatrimonioNeto ?? DBNull.Value;
            cmd.Parameters.Add("@vchTipoAcciones", SqlDbType.VarChar, 255).Value = (object?)r.TipoAcciones ?? DBNull.Value;
            cmd.Parameters.Add("@decValorAcciones", SqlDbType.Decimal).Value = (object?)r.ValorAcciones ?? DBNull.Value;
            cmd.Parameters.Add("@bitCotizaBolsa", SqlDbType.Bit).Value = (object?)r.CotizaBolsa ?? DBNull.Value;
            cmd.Parameters.Add("@decTipoCambio", SqlDbType.Decimal).Value = (object?)r.TipoCambio ?? DBNull.Value;
            cmd.Parameters.Add("@intIdTipoCambio", SqlDbType.Int).Value = (object?)r.IdTipoCambio ?? DBNull.Value;
            cmd.Parameters.Add("@vchAntecedentes", SqlDbType.VarChar, -1).Value = (object?)r.Antecedentes ?? DBNull.Value;
            cmd.Parameters.Add("@vchAspectosLegales", SqlDbType.VarChar, -1).Value = (object?)r.AspectosLegales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComentariosAspectoLegal", SqlDbType.VarChar, -1).Value = (object?)r.ComentariosAspectoLegal ?? DBNull.Value;
            cmd.Parameters.Add("@intIdSector", SqlDbType.Int).Value = (object?)r.IdSector ?? DBNull.Value;
            cmd.Parameters.Add("@vchActividad", SqlDbType.VarChar, 255).Value = (object?)r.Actividad ?? DBNull.Value;
            cmd.Parameters.Add("@intIdIsicCategoria", SqlDbType.Int).Value = (object?)r.IdIsicCategoria ?? DBNull.Value;
            cmd.Parameters.Add("@intIdIsicClase", SqlDbType.Int).Value = (object?)r.IdIsicClase ?? DBNull.Value;
            cmd.Parameters.Add("@vchActividadPrincipal", SqlDbType.VarChar, -1).Value = (object?)r.ActividadPrincipal ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasContado", SqlDbType.Decimal).Value = (object?)r.VentasContado ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasContadoText", SqlDbType.VarChar, 50).Value = (object?)r.VentasContadoText ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasCredito", SqlDbType.Decimal).Value = (object?)r.VentasCredito ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasCreditoText", SqlDbType.VarChar, 50).Value = (object?)r.VentasCreditoText ?? DBNull.Value;
            cmd.Parameters.Add("@intIdVentasCreditoTiempo", SqlDbType.Int).Value = (object?)r.IdVentasCreditoTiempo ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasInternacionales", SqlDbType.Decimal).Value = (object?)r.VentasInternacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasInternacionalesText", SqlDbType.VarChar, 50).Value = (object?)r.VentasInternacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasNacionales", SqlDbType.Decimal).Value = (object?)r.VentasNacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasNacionalesText", SqlDbType.VarChar, 255).Value = (object?)r.VentasNacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@decComprasNacionales", SqlDbType.Decimal).Value = (object?)r.ComprasNacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComprasNacionalesText", SqlDbType.VarChar, 255).Value = (object?)r.ComprasNacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@decComprasInternacionales", SqlDbType.Decimal).Value = (object?)r.ComprasInternacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComprasInternacionalesText", SqlDbType.VarChar, 255).Value = (object?)r.ComprasInternacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@decComprasContadoNacionales", SqlDbType.Decimal).Value = (object?)r.ComprasContadoNacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComprasContadoNacionalesText", SqlDbType.VarChar, 50).Value = (object?)r.ComprasContadoNacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@decComprasCreditoNacionales", SqlDbType.Decimal).Value = (object?)r.ComprasCreditoNacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComprasCreditoNacionalesText", SqlDbType.VarChar, 50).Value = (object?)r.ComprasCreditoNacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@intIdComprasCreditoNacionalesTiempo", SqlDbType.Int).Value = (object?)r.IdComprasCreditoNacionalesTiempo ?? DBNull.Value;
            cmd.Parameters.Add("@decComprasContadoInternacionales", SqlDbType.Decimal).Value = (object?)r.ComprasContadoInternacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComprasContadoInternacionalesText", SqlDbType.VarChar, 50).Value = (object?)r.ComprasContadoInternacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@decComprasCreditoInternacionales", SqlDbType.Decimal).Value = (object?)r.ComprasCreditoInternacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComprasCreditoInternacionalesText", SqlDbType.VarChar, 50).Value = (object?)r.ComprasCreditoInternacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@intIdComprasCreditoInternacionalesTiempo", SqlDbType.Int).Value = (object?)r.IdComprasCreditoInternacionalesTiempo ?? DBNull.Value;
            cmd.Parameters.Add("@intNumeroEmpleados", SqlDbType.Int).Value = (object?)r.NumeroEmpleados ?? DBNull.Value;
            cmd.Parameters.Add("@vchNumeroEmpleadosText", SqlDbType.VarChar, 50).Value = (object?)r.NumeroEmpleadosText ?? DBNull.Value;
            cmd.Parameters.Add("@vchComentariosOperaciones", SqlDbType.VarChar, -1).Value = (object?)r.ComentariosOperaciones ?? DBNull.Value;
            cmd.Parameters.Add("@vchContenidoInformacionFinanciera", SqlDbType.VarChar, -1).Value = (object?)r.ContenidoInformacionFinanciera ?? DBNull.Value;
            cmd.Parameters.Add("@vchComentarioInformacionFinanciera", SqlDbType.VarChar, -1).Value = (object?)r.ComentarioInformacionFinanciera ?? DBNull.Value;
            cmd.Parameters.Add("@vchActivosFijos", SqlDbType.VarChar, -1).Value = (object?)r.ActivosFijos ?? DBNull.Value;
            cmd.Parameters.Add("@vchSeguros", SqlDbType.VarChar, -1).Value = (object?)r.Seguros ?? DBNull.Value;
            cmd.Parameters.Add("@vchComentarioProveedor", SqlDbType.VarChar, -1).Value = (object?)r.ComentarioProveedor ?? DBNull.Value;
            cmd.Parameters.Add("@vchReferenciaBanco", SqlDbType.VarChar, -1).Value = (object?)r.ReferenciaBanco ?? DBNull.Value;
            cmd.Parameters.Add("@vchLitigios", SqlDbType.VarChar, -1).Value = (object?)r.Litigios ?? DBNull.Value;
            cmd.Parameters.Add("@vchRiesgoPrincipal", SqlDbType.VarChar, -1).Value = (object?)r.RiesgoPrincipal ?? DBNull.Value;
            cmd.Parameters.Add("@vchSuperintendecia", SqlDbType.VarChar, -1).Value = (object?)r.Superintendecia ?? DBNull.Value;
            cmd.Parameters.Add("@vchInformacionGeneral", SqlDbType.VarChar, -1).Value = (object?)r.InformacionGeneral ?? DBNull.Value;
            cmd.Parameters.Add("@vchOpinionCredito", SqlDbType.VarChar, -1).Value = (object?)r.OpinionCredito ?? DBNull.Value;
            cmd.Parameters.Add("@bitFlgTieneInformacion", SqlDbType.Bit).Value = (object?)r.FlgTieneInformacion ?? DBNull.Value;
        }

        private static void AgregarTvpsCampos(SqlCommand cmd, InformeCrear r)
        {
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
                using SqlCommand cmd = new("Informe_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarParametrosCampos(cmd, request);
                AgregarTvpsCampos(cmd, request);
                await cn.OpenAsync();
                return await LeerRespuestaConImagenesAsync<InformeCreado>(cmd);
            }
            catch (Exception ex)
            {
                return (new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() }, new());
            }
        }

        public async Task<(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)> ActualizarAsync(UsuarioGeneral u, InformeEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Informe_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = request.IdInforme;
                AgregarParametrosCampos(cmd, request);
                AgregarTvpsCampos(cmd, request);
                await cn.OpenAsync();
                return await LeerRespuestaConImagenesAsync<InformeCreado>(cmd);
            }
            catch (Exception ex)
            {
                return (new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() }, new());
            }
        }

        public async Task<Respuesta> ActualizarEstadoCargaAsync(UsuarioGeneral u, List<int> ids)
        {
            try
            {
                var t = new DataTable();
                t.Columns.Add("IdInformeLocalImagen", typeof(int));
                foreach (var id in ids)
                    t.Rows.Add(id);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeLocalImagen_ActualizarEstadoCarga", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarTvp(cmd, "@lstIds", t, "LISTA_INFORME_LOCAL_IMAGEN_ID");
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ObtenerUrlsImagenesAsync(UsuarioGeneral u, List<int> ids)
        {
            try
            {
                var t = new DataTable();
                t.Columns.Add("IdInformeLocalImagen", typeof(int));
                foreach (var id in ids)
                    t.Rows.Add(id);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeLocalImagen_ObtenerUrls", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarTvp(cmd, "@lstIds", t, "LISTA_INFORME_LOCAL_IMAGEN_ID");
                await cn.OpenAsync();
                return await LeerRespuestaAsync<InformeLocalImagenUrl>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeLocalImagenUrl>() };
            }
        }

        public async Task<Respuesta> ObtenerArchivoAsync(UsuarioGeneral u, int idInformeArchivo)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeArchivo", SqlDbType.Int).Value = idInformeArchivo;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<InformeArchivoConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeArchivoConsulta>() };
            }
        }

        public async Task<Respuesta> EliminarArchivoAsync(UsuarioGeneral u, int idInformeArchivo)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeArchivo", SqlDbType.Int).Value = idInformeArchivo;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ActualizarArchivoAsync(UsuarioGeneral u, InformeArchivoActualizarRequest r)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeArchivo", SqlDbType.Int).Value = r.IdInformeArchivo;
                cmd.Parameters.Add("@intIdTipoArchivo",   SqlDbType.Int).Value = (object?)r.IdTipoArchivo   ?? DBNull.Value;
                cmd.Parameters.Add("@intIdFaseEvidencia", SqlDbType.Int).Value = (object?)r.IdFaseEvidencia ?? DBNull.Value;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> InsertarArchivoLoteAsync(UsuarioGeneral u, int idInforme, int idPedido, List<InformeArchivoItem> archivos)
        {
            try
            {
                var t = new DataTable();
                t.Columns.Add("Nombre", typeof(string));
                t.Columns.Add("ArchivoUrl", typeof(string));
                t.Columns.Add("Extension", typeof(string));
                t.Columns.Add("TamanoBytes", typeof(long));
                t.Columns.Add("IdTipoArchivo", typeof(int));
                t.Columns.Add("IdFaseEvidencia", typeof(int));
                foreach (var a in archivos)
                    t.Rows.Add(a.Nombre, a.ArchivoUrl, a.Extension, a.TamanoBytes, a.IdTipoArchivo, (object?)a.IdFaseEvidencia ?? DBNull.Value);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_InsertarLote", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdPedido",  SqlDbType.Int).Value = idPedido;
                AgregarTvp(cmd, "@lstArchivos", t, "LISTA_INFORME_ARCHIVO");
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral u, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Informe_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 0;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                    var json = dr["Result"]?.ToString();
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<List<InformeConsulta>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<InformeConsulta>()
                        : new List<InformeConsulta>();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new List<InformeConsulta>();
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral u, FiltroInforme filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Informe_Listar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = (object?)filtro.IdPedido ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = (object?)filtro.IdEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)filtro.NumPag ?? DBNull.Value;
                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 0;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                    var json = dr["Result"]?.ToString();
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<InformeListaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new InformeListaResult()
                        : new InformeListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new InformeListaResult();
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new InformeListaResult() };
            }
        }

        public async Task<Respuesta> CalcularBalanceTotalizadoAsync(UsuarioGeneral u, InformeBalanceTotalizadoCalcularRequest r)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Informe_Balance_Totalizado_Calcular", cn) { CommandType = CommandType.StoredProcedure };
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
                return await LeerRespuestaAsync<InformeBalanceTotalizadoCalculado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeBalanceTotalizadoCalculado>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral u, int idInforme)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Informe_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<InformeEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeEliminado>() };
            }
        }
    }
}
