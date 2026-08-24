using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

namespace SafetyReport.DAO
{
    // Capa de datos del sub-recurso PEDIDO_FACTURA_LINEA — separado de PedidoFacturaDAO porque
    // agrupa exclusivamente el CRUD de líneas, no el resto del módulo de facturación (documentos,
    // notas, cuotas, etc.) — ver PLAN_Lineas_Facturacion.md.
    public class PedidoFacturaLineaDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<PedidoFacturaLineaDAO> _logger;

        public PedidoFacturaLineaDAO(DbConfig dbConfig, ILogger<PedidoFacturaLineaDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static DataTable ConstruirTablaListaGeneralNum(List<int> valores)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("NUM1", typeof(int));

            int i = 1;
            foreach (var valor in valores)
                table.Rows.Add(i++, valor);

            return table;
        }

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

        private static string? GetNullableString(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static int? GetNullableInt(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        // Agrupa idPedidos (mismo cliente/mes/tarifa, validado en el SP) bajo una línea nueva.
        // La línea nace libre (IdDocumentoElectronico NULL) — se asocia después vía
        // PedidoFacturaDAO.RegistrarEnvioAsync — ver PLAN_Lineas_Facturacion.md. idCliente se
        // valida en el SP contra cada pedido de idPedidos, no solo entre sí.
        public async Task<Respuesta> CrearAsync(
            UsuarioGeneral usuarioLogueado, int idCliente, List<int> idPedidos, string? codigo, string descripcion)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFacturaLinea_Crear", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;
                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 30).Value = (object?)codigo ?? DBNull.Value;
                cmd.Parameters.Add("@vchDescripcion", SqlDbType.VarChar, 500).Value = descripcion;

                var tvpIdPedido = cmd.Parameters.AddWithValue("@lstIdPedido", ConstruirTablaListaGeneralNum(idPedidos));
                tvpIdPedido.SqlDbType = SqlDbType.Structured;
                tvpIdPedido.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    respuesta.Result = new PedidoFacturaLineaConsulta
                    {
                        IdPedidoFacturaLinea = Convert.ToInt32(dr["IdPedidoFacturaLinea"]),
                        Codigo = dr["Codigo"] as string,
                        Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                        Cantidad = Convert.ToInt32(dr["Cantidad"]),
                        ValorUnitario = Convert.ToDecimal(dr["ValorUnitario"]),
                        Descuento = Convert.ToDecimal(dr["Descuento"]),
                        DescuentoPorcentaje = dr["DescuentoPorcentaje"] as decimal?
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ActualizarDatosAsync(
            UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea, string? codigo, string descripcion)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFacturaLinea_ActualizarDatos", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedidoFacturaLinea", SqlDbType.Int).Value = idPedidoFacturaLinea;
                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 30).Value = (object?)codigo ?? DBNull.Value;
                cmd.Parameters.Add("@vchDescripcion", SqlDbType.VarChar, 500).Value = descripcion;

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    respuesta.Result = new PedidoFacturaLineaConsulta
                    {
                        IdPedidoFacturaLinea = Convert.ToInt32(dr["IdPedidoFacturaLinea"]),
                        Codigo = dr["Codigo"] as string,
                        Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                        Cantidad = Convert.ToInt32(dr["Cantidad"]),
                        ValorUnitario = Convert.ToDecimal(dr["ValorUnitario"]),
                        Descuento = Convert.ToDecimal(dr["Descuento"]),
                        DescuentoPorcentaje = dr["DescuentoPorcentaje"] as decimal?
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Reemplaza la composición de la línea por idPedidos completo (no incremental) — ver
        // SP_PedidoFacturaLinea_ActualizarPedidos. Si la línea queda sin miembros, el SP la
        // soft-elimina y result set 2 no trae fila: Result queda null, no un objeto vacío.
        public async Task<Respuesta> ActualizarPedidosAsync(
            UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea, int idCliente, List<int> idPedidos)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFacturaLinea_ActualizarPedidos", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedidoFacturaLinea", SqlDbType.Int).Value = idPedidoFacturaLinea;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;

