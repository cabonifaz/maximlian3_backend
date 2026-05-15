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
            t.Columns.Add("TipoBalance", typeof(int));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++,
                    (object?)x.IdInformeBalance ?? DBNull.Value,
                    x.FechaBalance,
                    (object?)x.FechaHasta ?? DBNull.Value,
                    x.FlgActualidad,
                    (object?)x.TipoCambio ?? DBNull.Value,
                    x.IdMoneda, x.TipoBalance);
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
            t.Columns.Add("ReferenciaBanco", typeof(string));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++, (object?)x.IdInformeBanco ?? DBNull.Value, x.IdBanco,
                    (object?)x.NumeroCuenta ?? DBNull.Value, (object?)x.IdSector ?? DBNull.Value,
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
                    (object?)x.Comentarios ?? DBNull.Value);
            return t;
        }

        private static DataTable ConstruirTablaCuentaBalances(List<InformeBalanceItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("TotalCorriente", typeof(decimal));
            t.Columns.Add("TotalNoCorriente", typeof(decimal));
            t.Columns.Add("OtrosActivos", typeof(decimal));
            t.Columns.Add("TotalActivos", typeof(decimal));
            t.Columns.Add("TotalPasivosCorrientes", typeof(decimal));
            t.Columns.Add("TotalPasivosNoCorrientes", typeof(decimal));
            t.Columns.Add("OtrosPasivos", typeof(decimal));
            t.Columns.Add("TotalPasivos", typeof(decimal));
            t.Columns.Add("Patrimonio", typeof(decimal));
            t.Columns.Add("TotalPasivoPatrimonio", typeof(decimal));
            t.Columns.Add("VentasNetas", typeof(decimal));
            t.Columns.Add("UtilidadPerdida", typeof(decimal));
            t.Columns.Add("IndiceLiquidez", typeof(decimal));
            t.Columns.Add("CapitalTrabajo", typeof(decimal));
            t.Columns.Add("RatioEndeudamiento", typeof(decimal));
            t.Columns.Add("RatioRentabilidad", typeof(decimal));
            int i = 1;
            foreach (var x in items)
            {
                var cb = x.CuentaBalance;
                if (cb != null)
                    t.Rows.Add(i,
                        (object?)cb.TotalCorriente ?? DBNull.Value,
                        (object?)cb.TotalNoCorriente ?? DBNull.Value,
                        (object?)cb.OtrosActivos ?? DBNull.Value,
                        (object?)cb.TotalActivos ?? DBNull.Value,
                        (object?)cb.TotalPasivosCorrientes ?? DBNull.Value,
                        (object?)cb.TotalPasivosNoCorrientes ?? DBNull.Value,
                        (object?)cb.OtrosPasivos ?? DBNull.Value,
                        (object?)cb.TotalPasivos ?? DBNull.Value,
                        (object?)cb.Patrimonio ?? DBNull.Value,
                        (object?)cb.TotalPasivoPatrimonio ?? DBNull.Value,
                        (object?)cb.VentasNetas ?? DBNull.Value,
                        (object?)cb.UtilidadPerdida ?? DBNull.Value,
                        (object?)cb.IndiceLiquidez ?? DBNull.Value,
                        (object?)cb.CapitalTrabajo ?? DBNull.Value,
                        (object?)cb.RatioEndeudamiento ?? DBNull.Value,
                        (object?)cb.RatioRentabilidad ?? DBNull.Value);
                i++;
            }
            return t;
        }

        private static DataTable ConstruirTablaDirectorioEjecutivo(List<InformeDirectorioEjecutivoItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformeDirectorioEjecutivo", typeof(int));
            t.Columns.Add("IdTipoPersona", typeof(int));
            t.Columns.Add("NombreCompleto", typeof(string));
            t.Columns.Add("IdPais", typeof(int));
            t.Columns.Add("Direccion", typeof(string));
            t.Columns.Add("Ubigeo", typeof(string));
            t.Columns.Add("CodigoPostal", typeof(string));
            t.Columns.Add("IdTipoDocumento", typeof(int));
            t.Columns.Add("NumeroDocumento", typeof(string));
            t.Columns.Add("TaxIdType", typeof(int));
            t.Columns.Add("TaxNum", typeof(string));
            t.Columns.Add("IdNacionalidad", typeof(int));
            t.Columns.Add("FechaNacimiento", typeof(DateTime));
            t.Columns.Add("IdEstadoCivil", typeof(int));
            t.Columns.Add("IdProfesion", typeof(int));
            t.Columns.Add("Referencias", typeof(string));
            t.Columns.Add("Cargos", typeof(string));
            t.Columns.Add("FormularioVinculado", typeof(string));
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
                    (object?)x.IdTipoPersona ?? DBNull.Value,
                    (object?)x.NombreCompleto ?? DBNull.Value,
                    (object?)x.IdPais ?? DBNull.Value,
                    (object?)x.Direccion ?? DBNull.Value,
                    (object?)x.Ubigeo ?? DBNull.Value,
                    (object?)x.CodigoPostal ?? DBNull.Value,
                    (object?)x.IdTipoDocumento ?? DBNull.Value,
                    (object?)x.NumeroDocumento ?? DBNull.Value,
                    (object?)x.TaxIdType ?? DBNull.Value,
                    (object?)x.TaxNum ?? DBNull.Value,
                    (object?)x.IdNacionalidad ?? DBNull.Value,
                    (object?)x.FechaNacimiento ?? DBNull.Value,
                    (object?)x.IdEstadoCivil ?? DBNull.Value,
                    (object?)x.IdProfesion ?? DBNull.Value,
                    (object?)x.Referencias ?? DBNull.Value,
                    (object?)x.Cargos ?? DBNull.Value,
                    (object?)x.FormularioVinculado ?? DBNull.Value,
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
            t.Columns.Add("ImagenUrl", typeof(string));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++,
                    (object?)x.IdInformeLocal ?? DBNull.Value,
                    (object?)x.IdTipoLocal ?? DBNull.Value,
                    (object?)x.Comentario ?? DBNull.Value,
                    (object?)x.ImagenUrl ?? DBNull.Value);
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
            int img = 1;
            int localIdx = 1;
            foreach (var local in items)
            {
                foreach (var imagen in local.Imagenes)
                    t.Rows.Add(img++, (object?)imagen.IdInformeLocalImagen ?? DBNull.Value,
                        localIdx, imagen.ImagenURL, imagen.IdTipoArchivo);
                localIdx++;
            }
            return t;
        }

        private static DataTable ConstruirTablaInformePedidos(List<InformePedidoItem> items)
        {
            var t = new DataTable();
            t.Columns.Add("ID", typeof(int));
            t.Columns.Add("IdInformePedido", typeof(int));
            t.Columns.Add("IdPedido", typeof(int));
            t.Columns.Add("IdIdioma", typeof(int));
            t.Columns.Add("DocumentoWord", typeof(string));
            t.Columns.Add("DocumentoExcel", typeof(string));
            t.Columns.Add("IdEstado", typeof(int));
            int i = 1;
            foreach (var x in items)
                t.Rows.Add(i++, (object?)x.IdInformePedido ?? DBNull.Value, x.IdPedido, x.IdIdioma,
                    (object?)x.DocumentoWord ?? DBNull.Value,
                    (object?)x.DocumentoExcel ?? DBNull.Value,
                    x.IdEstado);
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
            cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = (object?)r.IdEstado ?? DBNull.Value;
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
            cmd.Parameters.Add("@vchAntecedentes", SqlDbType.VarChar, -1).Value = (object?)r.Antecedentes ?? DBNull.Value;
            cmd.Parameters.Add("@vchAspectosLegales", SqlDbType.VarChar, -1).Value = (object?)r.AspectosLegales ?? DBNull.Value;
            cmd.Parameters.Add("@vchComentariosAspectoLegal", SqlDbType.VarChar, -1).Value = (object?)r.ComentariosAspectoLegal ?? DBNull.Value;
            cmd.Parameters.Add("@intIdSector", SqlDbType.Int).Value = (object?)r.IdSector ?? DBNull.Value;
            cmd.Parameters.Add("@intIdActividad", SqlDbType.Int).Value = (object?)r.IdActividad ?? DBNull.Value;
            cmd.Parameters.Add("@intIdIsicCategoria", SqlDbType.Int).Value = (object?)r.IdIsicCategoria ?? DBNull.Value;
            cmd.Parameters.Add("@intIdIsicClase", SqlDbType.Int).Value = (object?)r.IdIsicClase ?? DBNull.Value;
            cmd.Parameters.Add("@vchActividadPrincipal", SqlDbType.VarChar, -1).Value = (object?)r.ActividadPrincipal ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasContado", SqlDbType.Decimal).Value = (object?)r.VentasContado ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasContadoText", SqlDbType.VarChar, 50).Value = (object?)r.VentasContadoText ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasCredito", SqlDbType.Decimal).Value = (object?)r.VentasCredito ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasCreditoText", SqlDbType.VarChar, 50).Value = (object?)r.VentasCreditoText ?? DBNull.Value;
            cmd.Parameters.Add("@intIdVentasCreditoTiempo", SqlDbType.Int).Value = (object?)r.IdVentasCreditoTiempo ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasNacionales", SqlDbType.Decimal).Value = (object?)r.VentasNacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasNacionalesText", SqlDbType.VarChar, 50).Value = (object?)r.VentasNacionalesText ?? DBNull.Value;
            cmd.Parameters.Add("@decVentasInternacionales", SqlDbType.Decimal).Value = (object?)r.VentasInternacionales ?? DBNull.Value;
            cmd.Parameters.Add("@vchVentasInternacionalesText", SqlDbType.VarChar, 50).Value = (object?)r.VentasInternacionalesText ?? DBNull.Value;
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
        }

        private static void AgregarTvpsCampos(SqlCommand cmd, InformeCrear r)
        {
            AgregarTvp(cmd, "@lstBalances", ConstruirTablaBalances(r.Balances), "LISTA_INFORME_BALANCE");
            AgregarTvp(cmd, "@lstCuentaBalances", ConstruirTablaCuentaBalances(r.Balances), "LISTA_INFORME_CUENTA_BALANCE");
            AgregarTvp(cmd, "@lstBancos", ConstruirTablaBancos(r.Bancos), "LISTA_INFORME_BANCO");
            AgregarTvp(cmd, "@lstCompanias", ConstruirTablaCompanias(r.CompaniasRelacionadas), "LISTA_INFORME_COMPANIA_RELACIONADA");
            AgregarTvp(cmd, "@lstExpImp", ConstruirTablaExpImp(r.ExportacionesImportaciones), "LISTA_INFORME_EXPORTACION_IMPORTACION");
            AgregarTvp(cmd, "@lstProveedores", ConstruirTablaProveedores(r.Proveedores), "LISTA_INFORME_PROVEEDOR");
            AgregarTvp(cmd, "@lstDirectoriosEjecutivos", ConstruirTablaDirectorioEjecutivo(r.DirectoriosEjecutivos), "LISTA_INFORME_DIRECTORIO_EJECUTIVO");
            AgregarTvp(cmd, "@lstLocales", ConstruirTablaLocales(r.Locales), "LISTA_INFORME_LOCAL");
            AgregarTvp(cmd, "@lstLocalImagenes", ConstruirTablaLocalImagenes(r.Locales), "LISTA_INFORME_LOCAL_IMAGEN");
            AgregarTvp(cmd, "@lstPedidos", ConstruirTablaInformePedidos(r.Pedidos), "LISTA_INFORME_PEDIDO");
        }

        // ── CRUD ─────────────────────────────────────────────────────────────────

        public async Task<Respuesta> InsertarAsync(UsuarioGeneral u, InformeCrear request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Informe_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarParametrosCampos(cmd, request);
                AgregarTvpsCampos(cmd, request);
                await cn.OpenAsync();
                return await LeerRespuestaAsync<InformeCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() };
            }
        }

        public async Task<Respuesta> ActualizarAsync(UsuarioGeneral u, InformeEditar request)
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
                return await LeerRespuestaAsync<InformeCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral u, int idInforme)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Informe_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
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
