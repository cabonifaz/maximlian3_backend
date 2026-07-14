using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class AsignacionDAO
    {
        private readonly DbConfig _dbConfig;

        public AsignacionDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static DataTable ConstruirTablaListaGeneralNum(List<int>? valores)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("NUM1", typeof(int));

            int i = 1;
            if (valores != null)
            {
                foreach (var valor in valores)
                {
                    table.Rows.Add(i++, valor);
                }
            }

            return table;
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();

            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 3;

                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                var json = dr["Result"]?.ToString();

                respuesta.Result = !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<T>()
                    : new List<T>();
            }
            else
            {
                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                respuesta.Result = new List<T>();
            }

            return respuesta;
        }

        private static DataTable ConstruirTablaAsignados(List<AsignacionUsuario> asignados)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("IdUsuarioAsignado", typeof(int));
            table.Columns.Add("IdRolAsignado", typeof(int));
            table.Columns.Add("IdEstado", typeof(int));

            int i = 1;
            foreach (var a in asignados)
                table.Rows.Add(i++, a.IdUsuarioAsignado, a.IdRolAsignado, a.IdEstado);

            return table;
        }

        public async Task<Respuesta> InsertarAsync(UsuarioGeneral usuarioActual, AsignacionCrear request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Asignacion_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;

                var tableIdsPedido = ConstruirTablaListaGeneralNum(request.IdsPedido);
                var tvpIdsPedido = cmd.Parameters.AddWithValue("@lstIdsPedido", tableIdsPedido);
                tvpIdsPedido.SqlDbType = SqlDbType.Structured;
                tvpIdsPedido.TypeName = "LISTA_GENERAL_NUM";

                var tableAsignados = ConstruirTablaAsignados(request.Asignados);
                var tvpAsignados = cmd.Parameters.AddWithValue("@lstAsignados", tableAsignados);
                tvpAsignados.SqlDbType = SqlDbType.Structured;
                tvpAsignados.TypeName = "LISTA_ASIGNADOS";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<AsignacionCreada>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionCreada>()
                };
            }
        }

        public async Task<Respuesta> ActualizarAsync(UsuarioGeneral usuarioActual, AsignacionActualizar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Asignacion_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.IdPedido;
                var tableAsignados = ConstruirTablaAsignados(request.Asignados);
                var tvpAsignados = cmd.Parameters.AddWithValue("@lstAsignados", tableAsignados);
                tvpAsignados.SqlDbType = SqlDbType.Structured;
                tvpAsignados.TypeName = "LISTA_ASIGNADOS";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<AsignacionCreada>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionCreada>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioActual, FiltroAsignacion request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Asignacion_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 3;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<AsignacionListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new AsignacionListaResult()
                        : new AsignacionListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new AsignacionListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new AsignacionListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioActual, int idAsignacion)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Asignacion_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdAsignacion", SqlDbType.Int).Value = idAsignacion;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<AsignacionConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionConsulta>()
                };
            }
        }

        public async Task<Respuesta> BandejaAsync(UsuarioGeneral usuarioActual, FiltroAsignacionBandeja filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Asignacion_Bandeja", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intNumPag", SqlDbType.Int).Value = filtro.NumPag;

                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"]) : 3;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<AsignacionBandejaResult>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                          ?? new AsignacionBandejaResult()
                        : new AsignacionBandejaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new AsignacionBandejaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new AsignacionBandejaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioActual, EliminarAsignacion request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Asignacion_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdAsignacion", SqlDbType.Int).Value = request.IdAsignacion;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<EliminarAsignacionResult>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<EliminarAsignacionResult>()
                };
            }
        }
    }
}