                var tvpIdPedido = cmd.Parameters.AddWithValue("@lstIdPedido", ConstruirTablaListaGeneralNum(idPedidos));
                tvpIdPedido.SqlDbType = SqlDbType.Structured;
                tvpIdPedido.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    respuesta.Result = new PedidoFacturaLineaConsulta
                    {
                        IdPedidoFacturaLinea = Convert.ToInt32(dr["IdPedidoFacturaLinea"]),
                        Codigo = dr["Codigo"] as string,
                        Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                        Cantidad = Convert.ToInt32(dr["Cantidad"]),
                        ValorUnitario = Convert.ToDecimal(dr["ValorUnitario"]),
                        Descuento = Convert.ToDecimal(dr["Descuento"]),
                        DescuentoPorcentaje = dr["DescuentoPorcentaje"] as decimal?
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Líneas libres (sin IdDocumentoElectronico) de un cliente, listas para asociar a un
        // documento vía PedidoFacturaDAO.RegistrarEnvioAsync — ver PLAN_Lineas_Facturacion.md.
        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, ListarLineasFacturacionRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFacturaLinea_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;
                cmd.Parameters.Add("@intAnio", SqlDbType.Int).Value = (object?)request.anio ?? DBNull.Value;
                cmd.Parameters.Add("@intMes", SqlDbType.Int).Value = (object?)request.mes ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoFacturaLineaListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.Lineas.Add(new PedidoFacturaLineaListaConsulta
                        {
                            IdPedidoFacturaLinea = Convert.ToInt32(dr["IdPedidoFacturaLinea"]),
                            Codigo = dr["Codigo"] as string,
                            Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                            IdTipoTramite = GetNullableInt(dr, "IdTipoTramite"),
                            TipoTramite = GetNullableString(dr, "TipoTramite"),
                            IdMoneda = GetNullableInt(dr, "IdMoneda"),
                            Moneda = GetNullableString(dr, "Moneda"),
                            Cantidad = Convert.ToInt32(dr["Cantidad"]),
                            ValorUnitario = Convert.ToDecimal(dr["ValorUnitario"]),
                            Descuento = Convert.ToDecimal(dr["Descuento"]),
                            DescuentoPorcentaje = GetNullableDecimal(dr, "DescuentoPorcentaje")
                        });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new PedidoFacturaLineaListaResult() };
            }
        }

        // Soft-delete de una línea + libera todos sus pedidos miembro. Sin chequeo de rol/permiso
        // adentro a propósito (llamable también desde cascades internos sin usuario logueado real)
        // — ver PLAN_Lineas_Facturacion.md.
        public async Task<Respuesta> DesvincularAsync(UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFactura_DesvincularLinea", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdPedidoFacturaLinea", SqlDbType.Int).Value = idPedidoFacturaLinea;

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();

                return await LeerCabeceraAsync(dr, cmd.CommandText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Insumo de PedidoFacturaHandler.GuardarBorradorFacturaAsync: montos ya congelados de las
        // líneas + el IdPedido de cada miembro (para IdExterno del documento) — ver
        // SP_PedidoFacturaLinea_ObtenerParaBorrador.
        public async Task<Respuesta> ObtenerParaBorradorAsync(UsuarioGeneral usuarioLogueado, int idCliente, List<int> idsLinea)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFacturaLinea_ObtenerParaBorrador", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;

                var tvpIdLinea = cmd.Parameters.AddWithValue("@lstIdPedidoFacturaLinea", ConstruirTablaListaGeneralNum(idsLinea));
                tvpIdLinea.SqlDbType = SqlDbType.Structured;
                tvpIdLinea.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new LineasParaBorradorConsulta();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.Lineas.Add(new PedidoFacturaLineaParaBorradorConsulta
                        {
                            IdPedidoFacturaLinea = Convert.ToInt32(dr["IdPedidoFacturaLinea"]),
                            Codigo = dr["Codigo"] as string,
                            Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                            Cantidad = Convert.ToInt32(dr["Cantidad"]),
                            ValorUnitario = Convert.ToDecimal(dr["ValorUnitario"]),
                            Descuento = Convert.ToDecimal(dr["Descuento"])
                        });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new LineasParaBorradorConsulta() };
            }
        }
    }
}
