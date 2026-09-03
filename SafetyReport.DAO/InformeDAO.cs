using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
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

        private static List<object> ConstruirBalancesJson(List<InformeBalanceItem> items)
        {
            int i = 1;
            return items.Select(x => (object)new
            {
                ID = i++,
                x.IdInformeBalance,
                x.FechaBalance,
                x.FechaHasta,
                x.FlgActualidad,
                TipoCambio = J4(x.TipoCambio),
                x.IdMoneda,
                x.IdTipoBalance,
                x.IdTipoEstadoFinanciero
            }).ToList();
        }

        private static List<object> ConstruirBancosJson(List<InformeBancoItem> items)
        {
            int i = 1;
            return items.Select(x => (object)new
            {
                ID = i++,
                x.IdInformeBanco,
                x.IdBanco,
                x.NumeroCuenta,
                x.IdSector,
                x.Sectorista,
                x.ReferenciaBanco
            }).ToList();
        }

        private static List<object> ConstruirCompaniasJson(List<InformeCompaniaRelacionadaItem> items)
        {
            int i = 1;
            return items.Select(x => (object)new
            {
                ID = i++,
                x.IdInformeCompaniaRelacionada,
                x.IdCompania
            }).ToList();
        }

        private static List<object> ConstruirExpImpJson(List<InformeExportacionImportacionItem> items)
        {
            int i = 1;
            return items.Select(x => (object)new
            {
                ID = i++,
                x.IdInformeExportacionImportacion,
                x.Anio,
                x.MesInicio,
                x.MesFin,
                x.IdMoneda,
                x.Paises,
                Monto = J2(x.Monto),
                x.Productos,
                x.IdTipoOperacion,
                x.NumOperaciones
            }).ToList();
        }

        private static List<object> ConstruirProveedoresJson(List<InformeProveedorItem> items)
        {
            int i = 1;
            return items.Select(x => (object)new
            {
                ID = i++,
                x.IdInformeProveedor,
                x.IdBancoProveedor,
                x.IdTipoPersona,
                x.Nombre,
                x.IdPais,
                x.IdTipoDocumento,
                x.NumeroDocumento,
                x.IdMoneda,
                x.FechaInicio,
                x.IdLimiteCredito,
                PromedioMensual = J4(x.PromedioMensual),
                x.PlazoCredito,
                x.Productos,
                x.IdCalificacion,
                x.Comentarios,
                x.NombreContacto,
                x.Telefono,
                x.ComienzoNegociaciones,
                x.IdPlazoCredito,
                x.EsTieneReferenciaComercial,
                TipoCambio = J6(x.TipoCambio)
            }).ToList();
        }

        private static List<object> ConstruirBalancesDesagregadoJson(List<InformeBalanceDesagregadoItem> items) =>
            items.Select(x => (object)new
            {
                x.Id,
                EfectivoEquivalente = J2(x.EfectivoEquivalente),
                OtrosActivosFinancierosCorriente = J2(x.OtrosActivosFinancierosCorriente),
                CuentasCobrarCorriente = J2(x.CuentasCobrarCorriente),
                InventariosCorriente = J2(x.InventariosCorriente),
                ActivosBiologicosCorriente = J2(x.ActivosBiologicosCorriente),
                ActivosImpuestosGanancias = J2(x.ActivosImpuestosGanancias),
                OtrosActivosNoFinancierosCorriente = J2(x.OtrosActivosNoFinancierosCorriente),
                TotalActivoCorriente = J2(x.TotalActivoCorriente),
                OtrosActivosFinancierosNoCorriente = J2(x.OtrosActivosFinancierosNoCorriente),
                InversionesSubsidiarias = J2(x.InversionesSubsidiarias),
                CuentasCobrarNoCorriente = J2(x.CuentasCobrarNoCorriente),
                InventariosNoCorriente = J2(x.InventariosNoCorriente),
                ActivosBiologicosNoCorriente = J2(x.ActivosBiologicosNoCorriente),
                PropiedadesInversion = J2(x.PropiedadesInversion),
                PropiedadesPlantaEquipo = J2(x.PropiedadesPlantaEquipo),
                Intangibles = J2(x.Intangibles),
                ActivosImpuestosDiferidos = J2(x.ActivosImpuestosDiferidos),
                ActivosImpuestosCorrientes = J2(x.ActivosImpuestosCorrientes),
                Plusvalia = J2(x.Plusvalia),
                OtrosActivosNoFinancierosNoCorriente = J2(x.OtrosActivosNoFinancierosNoCorriente),
                TotalActivoNoCorriente = J2(x.TotalActivoNoCorriente),
                TotalActivo = J2(x.TotalActivo),
                OtrosPasivosFinancierosCorriente = J2(x.OtrosPasivosFinancierosCorriente),
                CuentasPagarCorriente = J2(x.CuentasPagarCorriente),
                BeneficiosEmpleadosCorriente = J2(x.BeneficiosEmpleadosCorriente),
                OtrasProvisionesCorriente = J2(x.OtrasProvisionesCorriente),
                ImpuestosGananciasCorriente = J2(x.ImpuestosGananciasCorriente),
                OtrosPasivosNoFinancierosCorriente = J2(x.OtrosPasivosNoFinancierosCorriente),
                TotalPasivoCorriente = J2(x.TotalPasivoCorriente),
                OtrosPasivosFinancierosNoCorriente = J2(x.OtrosPasivosFinancierosNoCorriente),
                CuentasPagarNoCorriente = J2(x.CuentasPagarNoCorriente),
                BeneficiosEmpleadosNoCorriente = J2(x.BeneficiosEmpleadosNoCorriente),
                OtrasProvisionesNoCorriente = J2(x.OtrasProvisionesNoCorriente),
                ImpuestosDiferidosNoCorriente = J2(x.ImpuestosDiferidosNoCorriente),
                ImpuestosCorrientesNoCorriente = J2(x.ImpuestosCorrientesNoCorriente),
                OtrosPasivosNoFinancierosNoCorriente = J2(x.OtrosPasivosNoFinancierosNoCorriente),
                TotalPasivoNoCorriente = J2(x.TotalPasivoNoCorriente),
                TotalPasivos = J2(x.TotalPasivos),
                CapitalEmitido = J2(x.CapitalEmitido),
                PrimasEmision = J2(x.PrimasEmision),
                AccionesInversion = J2(x.AccionesInversion),
                AccionesCartera = J2(x.AccionesCartera),
                OtrasReservasCapital = J2(x.OtrasReservasCapital),
                ResultadosAcumulados = J2(x.ResultadosAcumulados),
                OtrasReservasPatrimonio = J2(x.OtrasReservasPatrimonio),
                TotalPatrimonio = J2(x.TotalPatrimonio),
                TotalPasivoPatrimonio = J2(x.TotalPasivoPatrimonio),
                IngresosOrdinarios = J2(x.IngresosOrdinarios),
                CostoVentas = J2(x.CostoVentas),
                GananciaBruta = J2(x.GananciaBruta),
                GastosVentas = J2(x.GastosVentas),
                GastosAdministracion = J2(x.GastosAdministracion),
                OtrosIngresosOperativos = J2(x.OtrosIngresosOperativos),
                OtrosGastosOperativos = J2(x.OtrosGastosOperativos),
                OtrasGananciasPerdidas = J2(x.OtrasGananciasPerdidas),
                GananciaOperativa = J2(x.GananciaOperativa),
                IngresosFinancieros = J2(x.IngresosFinancieros),
                IngresosIntereses = J2(x.IngresosIntereses),
                GastosFinancieros = J2(x.GastosFinancieros),
                DeterioroValor = J2(x.DeterioroValor),
                OtrosIngresosSubsidiarias = J2(x.OtrosIngresosSubsidiarias),
                DiferenciasCambio = J2(x.DiferenciasCambio),
                GananciaAntesImpuestos = J2(x.GananciaAntesImpuestos),
                IngresoGastoImpuesto = J2(x.IngresoGastoImpuesto),
                OperacionesDescontinuadas = J2(x.OperacionesDescontinuadas),
                GananciaNeta = J2(x.GananciaNeta),
                IndiceLiquidez = J2(x.IndiceLiquidez),
                CapitalTrabajo = J2(x.CapitalTrabajo),
                RatioEndeudamiento = J2(x.RatioEndeudamiento),
                RatioRentabilidad = J2(x.RatioRentabilidad)
            }).ToList();

        private static List<object> ConstruirBalancesTotalizadoJson(List<InformeBalanceTotalizadoItem> items) =>
            items.Select(x => (object)new
            {
                x.Id,
                TotalActivoCorriente = J2(x.TotalActivoCorriente),
                TotalActivoNoCorriente = J2(x.TotalActivoNoCorriente),
                TotalActivo = J2(x.TotalActivo),
                TotalPasivoCorriente = J2(x.TotalPasivoCorriente),
                TotalPasivoNoCorriente = J2(x.TotalPasivoNoCorriente),
                TotalPasivos = J2(x.TotalPasivos),
                TotalPatrimonio = J2(x.TotalPatrimonio),
                TotalPasivoPatrimonio = J2(x.TotalPasivoPatrimonio),
                IngresosOrdinarios = J2(x.IngresosOrdinarios),
                GananciaNeta = J2(x.GananciaNeta),
                IndiceLiquidez = J2(x.IndiceLiquidez),
                CapitalTrabajo = J2(x.CapitalTrabajo),
                RatioEndeudamiento = J2(x.RatioEndeudamiento),
                RatioRentabilidad = J2(x.RatioRentabilidad)
            }).ToList();

        private static List<object> ConstruirBalancesBancoJson(List<InformeBalanceBancoItem> items) =>
            items.Select(x => (object)new
            {
                x.Id,
                Disponible = J2(x.Disponible),
                FondosInterbancarios = J2(x.FondosInterbancarios),
                InversionesValorRazonable = J2(x.InversionesValorRazonable),
                CarteraCreditos = J2(x.CarteraCreditos),
                DerivadosNegociacionActivo = J2(x.DerivadosNegociacionActivo),
                DerivadosCoberturaActivo = J2(x.DerivadosCoberturaActivo),
                BienesRealizables = J2(x.BienesRealizables),
                ParticipacionesSubsidiarias = J2(x.ParticipacionesSubsidiarias),
                InmuebleMobiliarioEquipo = J2(x.InmuebleMobiliarioEquipo),
                ImpuestoRentaDiferido = J2(x.ImpuestoRentaDiferido),
                OtrosActivos = J2(x.OtrosActivos),
                TotalActivos = J2(x.TotalActivos),
                ObligacionesPublico = J2(x.ObligacionesPublico),
                FondosInterbancariosPasivo = J2(x.FondosInterbancariosPasivo),
                AdeudosFinancieras = J2(x.AdeudosFinancieras),
                DerivadosNegociacionPasivo = J2(x.DerivadosNegociacionPasivo),
                DerivadosCoberturaPasivo = J2(x.DerivadosCoberturaPasivo),
                CuentasPagarProvisiones = J2(x.CuentasPagarProvisiones),
                TotalPasivo = J2(x.TotalPasivo),
                CapitalSocial = J2(x.CapitalSocial),
                Reservas = J2(x.Reservas),
                ResultadosNoRealizados = J2(x.ResultadosNoRealizados),
                ResultadoEjercicio = J2(x.ResultadoEjercicio),
                TotalPatrimonio = J2(x.TotalPatrimonio),
                TotalPasivoPatrimonio = J2(x.TotalPasivoPatrimonio),
                IngresosIntereses = J2(x.IngresosIntereses),
                UtilidadEjercicio = J2(x.UtilidadEjercicio)
            }).ToList();

        private static List<object> ConstruirBalancesSeguroJson(List<InformeBalanceSeguroItem> items) =>
            items.Select(x => (object)new
            {
                x.Id,
                EfectivoDisponible = J2(x.EfectivoDisponible),
                InversionesFinancieras = J2(x.InversionesFinancieras),
                PrestamosInteresesNetos = J2(x.PrestamosInteresesNetos),
                PrimasCobrar = J2(x.PrimasCobrar),
                DeudasReaseguradores = J2(x.DeudasReaseguradores),
                ActivosVenta = J2(x.ActivosVenta),
                PropiedadesInversion = J2(x.PropiedadesInversion),
                PropiedadPlantaEquipo = J2(x.PropiedadPlantaEquipo),
                OtrosActivos = J2(x.OtrosActivos),
                TotalActivos = J2(x.TotalActivos),
                ObligacionesAsegurados = J2(x.ObligacionesAsegurados),
                ReservasSiniestros = J2(x.ReservasSiniestros),
                ReservasTecnicas = J2(x.ReservasTecnicas),
                ObligacionesReaseguradores = J2(x.ObligacionesReaseguradores),
                ObligacionesFinancieras = J2(x.ObligacionesFinancieras),
                CuentasPagar = J2(x.CuentasPagar),
                OtrosPasivos = J2(x.OtrosPasivos),
                TotalPasivo = J2(x.TotalPasivo),
                CapitalSocial = J2(x.CapitalSocial),
                AportesCapitalNoCapitalizados = J2(x.AportesCapitalNoCapitalizados),
                ResultadosAcumulados = J2(x.ResultadosAcumulados),
                PatrimonioRestringido = J2(x.PatrimonioRestringido),
                TotalPatrimonio = J2(x.TotalPatrimonio),
                TotalPasivoPatrimonio = J2(x.TotalPasivoPatrimonio),
                PrimasGanadasNetas = J2(x.PrimasGanadasNetas),
                UtilidadNeta = J2(x.UtilidadNeta)
            }).ToList();

        private static List<object> ConstruirBalancesTurquiaJson(List<InformeBalanceTurquiaItem> items) =>
            items.Select(x => (object)new
            {
                x.Id,
                x.Ano,
                x.FechaBalance,
                x.IdMoneda,
                x.DuracionPeriodo,
                x.IdNivelConfiabilidad,
                TipoCambio = J6(x.TipoCambio),
                Efectivo = J2(x.Efectivo),
                Existencias = J2(x.Existencias),
                Deudores = J2(x.Deudores),
                TotalCorriente = J2(x.TotalCorriente),
                BienesTongibles = J2(x.BienesTongibles),
                ActivosIntangibles = J2(x.ActivosIntangibles),
                ActivoFijoNeto = J2(x.ActivoFijoNeto),
                TotalActivos = J2(x.TotalActivos),
                Prestamos = J2(x.Prestamos),
                Acreedores = J2(x.Acreedores),
                PasivosCorrientes = J2(x.PasivosCorrientes),
                PasivosNoCorrientes = J2(x.PasivosNoCorrientes),
                PasivosLargoPlazo = J2(x.PasivosLargoPlazo),
                TotalPasivosNoCorrientes = J2(x.TotalPasivosNoCorrientes),
                TotalPasivos = J2(x.TotalPasivos),
                Capital = J2(x.Capital),
                Reservas = J2(x.Reservas),
                ResultadosAcumulados = J2(x.ResultadosAcumulados),
                ResultadoEjercicio = J2(x.ResultadoEjercicio),
                OtrasCuentas = J2(x.OtrasCuentas),
                Patrimonio = J2(x.Patrimonio),
                TotalPatrimonio = J2(x.TotalPatrimonio),
                TotalPasivosPatrimonio = J2(x.TotalPasivosPatrimonio),
                VentasNetas = J2(x.VentasNetas),
                CostoVentas = J2(x.CostoVentas),
                CostoMateriales = J2(x.CostoMateriales),
                GananciaBruta = J2(x.GananciaBruta),
                OtrosGastosOperativos = J2(x.OtrosGastosOperativos),
                CostoEmpleados = J2(x.CostoEmpleados),
                Depreciacion = J2(x.Depreciacion),
                IngresosFinancieros = J2(x.IngresosFinancieros),
                GastosFinancieros = J2(x.GastosFinancieros),
                InteresesPagados = J2(x.InteresesPagados),
                PlFinanciero = J2(x.PlFinanciero),
                IngresosExtraordinarios = J2(x.IngresosExtraordinarios),
                GastosExtraordinarios = J2(x.GastosExtraordinarios),
                PlExtraordinario = J2(x.PlExtraordinario),
                GananciaAntesImpuestos = J2(x.GananciaAntesImpuestos),
                Impuestos = J2(x.Impuestos),
                GananciaNeta = J2(x.GananciaNeta),
                Ebit = J2(x.Ebit),
                Ebitda = J2(x.Ebitda),
                Ganancia = J2(x.Ganancia),
                IndiceLiquidez = J2(x.IndiceLiquidez),
                CapitalTrabajo = J2(x.CapitalTrabajo),
                RatioEndeudamiento = J2(x.RatioEndeudamiento),
                RatioRentabilidad = J2(x.RatioRentabilidad)
            }).ToList();

        private static List<object> ConstruirDirectoriosEjecutivosJson(List<InformeDirectorioEjecutivoItem> items)
        {
            int i = 1;
            return items.Select(x => (object)new
            {
                ID = i++,
                x.IdInformeDirectorioEjecutivo,
                x.IdDirectorioEjecutivo,
                x.IdCargo,
                x.VinculadoDesde,
                x.CompaniaAnterior,
                x.Participacion,
                x.Orden,
                x.EsParticipanteDirectiva,
                x.ApareceImpresoLista,
                x.ImprimeDatosEjecutivos
            }).ToList();
        }

        private static List<object> ConstruirLocalesJson(List<InformeLocalItem> items)
        {
            int i = 1;
            return items.Select(x => (object)new
            {
                ID = i++,
                x.IdInformeLocal,
                x.IdTipoLocal,
                x.Comentario
            }).ToList();
        }

        private static List<object> ConstruirLocalImagenesJson(List<InformeLocalItem> items)
        {
            var resultado = new List<object>();
            int img = 1;
            int localIdx = 1;
            foreach (var local in items)
            {
                foreach (var imagen in local.Imagenes)
                {
                    resultado.Add(new
                    {
                        ID = img++,
                        imagen.IdInformeLocalImagen,
                        IdLocal = localIdx,
                        imagen.ImagenURL,
                        imagen.IdTipoArchivo,
                        imagen.Nombre
                    });
                }
                localIdx++;
            }
            return resultado;
        }

        private static object ConstruirIdentificacionJson(InformeCrear r) => new
        {
            r.IdTipoPersona,
            r.Nombre,
            r.NombreComercial,
            r.IdPais,
            r.OperacionesTCMoneda,
            r.TaxIdType,
            r.TaxNum,
            r.Direccion,
            r.Ubigeo,
            r.CodigoPostal,
            r.Telefono,
            r.Fax,
            r.Email,
            r.PaginaWeb,
            r.IdEstadoManual,
            r.DatosAdicionales,
            r.ObservacionesIdentificacion
        };

        private static object ConstruirAspectosLegalesJson(InformeCrear r) => new
        {
            r.IdTipoEmpresa,
            r.FechaConstitucion,
            r.IdCiudadRegistro,
            r.IdNotaria,
            r.IdNotario,
            r.IdRegistro,
            r.IdPlazo,
            r.IdOperacionesCambioDivisas,
            CapitalInicial = J2(r.CapitalInicial),
            CapitalPagado = J2(r.CapitalPagado),
            r.FechaUltimoIncremento,
            r.IdTipoIncremento,
            PatrimonioNeto = J2(r.PatrimonioNeto),
            r.TipoAcciones,
            ValorAcciones = J2(r.ValorAcciones),
            r.CotizaBolsa,
            TipoCambio = J6(r.TipoCambio),
            r.IdTipoCambio,
            r.Antecedentes,
            r.AspectosLegales,
            r.ComentariosAspectoLegal
        };

        private static object ConstruirRamoOperacionesJson(InformeCrear r) => new
        {
            r.IdSector,
            r.Actividad,
            r.IdIsicCategoria,
            r.IdIsicClase,
            r.ActividadPrincipal,
            VentasContado = J2(r.VentasContado),
            r.VentasContadoText,
            VentasCredito = J2(r.VentasCredito),
            r.VentasCreditoText,
            r.IdVentasCreditoTiempo,
            VentasInternacionales = J2(r.VentasInternacionales),
            r.VentasInternacionalesText,
            VentasNacionales = J2(r.VentasNacionales),
            r.VentasNacionalesText,
            ComprasNacionales = J2(r.ComprasNacionales),
            r.ComprasNacionalesText,
            ComprasInternacionales = J2(r.ComprasInternacionales),
            r.ComprasInternacionalesText,
            ComprasContadoNacionales = J2(r.ComprasContadoNacionales),
            r.ComprasContadoNacionalesText,
            ComprasCreditoNacionales = J2(r.ComprasCreditoNacionales),
            r.ComprasCreditoNacionalesText,
            r.IdComprasCreditoNacionalesTiempo,
            ComprasContadoInternacionales = J2(r.ComprasContadoInternacionales),
            r.ComprasContadoInternacionalesText,
            ComprasCreditoInternacionales = J2(r.ComprasCreditoInternacionales),
            r.ComprasCreditoInternacionalesText,
            r.IdComprasCreditoInternacionalesTiempo,
            r.NumeroEmpleados,
            r.NumeroEmpleadosText,
            r.ComentariosOperaciones
        };

        private static object ConstruirInformacionFinancieraJson(InformeCrear r) => new
        {
            r.ContenidoInformacionFinanciera,
            r.ComentarioInformacionFinanciera,
            r.ActivosFijos,
            r.Seguros
        };

        private static object ConstruirBancosProveedoresJson(InformeCrear r) => new
        {
            r.ComentarioProveedor,
            r.ReferenciaBanco,
            r.Litigios,
            r.RiesgoPrincipal,
            r.Superintendecia
        };

        private static object ConstruirDatosGeneralesJson(InformeCrear r) => new
        {
            r.InformacionGeneral,
            r.OpinionCredito
        };

        // ── Decimal rounding helpers (match SQL TVP column precision) ────────────

        private static object D2(decimal? v) => v.HasValue ? (object)Math.Round(v.Value, 2, MidpointRounding.AwayFromZero) : DBNull.Value;
        private static object D4(decimal? v) => v.HasValue ? (object)Math.Round(v.Value, 4, MidpointRounding.AwayFromZero) : DBNull.Value;
        private static object D6(decimal? v) => v.HasValue ? (object)Math.Round(v.Value, 6, MidpointRounding.AwayFromZero) : DBNull.Value;

        // Mismo redondeo que D2/D4/D6 pero devolviendo decimal? (para serializar a JSON en vez de DataTable).
        private static decimal? J2(decimal? v) => v.HasValue ? Math.Round(v.Value, 2, MidpointRounding.AwayFromZero) : null;
        private static decimal? J4(decimal? v) => v.HasValue ? Math.Round(v.Value, 4, MidpointRounding.AwayFromZero) : null;
        private static decimal? J6(decimal? v) => v.HasValue ? Math.Round(v.Value, 6, MidpointRounding.AwayFromZero) : null;

        // ── Reader helper ─────────────────────────────────────────────────────────

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje. Sin columna Result.
        private async Task<Respuesta> LeerCabeceraAsync(DbDataReader dr, string procedimiento)
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

        private static int? GetNullableInt(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static bool? GetNullableBool(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToBoolean(dr[columna]);

        private static DateTime? GetNullableDateTime(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDateTime(dr[columna]);

        private static string? GetNullableString(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        // Lee el result set de imagenes pendientes (IdInformeLocalImagen, ImagenURL, Nombre)
        // que Informe_Insertar/Informe_Actualizar emiten como su ultimo result set en exito.
        private static async Task<List<InformeLocalImagenPendiente>> LeerImagenesPendientesAsync(DbDataReader dr)
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

        private static void AgregarParametrosCampos(MySqlCommand cmd, InformeCrear r)
        {
            cmd.Parameters.AddWithValue("@intIdPedido", (object?)r.IdPedido ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bitFlgTieneInformacion", (object?)r.FlgTieneInformacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@intIdEstadoInforme", (object?)r.IdEstadoInforme ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@intIdFormatoFecha", (object?)r.IdFormatoFecha ?? DBNull.Value);
        }

        private static void AgregarTvpsCampos(MySqlCommand cmd, InformeCrear r)
        {
            cmd.Parameters.AddWithValue("@tvpIdentificacion", JsonSerializer.Serialize(ConstruirIdentificacionJson(r)));
            cmd.Parameters.AddWithValue("@tvpAspectosLegales", JsonSerializer.Serialize(ConstruirAspectosLegalesJson(r)));
            cmd.Parameters.AddWithValue("@tvpRamoOperaciones", JsonSerializer.Serialize(ConstruirRamoOperacionesJson(r)));
            cmd.Parameters.AddWithValue("@tvpInformacionFinanciera", JsonSerializer.Serialize(ConstruirInformacionFinancieraJson(r)));
            cmd.Parameters.AddWithValue("@tvpBancosProveedores", JsonSerializer.Serialize(ConstruirBancosProveedoresJson(r)));
            cmd.Parameters.AddWithValue("@tvpDatosGenerales", JsonSerializer.Serialize(ConstruirDatosGeneralesJson(r)));

            cmd.Parameters.AddWithValue("@lstBalances", JsonSerializer.Serialize(ConstruirBalancesJson(r.lstBalances)));
            cmd.Parameters.AddWithValue("@lstBalancesDesagregado", JsonSerializer.Serialize(ConstruirBalancesDesagregadoJson(r.lstBalancesDesagregado)));
            cmd.Parameters.AddWithValue("@lstBalancesTotalizado", JsonSerializer.Serialize(ConstruirBalancesTotalizadoJson(r.lstBalancesTotalizado)));
            cmd.Parameters.AddWithValue("@lstBalancesBanco", JsonSerializer.Serialize(ConstruirBalancesBancoJson(r.lstBalancesBanco)));
            cmd.Parameters.AddWithValue("@lstBalancesSeguro", JsonSerializer.Serialize(ConstruirBalancesSeguroJson(r.lstBalancesSeguro)));
            cmd.Parameters.AddWithValue("@lstBalancesTurquia", JsonSerializer.Serialize(ConstruirBalancesTurquiaJson(r.lstBalancesTurquia)));
            cmd.Parameters.AddWithValue("@lstBancos", JsonSerializer.Serialize(ConstruirBancosJson(r.lstBancos)));
            cmd.Parameters.AddWithValue("@lstCompanias", JsonSerializer.Serialize(ConstruirCompaniasJson(r.lstCompaniasRelacionadas)));
            cmd.Parameters.AddWithValue("@lstExpImp", JsonSerializer.Serialize(ConstruirExpImpJson(r.lstExportacionesImportaciones)));
            cmd.Parameters.AddWithValue("@lstProveedores", JsonSerializer.Serialize(ConstruirProveedoresJson(r.lstProveedores)));
            cmd.Parameters.AddWithValue("@lstDirectoriosEjecutivos", JsonSerializer.Serialize(ConstruirDirectoriosEjecutivosJson(r.lstDirectoriosEjecutivos)));
            cmd.Parameters.AddWithValue("@lstLocales", JsonSerializer.Serialize(ConstruirLocalesJson(r.lstLocales)));
            cmd.Parameters.AddWithValue("@lstLocalImagenes", JsonSerializer.Serialize(ConstruirLocalImagenesJson(r.lstLocales)));
        }

        // ── CRUD ─────────────────────────────────────────────────────────────────

        public async Task<(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)> InsertarAsync(UsuarioGeneral u, InformeCrear request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", request.IdInforme);
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
        private static async Task<Dictionary<int, JsonElement>> LeerDetalleBalanceAsync(DbDataReader dr)
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
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
                                    IdFaseEvidencia = GetNullableInt(dr, "IdFaseEvidencia")
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

        public async Task<(Respuesta respuesta, string? nombreInforme, bool requiereTraduccion, int cantidadEnvios, string formatosCliente)> GenerarDocumentoAsync(UsuarioGeneral u, int idInforme, int idPedido)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_GenerarDocumento", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
                await cn.OpenAsync();

                var respuesta = new Respuesta();
                string? nombreInforme = null;
                var requiereTraduccion = false;
                var cantidadEnvios = 0;
                var formatosCliente = string.Empty;
                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 3;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                    respuesta.Result = dr["Result"]?.ToString();
                    nombreInforme = dr["NombreInforme"]?.ToString();
                    requiereTraduccion = dr["RequiereTraduccion"] != DBNull.Value && Convert.ToBoolean(dr["RequiereTraduccion"]);
                    cantidadEnvios = dr["CantidadEnvios"] != DBNull.Value ? Convert.ToInt32(dr["CantidadEnvios"]) : 0;
                    formatosCliente = dr["FormatosCliente"]?.ToString() ?? string.Empty;
                }
                else
                {
                    _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", cmd.CommandText);

                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                }
                return (respuesta, nombreInforme, requiereTraduccion, cantidadEnvios, formatosCliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return (new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message }, null, false, 0, string.Empty);
            }
        }

        public async Task<(Respuesta respuesta, string? nombreInforme)> GenerarDocumentoXmlAsync(UsuarioGeneral u, int idInforme, int idPedido)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_GenerarDocumentoXml", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_ObtenerDocumento", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_ActualizarEstado", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdEstadoInforme", idEstadoInforme);
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

        public async Task<Respuesta> ObtenerDatosNotificacionInformeAsync(UsuarioGeneral u, int idInforme)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_ObtenerDatosNotificacionInforme", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdEstadoInforme", 4);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<NotificacionInformeDatosConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new NotificacionInformeDatosConsulta
                        {
                            Correo = GetNullableString(dr, "Correo"),
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            CodigoPedido = dr["CodigoPedido"]?.ToString() ?? string.Empty,
                            Asunto = dr["Asunto"]?.ToString() ?? string.Empty,
                            CuerpoHtml = dr["CuerpoHtml"]?.ToString() ?? string.Empty
                        });
                    }
                }

                var datos = lista.FirstOrDefault();
                if (datos != null && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        datos.Formatos.Add(dr["Formato"]?.ToString() ?? string.Empty);
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<NotificacionInformeDatosConsulta>() };
            }
        }

        public async Task<Respuesta> RegistrarEnvioInformeAsync(UsuarioGeneral u, int idInforme, int idPedido)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeEnvio_Registrar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_ObtenerRutaDocumento", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_ActualizarDocumento", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@vchUrlDocumento", urlDocumento);
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)filtro.Busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPedido", (object?)filtro.IdPedido ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchIdEstado", (object?)filtro.IdEstado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchIdPlantilla", (object?)filtro.IdPlantilla ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchIdTipoTramite", (object?)filtro.IdTipoTramite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)filtro.NumPag ?? DBNull.Value);
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
                                CodigoPedido = GetNullableString(dr, "CodigoPedido"),
                                Cliente = GetNullableString(dr, "Cliente"),
                                IdFase = GetNullableInt(dr, "IdFase"),
                                Plantilla = GetNullableString(dr, "Plantilla"),
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_ListarIdPorCompania", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdCompania", filtro.IdCompania);
                cmd.Parameters.AddWithValue("@dtmFchInicio", (object?)filtro.FchInicio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtmFchFin", (object?)filtro.FchFin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)filtro.NumPag ?? DBNull.Value);
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
                                Fecha = GetNullableString(dr, "Fecha")
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Balance_Desagregado_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@decEfectivoEquivalente", D2(r.EfectivoEquivalente));
                cmd.Parameters.AddWithValue("@decOtrosActivosFinancierosCorriente", D2(r.OtrosActivosFinancierosCorriente));
                cmd.Parameters.AddWithValue("@decCuentasCobrarCorriente", D2(r.CuentasCobrarCorriente));
                cmd.Parameters.AddWithValue("@decInventariosCorriente", D2(r.InventariosCorriente));
                cmd.Parameters.AddWithValue("@decActivosBiologicosCorriente", D2(r.ActivosBiologicosCorriente));
                cmd.Parameters.AddWithValue("@decActivosImpuestosGanancias", D2(r.ActivosImpuestosGanancias));
                cmd.Parameters.AddWithValue("@decOtrosActivosNoFinancierosCorriente", D2(r.OtrosActivosNoFinancierosCorriente));
                cmd.Parameters.AddWithValue("@decOtrosActivosFinancierosNoCorriente", D2(r.OtrosActivosFinancierosNoCorriente));
                cmd.Parameters.AddWithValue("@decInversionesSubsidiarias", D2(r.InversionesSubsidiarias));
                cmd.Parameters.AddWithValue("@decCuentasCobrarNoCorriente", D2(r.CuentasCobrarNoCorriente));
                cmd.Parameters.AddWithValue("@decInventariosNoCorriente", D2(r.InventariosNoCorriente));
                cmd.Parameters.AddWithValue("@decActivosBiologicosNoCorriente", D2(r.ActivosBiologicosNoCorriente));
                cmd.Parameters.AddWithValue("@decPropiedadesInversion", D2(r.PropiedadesInversion));
                cmd.Parameters.AddWithValue("@decPropiedadesPlantaEquipo", D2(r.PropiedadesPlantaEquipo));
                cmd.Parameters.AddWithValue("@decIntangibles", D2(r.Intangibles));
                cmd.Parameters.AddWithValue("@decActivosImpuestosDiferidos", D2(r.ActivosImpuestosDiferidos));
                cmd.Parameters.AddWithValue("@decActivosImpuestosCorrientes", D2(r.ActivosImpuestosCorrientes));
                cmd.Parameters.AddWithValue("@decPlusvalia", D2(r.Plusvalia));
                cmd.Parameters.AddWithValue("@decOtrosActivosNoFinancierosNoCorriente", D2(r.OtrosActivosNoFinancierosNoCorriente));
                cmd.Parameters.AddWithValue("@decOtrosPasivosFinancierosCorriente", D2(r.OtrosPasivosFinancierosCorriente));
                cmd.Parameters.AddWithValue("@decCuentasPagarCorriente", D2(r.CuentasPagarCorriente));
                cmd.Parameters.AddWithValue("@decBeneficiosEmpleadosCorriente", D2(r.BeneficiosEmpleadosCorriente));
                cmd.Parameters.AddWithValue("@decOtrasProvisionesCorriente", D2(r.OtrasProvisionesCorriente));
                cmd.Parameters.AddWithValue("@decImpuestosGananciasCorriente", D2(r.ImpuestosGananciasCorriente));
                cmd.Parameters.AddWithValue("@decOtrosPasivosNoFinancierosCorriente", D2(r.OtrosPasivosNoFinancierosCorriente));
                cmd.Parameters.AddWithValue("@decOtrosPasivosFinancierosNoCorriente", D2(r.OtrosPasivosFinancierosNoCorriente));
                cmd.Parameters.AddWithValue("@decCuentasPagarNoCorriente", D2(r.CuentasPagarNoCorriente));
                cmd.Parameters.AddWithValue("@decBeneficiosEmpleadosNoCorriente", D2(r.BeneficiosEmpleadosNoCorriente));
                cmd.Parameters.AddWithValue("@decOtrasProvisionesNoCorriente", D2(r.OtrasProvisionesNoCorriente));
                cmd.Parameters.AddWithValue("@decImpuestosDiferidosNoCorriente", D2(r.ImpuestosDiferidosNoCorriente));
                cmd.Parameters.AddWithValue("@decImpuestosCorrientesNoCorriente", D2(r.ImpuestosCorrientesNoCorriente));
                cmd.Parameters.AddWithValue("@decOtrosPasivosNoFinancierosNoCorriente", D2(r.OtrosPasivosNoFinancierosNoCorriente));
                cmd.Parameters.AddWithValue("@decCapitalEmitido", D2(r.CapitalEmitido));
                cmd.Parameters.AddWithValue("@decPrimasEmision", D2(r.PrimasEmision));
                cmd.Parameters.AddWithValue("@decAccionesInversion", D2(r.AccionesInversion));
                cmd.Parameters.AddWithValue("@decAccionesCartera", D2(r.AccionesCartera));
                cmd.Parameters.AddWithValue("@decOtrasReservasCapital", D2(r.OtrasReservasCapital));
                cmd.Parameters.AddWithValue("@decResultadosAcumulados", D2(r.ResultadosAcumulados));
                cmd.Parameters.AddWithValue("@decOtrasReservasPatrimonio", D2(r.OtrasReservasPatrimonio));
                cmd.Parameters.AddWithValue("@decIngresosOrdinarios", D2(r.IngresosOrdinarios));
                cmd.Parameters.AddWithValue("@decCostoVentas", D2(r.CostoVentas));
                cmd.Parameters.AddWithValue("@decGastosVentas", D2(r.GastosVentas));
                cmd.Parameters.AddWithValue("@decGastosAdministracion", D2(r.GastosAdministracion));
                cmd.Parameters.AddWithValue("@decOtrosIngresosOperativos", D2(r.OtrosIngresosOperativos));
                cmd.Parameters.AddWithValue("@decOtrosGastosOperativos", D2(r.OtrosGastosOperativos));
                cmd.Parameters.AddWithValue("@decOtrasGananciasPerdidas", D2(r.OtrasGananciasPerdidas));
                cmd.Parameters.AddWithValue("@decIngresosFinancieros", D2(r.IngresosFinancieros));
                cmd.Parameters.AddWithValue("@decIngresosIntereses", D2(r.IngresosIntereses));
                cmd.Parameters.AddWithValue("@decGastosFinancieros", D2(r.GastosFinancieros));
                cmd.Parameters.AddWithValue("@decDeterioroValor", D2(r.DeterioroValor));
                cmd.Parameters.AddWithValue("@decOtrosIngresosSubsidiarias", D2(r.OtrosIngresosSubsidiarias));
                cmd.Parameters.AddWithValue("@decDiferenciasCambio", D2(r.DiferenciasCambio));
                cmd.Parameters.AddWithValue("@decIngresoGastoImpuesto", D2(r.IngresoGastoImpuesto));
                cmd.Parameters.AddWithValue("@decOperacionesDescontinuadas", D2(r.OperacionesDescontinuadas));
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Balance_Seguro_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@decEfectivoDisponible", D2(r.EfectivoDisponible));
                cmd.Parameters.AddWithValue("@decInversionesFinancieras", D2(r.InversionesFinancieras));
                cmd.Parameters.AddWithValue("@decPrestamosInteresesNetos", D2(r.PrestamosInteresesNetos));
                cmd.Parameters.AddWithValue("@decPrimasCobrar", D2(r.PrimasCobrar));
                cmd.Parameters.AddWithValue("@decDeudasReaseguradores", D2(r.DeudasReaseguradores));
                cmd.Parameters.AddWithValue("@decActivosVenta", D2(r.ActivosVenta));
                cmd.Parameters.AddWithValue("@decPropiedadesInversion", D2(r.PropiedadesInversion));
                cmd.Parameters.AddWithValue("@decPropiedadPlantaEquipo", D2(r.PropiedadPlantaEquipo));
                cmd.Parameters.AddWithValue("@decOtrosActivos", D2(r.OtrosActivos));
                cmd.Parameters.AddWithValue("@decObligacionesAsegurados", D2(r.ObligacionesAsegurados));
                cmd.Parameters.AddWithValue("@decReservasSiniestros", D2(r.ReservasSiniestros));
                cmd.Parameters.AddWithValue("@decReservasTecnicas", D2(r.ReservasTecnicas));
                cmd.Parameters.AddWithValue("@decObligacionesReaseguradores", D2(r.ObligacionesReaseguradores));
                cmd.Parameters.AddWithValue("@decObligacionesFinancieras", D2(r.ObligacionesFinancieras));
                cmd.Parameters.AddWithValue("@decCuentasPagar", D2(r.CuentasPagar));
                cmd.Parameters.AddWithValue("@decOtrosPasivos", D2(r.OtrosPasivos));
                cmd.Parameters.AddWithValue("@decCapitalSocial", D2(r.CapitalSocial));
                cmd.Parameters.AddWithValue("@decAportesCapitalNoCapitalizados", D2(r.AportesCapitalNoCapitalizados));
                cmd.Parameters.AddWithValue("@decResultadosAcumulados", D2(r.ResultadosAcumulados));
                cmd.Parameters.AddWithValue("@decPatrimonioRestringido", D2(r.PatrimonioRestringido));
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Balance_Banco_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@decDisponible", D2(r.Disponible));
                cmd.Parameters.AddWithValue("@decFondosInterbancarios", D2(r.FondosInterbancarios));
                cmd.Parameters.AddWithValue("@decInversionesValorRazonable", D2(r.InversionesValorRazonable));
                cmd.Parameters.AddWithValue("@decCarteraCreditos", D2(r.CarteraCreditos));
                cmd.Parameters.AddWithValue("@decDerivadosNegociacionActivo", D2(r.DerivadosNegociacionActivo));
                cmd.Parameters.AddWithValue("@decDerivadosCoberturaActivo", D2(r.DerivadosCoberturaActivo));
                cmd.Parameters.AddWithValue("@decBienesRealizables", D2(r.BienesRealizables));
                cmd.Parameters.AddWithValue("@decParticipacionesSubsidiarias", D2(r.ParticipacionesSubsidiarias));
                cmd.Parameters.AddWithValue("@decInmuebleMobiliarioEquipo", D2(r.InmuebleMobiliarioEquipo));
                cmd.Parameters.AddWithValue("@decImpuestoRentaDiferido", D2(r.ImpuestoRentaDiferido));
                cmd.Parameters.AddWithValue("@decOtrosActivos", D2(r.OtrosActivos));
                cmd.Parameters.AddWithValue("@decObligacionesPublico", D2(r.ObligacionesPublico));
                cmd.Parameters.AddWithValue("@decFondosInterbancariosPasivo", D2(r.FondosInterbancariosPasivo));
                cmd.Parameters.AddWithValue("@decAdeudosFinancieras", D2(r.AdeudosFinancieras));
                cmd.Parameters.AddWithValue("@decDerivadosNegociacionPasivo", D2(r.DerivadosNegociacionPasivo));
                cmd.Parameters.AddWithValue("@decDerivadosCoberturaPasivo", D2(r.DerivadosCoberturaPasivo));
                cmd.Parameters.AddWithValue("@decCuentasPagarProvisiones", D2(r.CuentasPagarProvisiones));
                cmd.Parameters.AddWithValue("@decCapitalSocial", D2(r.CapitalSocial));
                cmd.Parameters.AddWithValue("@decReservas", D2(r.Reservas));
                cmd.Parameters.AddWithValue("@decResultadosNoRealizados", D2(r.ResultadosNoRealizados));
                cmd.Parameters.AddWithValue("@decResultadoEjercicio", D2(r.ResultadoEjercicio));
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Balance_Turquia_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@decEfectivo", D2(r.Efectivo));
                cmd.Parameters.AddWithValue("@decExistencias", D2(r.Existencias));
                cmd.Parameters.AddWithValue("@decDeudores", D2(r.Deudores));
                cmd.Parameters.AddWithValue("@decBienesTongibles", D2(r.BienesTongibles));
                cmd.Parameters.AddWithValue("@decActivosIntangibles", D2(r.ActivosIntangibles));
                cmd.Parameters.AddWithValue("@decPrestamos", D2(r.Prestamos));
                cmd.Parameters.AddWithValue("@decAcreedores", D2(r.Acreedores));
                cmd.Parameters.AddWithValue("@decPasivosNoCorrientes", D2(r.PasivosNoCorrientes));
                cmd.Parameters.AddWithValue("@decPasivosLargoPlazo", D2(r.PasivosLargoPlazo));
                cmd.Parameters.AddWithValue("@decPatrimonio", D2(r.Patrimonio));
                cmd.Parameters.AddWithValue("@decReservas", D2(r.Reservas));
                cmd.Parameters.AddWithValue("@decResultadosAcumulados", D2(r.ResultadosAcumulados));
                cmd.Parameters.AddWithValue("@decPerdidaGanancias", D2(r.PerdidaGanancias));
                cmd.Parameters.AddWithValue("@decOtrasCuentas", D2(r.OtrasCuentas));
                cmd.Parameters.AddWithValue("@decVentasNetas", D2(r.VentasNetas));
                cmd.Parameters.AddWithValue("@decCostoVentas", D2(r.CostoVentas));
                cmd.Parameters.AddWithValue("@decOtrosGastosOperativos", D2(r.OtrosGastosOperativos));
                cmd.Parameters.AddWithValue("@decCostoEmpleados", D2(r.CostoEmpleados));
                cmd.Parameters.AddWithValue("@decDepreciacion", D2(r.Depreciacion));
                cmd.Parameters.AddWithValue("@decIngresosFinancieros", D2(r.IngresosFinancieros));
                cmd.Parameters.AddWithValue("@decGastosFinancieros", D2(r.GastosFinancieros));
                cmd.Parameters.AddWithValue("@decIngresosExtraordinarios", D2(r.IngresosExtraordinarios));
                cmd.Parameters.AddWithValue("@decGastosExtraordinarios", D2(r.GastosExtraordinarios));
                cmd.Parameters.AddWithValue("@decImpuestos", D2(r.Impuestos));
                cmd.Parameters.AddWithValue("@decCostoMateriales", D2(r.CostoMateriales));
                cmd.Parameters.AddWithValue("@decInteresesPagados", D2(r.InteresesPagados));
                cmd.Parameters.AddWithValue("@decCapital", D2(r.Capital));
                cmd.Parameters.AddWithValue("@decEbit", D2(r.Ebit));
                cmd.Parameters.AddWithValue("@decEbitda", D2(r.Ebitda));
                cmd.Parameters.AddWithValue("@decGanancia", D2(r.Ganancia));
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Balance_Totalizado_Calcular", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@decTotalActivoCorriente", D2(r.TotalActivoCorriente));
                cmd.Parameters.AddWithValue("@decTotalActivoNoCorriente", D2(r.TotalActivoNoCorriente));
                cmd.Parameters.AddWithValue("@decTotalPasivoCorriente", D2(r.TotalPasivoCorriente));
                cmd.Parameters.AddWithValue("@decTotalPasivoNoCorriente", D2(r.TotalPasivoNoCorriente));
                cmd.Parameters.AddWithValue("@decTotalPatrimonio", D2(r.TotalPatrimonio));
                cmd.Parameters.AddWithValue("@decIngresosOrdinarios", D2(r.IngresosOrdinarios));
                cmd.Parameters.AddWithValue("@decGananciaNeta", D2(r.GananciaNeta));
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_ObtenerOCrear", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
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

        public async Task<Respuesta> ObtenerEvolucionAsync(UsuarioGeneral usuarioLogueado, EvolucionInformesRequest filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Informe_EvolucionDashboard", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdColaborador", (object?)filtro.idColaborador ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdRolInforme", (object?)filtro.rol ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtmFechaDesde", (object?)filtro.fechaDesde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtmFechaHasta", (object?)filtro.fechaHasta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intGranularidad", filtro.granularidad);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new List<EvolucionInformesConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        resultado.Add(new EvolucionInformesConsulta
                        {
                            Periodo = dr["Periodo"].ToString() ?? string.Empty,
                            Etiqueta = dr["Etiqueta"].ToString() ?? string.Empty,
                            CantidadInformes = Convert.ToInt32(dr["CantidadInformes"])
                        });
                    }
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<EvolucionInformesConsulta>() };
            }
        }

    }
}
