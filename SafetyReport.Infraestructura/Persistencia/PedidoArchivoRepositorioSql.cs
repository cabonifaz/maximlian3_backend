using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.PedidoArchivo;
using System.Data;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class PedidoArchivoRepositorioSql : IPedidoArchivoRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<PedidoArchivoRepositorioSql> _logger;

        public PedidoArchivoRepositorioSql(DbConfig dbConfig, ILogger<PedidoArchivoRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje. Sin columna Result.
        private async Task<Respuesta> LeerCabeceraAsync(MySqlDataReader dr, string procedimiento)
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

        private static string? GetNullableString(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoCrear request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoArchivo_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = request.IdPedido;
                cmd.Parameters.Add("@p_vchDocumentoURL", MySqlDbType.VarChar).Value = request.DocumentoURL;
                cmd.Parameters.Add("@p_vchNombreDocumento", MySqlDbType.VarChar, 255).Value = request.NombreDocumento;
                cmd.Parameters.Add("@p_vchFormatoDocumento", MySqlDbType.VarChar, 255).Value = request.FormatoDocumento;
                cmd.Parameters.Add("@p_bigTamanoArchivo", MySqlDbType.Int64).Value = request.TamanoArchivo;
                cmd.Parameters.Add("@p_intIdTipoArchivo", MySqlDbType.Int32).Value = request.IdTipoArchivo;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedidoArchivo", MySqlDbType.Int32).Value = request.IdPedidoArchivo;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = request.IdPedido;
                cmd.Parameters.Add("@p_vchDocumentoURL", MySqlDbType.VarChar).Value = request.DocumentoURL;
                cmd.Parameters.Add("@p_vchNombreDocumento", MySqlDbType.VarChar, 255).Value = request.NombreDocumento;
                cmd.Parameters.Add("@p_vchFormatoDocumento", MySqlDbType.VarChar, 255).Value = request.FormatoDocumento;
                cmd.Parameters.Add("@p_bigTamanoArchivo", MySqlDbType.Int64).Value = request.TamanoArchivo;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = request.IdEstado;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedidoArchivo", MySqlDbType.Int32).Value = request.IdPedidoArchivo;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = request.IdPedido;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = request.idPedido;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)request.numPag ?? DBNull.Value;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedidoArchivo", MySqlDbType.Int32).Value = request.IdPedidoArchivo;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = request.IdPedido;

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
