using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class UsuarioDAO
    {
        private readonly DbConfig _dbConfig;

        public UsuarioDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static DataTable ConstruirTablaRoles(List<int>? roles)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("NUM1", typeof(int));

            int i = 1;
            if (roles != null)
            {
                foreach (var rol in roles)
                {
                    table.Rows.Add(i++, rol);
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
                    : 0;

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
                respuesta.IdTipoMensaje = 1;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                respuesta.Result = new List<T>();
            }

            return respuesta;
        }

        public async Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, Usuario request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_INS", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 50).Value = request.Nombres;
                cmd.Parameters.Add("@vchApellidoPaterno", SqlDbType.VarChar, 50).Value = request.ApellidoPaterno;
                cmd.Parameters.Add("@vchApellidoMaterno", SqlDbType.VarChar, 50).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 100).Value = request.Email;

                var table = ConstruirTablaRoles(request.Roles);
                var tvp = cmd.Parameters.AddWithValue("@lstRoles", table);
                tvp.SqlDbType = SqlDbType.Structured;
                tvp.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<UsuarioCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EditarUsuarioAsync(UsuarioGeneral usuarioLogueado, EditarUsuario request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_UPD", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdUsuarioEditar", SqlDbType.Int).Value = request.IdUsuario;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 50).Value = request.InfoUsuario.Nombres;
                cmd.Parameters.Add("@vchApellidoPaterno", SqlDbType.VarChar, 50).Value = request.InfoUsuario.ApellidoPaterno;
                cmd.Parameters.Add("@vchApellidoMaterno", SqlDbType.VarChar, 50).Value = (object?)request.InfoUsuario.ApellidoMaterno ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 100).Value = request.InfoUsuario.Email;

                var table = ConstruirTablaRoles(request.InfoUsuario.Roles);
                var tvp = cmd.Parameters.AddWithValue("@lstRoles", table);
                tvp.SqlDbType = SqlDbType.Structured;
                tvp.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<UsuarioCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioEliminar)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_DEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioActual.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdUsuarioDel", SqlDbType.Int).Value = idUsuarioEliminar;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<EliminarUsuario>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<EliminarUsuario>()
                };
            }
        }

        public async Task<Respuesta> ListarUsuariosAsync(UsuarioGeneral usuarioActual, string? filtro)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_LST", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioActual.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@vchFiltro", SqlDbType.VarChar, 255).Value = (object?)filtro ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<UsuarioConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioConsulta>()
                };
            }
        }

        public async Task<Respuesta> ObtenerUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioConsulta)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_SEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioActual.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdUsuarioConsulta", SqlDbType.Int).Value = idUsuarioConsulta;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<UsuarioConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioConsulta>()
                };
            }
        }

        public async Task<Respuesta> ActualizarSubAsync(int idUsuario, string sub)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_UPD_COGNITO", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@Sub", SqlDbType.VarChar, 255).Value = sub;

                await cn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Sub actualizado correctamente.",
                    Result = null
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = null
                };
            }
        }
    }
}