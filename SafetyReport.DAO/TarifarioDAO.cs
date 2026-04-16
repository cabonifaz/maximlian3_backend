using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class TarifarioDAO
    {
        private readonly DbConfig _dbConfig;

        public TarifarioDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();

            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 0;

                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                var json = dr["Result"]?.ToString();

                respuesta.Result = respuesta.IdTipoMensaje == 2 && !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<T>()
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

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TarifarioCrear request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Tarifario_Insertar", cn);

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
                return await LeerRespuestaAsync<TarifarioCreado>(cmd);
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
                using SqlCommand cmd = new SqlCommand("Tarifario_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 0;

                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = respuesta.IdTipoMensaje == 2 && !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<TarifarioListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new TarifarioListaResult()
                        : new TarifarioListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
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
                using SqlCommand cmd = new SqlCommand("Tarifario_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.idTarifario;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TarifarioConsulta>(cmd);
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
                using SqlCommand cmd = new SqlCommand("Tarifario_Actualizar", cn);

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
                return await LeerRespuestaAsync<TarifarioCreado>(cmd);
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
                using SqlCommand cmd = new SqlCommand("Tarifario_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.idTarifario;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.idCliente;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TarifarioEliminado>(cmd);
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
                using SqlCommand cmd = new SqlCommand("Tarifario_Listar_Corta", cn);

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

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 0;

                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = respuesta.IdTipoMensaje == 2 && !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<TarifarioListaCortaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new TarifarioListaCortaResult()
                        : new TarifarioListaCortaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
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
    }
}