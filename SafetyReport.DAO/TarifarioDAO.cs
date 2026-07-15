using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;

namespace SafetyReport.DAO
{
    public class TarifarioDAO
    {
        private readonly DbConfig _dbConfig;

        public TarifarioDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static int? GetNullableInt(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static string? GetNullableString(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private static async Task<Respuesta> LeerCabeceraAsync(SqlDataReader dr)
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
                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
            }

            return respuesta;
        }

        private static async Task<List<T>> LeerIdsAsync<T>(SqlDataReader dr, string columnName, Func<int?, T> factory)
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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("SP_Tarifario_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@intIdProducto", SqlDbType.Int).Value = request.IdProducto;
                cmd.Parameters.Add("@intIdTipoTramite", SqlDbType.Int).Value = request.IdTipoTramite;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@intIdMoneda", SqlDbType.Int).Value = request.IdMoneda;
                cmd.Parameters.Add("@intDiasMax", SqlDbType.Int).Value = request.DiasMax;
                cmd.Parameters.Add("@intDiasMin", SqlDbType.Int).Value = request.DiasMin;

                cmd.Parameters.Add("@decPrecio", SqlDbType.Decimal).Value = request.Precio;
                cmd.Parameters["@decPrecio"].Precision = 18;
                cmd.Parameters["@decPrecio"].Scale = 2;

                cmd.Parameters.Add("@decPenalidad", SqlDbType.Decimal).Value = request.Penalidad;
                cmd.Parameters["@decPenalidad"].Precision = 18;
                cmd.Parameters["@decPenalidad"].Scale = 2;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr);

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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("SP_Tarifario_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr);

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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("SP_Tarifario_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.idTarifario;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr);

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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("SP_Tarifario_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.IdTarifario;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@intIdProducto", SqlDbType.Int).Value = request.IdProducto;
                cmd.Parameters.Add("@intIdTipoTramite", SqlDbType.Int).Value = request.IdTipoTramite;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@intIdMoneda", SqlDbType.Int).Value = request.IdMoneda;
                cmd.Parameters.Add("@intDiasMax", SqlDbType.Int).Value = request.DiasMax;
                cmd.Parameters.Add("@intDiasMin", SqlDbType.Int).Value = request.DiasMin;

                cmd.Parameters.Add("@decPrecio", SqlDbType.Decimal).Value = request.Precio;
                cmd.Parameters["@decPrecio"].Precision = 18;
                cmd.Parameters["@decPrecio"].Scale = 2;

                cmd.Parameters.Add("@decPenalidad", SqlDbType.Decimal).Value = request.Penalidad;
                cmd.Parameters["@decPenalidad"].Precision = 18;
                cmd.Parameters["@decPenalidad"].Scale = 2;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr);

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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("SP_Tarifario_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.idTarifario;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr);

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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("SP_Tarifario_Listar_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;
                cmd.Parameters.Add("@intIdTipoProducto", SqlDbType.Int).Value = (object?)request.idTipoProducto ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoTramite", SqlDbType.Int).Value = (object?)request.idTipoTramite ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = (object?)request.idPais ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr);

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