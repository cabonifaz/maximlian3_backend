using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Tarifario;
using System.Data;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class TarifarioRepositorioSql : ITarifarioRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<TarifarioRepositorioSql> _logger;

        public TarifarioRepositorioSql(DbConfig dbConfig, ILogger<TarifarioRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static int? GetNullableInt(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static string? GetNullableString(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

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

        private static async Task<List<T>> LeerIdsAsync<T>(MySqlDataReader dr, string columnName, Func<int?, T> factory)
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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_intIdProducto", MySqlDbType.Int32).Value = request.IdProducto;
                cmd.Parameters.Add("@p_intIdTipoTramite", MySqlDbType.Int32).Value = request.IdTipoTramite;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = request.IdPais;
                cmd.Parameters.Add("@p_intIdMoneda", MySqlDbType.Int32).Value = request.IdMoneda;
                cmd.Parameters.Add("@p_intDiasMax", MySqlDbType.Int32).Value = request.DiasMax;
                cmd.Parameters.Add("@p_intDiasMin", MySqlDbType.Int32).Value = request.DiasMin;

                cmd.Parameters.Add("@p_decPrecio", MySqlDbType.Decimal).Value = request.Precio;
                cmd.Parameters["@p_decPrecio"].Precision = 18;
                cmd.Parameters["@p_decPrecio"].Scale = 2;

                cmd.Parameters.Add("@p_decPenalidad", MySqlDbType.Decimal).Value = request.Penalidad;
                cmd.Parameters["@p_decPenalidad"].Precision = 18;
                cmd.Parameters["@p_decPenalidad"].Scale = 2;

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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)request.numPag ?? DBNull.Value;

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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdTarifario", MySqlDbType.Int32).Value = request.idTarifario;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;

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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdTarifario", MySqlDbType.Int32).Value = request.IdTarifario;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_intIdProducto", MySqlDbType.Int32).Value = request.IdProducto;
                cmd.Parameters.Add("@p_intIdTipoTramite", MySqlDbType.Int32).Value = request.IdTipoTramite;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = request.IdPais;
                cmd.Parameters.Add("@p_intIdMoneda", MySqlDbType.Int32).Value = request.IdMoneda;
                cmd.Parameters.Add("@p_intDiasMax", MySqlDbType.Int32).Value = request.DiasMax;
                cmd.Parameters.Add("@p_intDiasMin", MySqlDbType.Int32).Value = request.DiasMin;

                cmd.Parameters.Add("@p_decPrecio", MySqlDbType.Decimal).Value = request.Precio;
                cmd.Parameters["@p_decPrecio"].Precision = 18;
                cmd.Parameters["@p_decPrecio"].Scale = 2;

                cmd.Parameters.Add("@p_decPenalidad", MySqlDbType.Decimal).Value = request.Penalidad;
                cmd.Parameters["@p_decPenalidad"].Precision = 18;
                cmd.Parameters["@p_decPenalidad"].Scale = 2;

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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdTarifario", MySqlDbType.Int32).Value = request.idTarifario;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;

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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;
                cmd.Parameters.Add("@p_intIdTipoProducto", MySqlDbType.Int32).Value = (object?)request.idTipoProducto ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTipoTramite", MySqlDbType.Int32).Value = (object?)request.idTipoTramite ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = (object?)request.idPais ?? DBNull.Value;

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
