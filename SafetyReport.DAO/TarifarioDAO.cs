using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;

namespace SafetyReport.DAO
{
    public class TarifarioDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<TarifarioDAO> _logger;

        public TarifarioDAO(DbConfig dbConfig, ILogger<TarifarioDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static int? GetNullableInt(DbDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static string? GetNullableString(DbDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
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

        private static async Task<List<T>> LeerIdsAsync<T>(DbDataReader dr, string columnName, Func<int?, T> factory)
        {
            var lista = new List<T>();

            while (await dr.ReadAsync())
            {
                lista.Add(factory(GetNullableInt(dr, columnName)));
            }

            return lista;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TarifarioCrear request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_Tarifario_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", request.IdCliente);
                cmd.Parameters.AddWithValue("@intIdProducto", request.IdProducto);
                cmd.Parameters.AddWithValue("@intIdTipoTramite", request.IdTipoTramite);
                cmd.Parameters.AddWithValue("@intIdPais", request.IdPais);
                cmd.Parameters.AddWithValue("@intIdMoneda", request.IdMoneda);
                cmd.Parameters.AddWithValue("@intDiasMax", request.DiasMax);
                cmd.Parameters.AddWithValue("@intDiasMin", request.DiasMin);

                var decPrecio = cmd.Parameters.AddWithValue("@decPrecio", request.Precio);
                decPrecio.Precision = 18;
                decPrecio.Scale = 2;

                var decPenalidad = cmd.Parameters.AddWithValue("@decPenalidad", request.Penalidad);
                decPenalidad.Precision = 18;
                decPenalidad.Scale = 2;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdTarifario", id => new TarifarioCreado { IdTarifario = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<TarifarioCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioCreado>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, TarifarioFiltro request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_Tarifario_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", request.idCliente);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)request.busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)request.numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new TarifarioListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstTarifario.Add(new TarifarioListaConsulta
                            {
                                IdTarifario = Convert.ToInt32(dr["IdTarifario"]),
                                Producto = dr["Producto"]?.ToString() ?? string.Empty,
                                Pais = dr["Pais"]?.ToString() ?? string.Empty,
                                Moneda = dr["Moneda"]?.ToString() ?? string.Empty,
                                TipoTramite = dr["TipoTramite"]?.ToString() ?? string.Empty,
                                DiasMinMax = dr["DiasMinMax"]?.ToString() ?? string.Empty,
                                Precio = Convert.ToDecimal(dr["Precio"]),
                                Penalidad = Convert.ToDecimal(dr["Penalidad"])
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new TarifarioListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new TarifarioListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_Tarifario_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdTarifario", request.idTarifario);
                cmd.Parameters.AddWithValue("@intIdCliente", request.idCliente);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<TarifarioConsulta>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(new TarifarioConsulta
                        {
                            IdTarifario = Convert.ToInt32(dr["IdTarifario"]),
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            IdProducto = Convert.ToInt32(dr["IdProducto"]),
                            IdTipoTramite = Convert.ToInt32(dr["IdTipoTramite"]),
                            IdPais = Convert.ToInt32(dr["IdPais"]),
                            IdMoneda = Convert.ToInt32(dr["IdMoneda"]),
                            DiasMax = Convert.ToInt32(dr["DiasMax"]),
                            DiasMin = Convert.ToInt32(dr["DiasMin"]),
                            Precio = Convert.ToDecimal(dr["Precio"]),
                            Penalidad = Convert.ToDecimal(dr["Penalidad"])
                        });
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<TarifarioConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioConsulta>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, TarifarioEditar request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_Tarifario_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdTarifario", request.IdTarifario);
                cmd.Parameters.AddWithValue("@intIdCliente", request.IdCliente);
                cmd.Parameters.AddWithValue("@intIdProducto", request.IdProducto);
                cmd.Parameters.AddWithValue("@intIdTipoTramite", request.IdTipoTramite);
                cmd.Parameters.AddWithValue("@intIdPais", request.IdPais);
                cmd.Parameters.AddWithValue("@intIdMoneda", request.IdMoneda);
                cmd.Parameters.AddWithValue("@intDiasMax", request.DiasMax);
                cmd.Parameters.AddWithValue("@intDiasMin", request.DiasMin);

                var decPrecio = cmd.Parameters.AddWithValue("@decPrecio", request.Precio);
                decPrecio.Precision = 18;
                decPrecio.Scale = 2;

                var decPenalidad = cmd.Parameters.AddWithValue("@decPenalidad", request.Penalidad);
                decPenalidad.Precision = 18;
                decPenalidad.Scale = 2;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdTarifario", id => new TarifarioCreado { IdTarifario = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<TarifarioCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_Tarifario_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdTarifario", request.idTarifario);
                cmd.Parameters.AddWithValue("@intIdCliente", request.idCliente);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdTarifario", id => new TarifarioEliminado { IdTarifario = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<TarifarioEliminado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioEliminado>()
                };
            }
        }

        public async Task<Respuesta> ListaCortaAsync(UsuarioGeneral usuarioLogueado, TarifarioListaCortaFiltro request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_Tarifario_ListaCorta", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", request.idCliente);
                cmd.Parameters.AddWithValue("@intIdTipoProducto", (object?)request.idTipoProducto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTipoTramite", (object?)request.idTipoTramite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPais", (object?)request.idPais ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new TarifarioListaCortaResult();

                    // primer result set posterior a la cabecera: TotalRegistros/TotalPaginas (no usados por este modelo)
                    await dr.ReadAsync();

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstTarifario.Add(new TarifarioListaCorta
                            {
                                IdTarifario = Convert.ToInt32(dr["IdTarifario"]),
                                TipoTramite = dr["TipoTramite"]?.ToString() ?? string.Empty,
                                IdMoneda = Convert.ToInt32(dr["IdMoneda"]),
                                SimboloMoneda = GetNullableString(dr, "SimboloMoneda") ?? string.Empty,
                                Moneda = dr["Moneda"]?.ToString() ?? string.Empty,
                                Precio = Convert.ToDecimal(dr["Precio"]),
                                IdPais = Convert.ToInt32(dr["IdPais"]),
                                Pais = dr["Pais"]?.ToString() ?? string.Empty,
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                IdTipoTramite = Convert.ToInt32(dr["IdTipoTramite"]),
                                DiasMin = Convert.ToInt32(dr["DiasMin"]),
                                DiasMax = Convert.ToInt32(dr["DiasMax"])
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new TarifarioListaCortaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new TarifarioListaCortaResult()
                };
            }
        }
    }
}