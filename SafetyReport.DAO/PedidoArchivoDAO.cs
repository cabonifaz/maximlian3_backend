using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;

namespace SafetyReport.DAO
{
    public class PedidoArchivoDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<PedidoArchivoDAO> _logger;

        public PedidoArchivoDAO(DbConfig dbConfig, ILogger<PedidoArchivoDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

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

        private static string? GetNullableString(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoCrear request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoArchivo_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", request.IdPedido);
                cmd.Parameters.AddWithValue("@vchDocumentoURL", request.DocumentoURL);
                cmd.Parameters.AddWithValue("@vchNombreDocumento", request.NombreDocumento);
                cmd.Parameters.AddWithValue("@vchFormatoDocumento", request.FormatoDocumento);
                cmd.Parameters.AddWithValue("@bigTamanoArchivo", request.TamanoArchivo);
                cmd.Parameters.AddWithValue("@intIdTipoArchivo", request.IdTipoArchivo);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoArchivoCreado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new PedidoArchivoCreado { DocumentoURL = GetNullableString(dr, "DocumentoURL") ?? string.Empty });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoEditar request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoArchivo_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedidoArchivo", request.IdPedidoArchivo);
                cmd.Parameters.AddWithValue("@intIdPedido", request.IdPedido);
                cmd.Parameters.AddWithValue("@vchDocumentoURL", request.DocumentoURL);
                cmd.Parameters.AddWithValue("@vchNombreDocumento", request.NombreDocumento);
                cmd.Parameters.AddWithValue("@vchFormatoDocumento", request.FormatoDocumento);
                cmd.Parameters.AddWithValue("@bigTamanoArchivo", request.TamanoArchivo);
                cmd.Parameters.AddWithValue("@intIdEstado", request.IdEstado);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoArchivoCreado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new PedidoArchivoCreado { DocumentoURL = GetNullableString(dr, "DocumentoURL") ?? string.Empty });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoArchivo_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedidoArchivo", request.IdPedidoArchivo);
                cmd.Parameters.AddWithValue("@intIdPedido", request.IdPedido);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoArchivoConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        lista.Add(new PedidoArchivoConsulta
                        {
                            IdPedidoArchivo = Convert.ToInt32(dr["IdPedidoArchivo"]),
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            DocumentoURL = GetNullableString(dr, "DocumentoURL") ?? string.Empty,
                            NombreDocumento = GetNullableString(dr, "NombreDocumento") ?? string.Empty,
                            IdFormato = Convert.ToInt32(dr["IdFormato"]),
                            IdEstado = Convert.ToInt32(dr["IdEstado"]),
                            TamanoArchivo = Convert.ToInt64(dr["TamanoArchivo"]),
                            IdTipoArchivo = Convert.ToInt32(dr["IdTipoArchivo"])
                        });
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoArchivo request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoArchivo_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", request.idPedido);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)request.busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEstado", (object?)request.idEstado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)request.numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoArchivoListaResult();
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
                            resultado.lstPedidoArchivo.Add(new PedidoArchivoListaConsulta
                            {
                                IdPedidoArchivo = Convert.ToInt32(dr["IdPedidoArchivo"]),
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                DocumentoURL = GetNullableString(dr, "DocumentoURL") ?? string.Empty,
                                NombreDocumento = GetNullableString(dr, "NombreDocumento") ?? string.Empty,
                                TamanoArchivo = Convert.ToInt64(dr["TamanoArchivo"]),
                                IdFormato = Convert.ToInt32(dr["IdFormato"]),
                                TipoFormato = GetNullableString(dr, "TipoFormato") ?? string.Empty,
                                IdTipoArchivo = Convert.ToInt32(dr["IdTipoArchivo"]),
                                IdEstado = Convert.ToInt32(dr["IdEstado"]),
                                FechaCarga = GetNullableString(dr, "FechaCarga") ?? string.Empty
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

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoArchivoListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoArchivo_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedidoArchivo", request.IdPedidoArchivo);
                cmd.Parameters.AddWithValue("@intIdPedido", request.IdPedido);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoArchivoEliminado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new PedidoArchivoEliminado { IdPedidoArchivo = Convert.ToInt32(dr["IdPedidoArchivo"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoEliminado>()
                };
            }
        }

    }
}