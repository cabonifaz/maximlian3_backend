using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

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

        private static string? GetNullableString(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static int? GetNullableInt(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(DbDataReader dr, string columna) =>
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFacturaLinea_Crear", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", idCliente);
                cmd.Parameters.AddWithValue("@vchCodigo", (object?)codigo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchDescripcion", descripcion);
                cmd.Parameters.AddWithValue("@lstIdPedido", JsonSerializer.Serialize(idPedidos));

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

        // Versión en lote de CrearAsync — el llamador arma los grupos (idsPedido + los 5 campos de
        // la línea por grupo), una PEDIDO_FACTURA_LINEA por grupo. IdGrupo (1-based, orden de la
        // lista) une @tvpLineas con @lstIdPedido — ver SP_PedidoFacturaLinea_CrearLote.
        public async Task<Respuesta> CrearLoteAsync(UsuarioGeneral usuarioLogueado, int idCliente, List<GrupoLineaLoteRequest> grupos)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFacturaLinea_CrearLote", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", idCliente);

                var lineasJson = new List<object>();
                var pedidosJson = new List<object>();
                var idGrupo = 1;
                foreach (var grupo in grupos)
                {
                    lineasJson.Add(new { IdGrupo = idGrupo, grupo.codigo, grupo.descripcion, grupo.valorUnitario, grupo.descuento });
                    foreach (var idPedido in grupo.idsPedido)
                        pedidosJson.Add(new { ID = idGrupo, NUM1 = idPedido });
                    idGrupo++;
                }

                cmd.Parameters.AddWithValue("@tvpLineas", JsonSerializer.Serialize(lineasJson));
                cmd.Parameters.AddWithValue("@lstIdPedido", JsonSerializer.Serialize(pedidosJson));

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lineas = new List<PedidoFacturaLineaConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        lineas.Add(new PedidoFacturaLineaConsulta
                        {
                            IdPedidoFacturaLinea = Convert.ToInt32(dr["IdPedidoFacturaLinea"]),
                            Codigo = dr["Codigo"] as string,
                            Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                            Cantidad = Convert.ToInt32(dr["Cantidad"]),
                            ValorUnitario = Convert.ToDecimal(dr["ValorUnitario"]),
                            Descuento = Convert.ToDecimal(dr["Descuento"]),
                        });
                }

                respuesta.Result = lineas;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<PedidoFacturaLineaConsulta>() };
            }
        }

        public async Task<Respuesta> ActualizarDatosAsync(
            UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea, string? codigo, string descripcion,
            decimal valorUnitario, decimal descuento)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFacturaLinea_ActualizarDatos", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedidoFacturaLinea", idPedidoFacturaLinea);
                cmd.Parameters.AddWithValue("@vchCodigo", (object?)codigo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchDescripcion", descripcion);
                cmd.Parameters.AddWithValue("@decValorUnitario", valorUnitario);
                cmd.Parameters.AddWithValue("@decDescuento", descuento);

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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFacturaLinea_ActualizarPedidos", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedidoFacturaLinea", idPedidoFacturaLinea);
                cmd.Parameters.AddWithValue("@intIdCliente", idCliente);
                cmd.Parameters.AddWithValue("@lstIdPedido", JsonSerializer.Serialize(idPedidos));

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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFacturaLinea_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", request.idCliente);
                cmd.Parameters.AddWithValue("@intAnio", (object?)request.anio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intMes", (object?)request.mes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdDocumentoElectronico", (object?)request.idDocumentoElectronico ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdMoneda", (object?)request.idMoneda ?? DBNull.Value);

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
                            IdDocumentoElectronico = GetNullableInt(dr, "IdDocumentoElectronico"),
                            Codigo = dr["Codigo"] as string,
                            Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                            IdMoneda = GetNullableInt(dr, "IdMoneda"),
                            Moneda = GetNullableString(dr, "Moneda"),
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_DesvincularLinea", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdPedidoFacturaLinea", idPedidoFacturaLinea);

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

        // Insumo de PedidoFacturaHandler.GuardarBorradorFacturaAsync/GuardarCambiosFacturaAsync:
        // montos ya congelados de las líneas — ver SP_PedidoFacturaLinea_ObtenerParaBorrador.
        // idMoneda es la moneda a facturar (request.idMonedaMaestro del payload) — el SP la valida
        // contra la de cada línea. idDocumentoElectronico es opcional: además de las libres, también
        // trae las líneas ya asociadas a ese documento (edición de un borrador existente).
        public async Task<Respuesta> ObtenerParaBorradorAsync(
            UsuarioGeneral usuarioLogueado, int idCliente, int idMoneda, List<int> idsLinea, int? idDocumentoElectronico = null)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFacturaLinea_ObtenerParaBorrador", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", idCliente);
                cmd.Parameters.AddWithValue("@intIdMoneda", idMoneda);
                cmd.Parameters.AddWithValue("@intIdDocumentoElectronico", (object?)idDocumentoElectronico ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@lstIdPedidoFacturaLinea", JsonSerializer.Serialize(idsLinea));

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
                            IdDocumentoElectronico = GetNullableInt(dr, "IdDocumentoElectronico"),
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
