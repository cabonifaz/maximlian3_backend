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

        public async Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Usuario_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 50).Value = request.Nombres;
                cmd.Parameters.Add("@vchApellidoPaterno", SqlDbType.VarChar, 50).Value = request.ApellidoPaterno;
                cmd.Parameters.Add("@vchApellidoMaterno", SqlDbType.VarChar, 50).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 100).Value = request.Email;
                cmd.Parameters.Add("@vchUsuarioCreado", SqlDbType.VarChar, 32).Value = request.usuarioCreacion;

                var tableRoles = ConstruirTablaListaGeneralNum(request.Roles);
                var tvpRoles = cmd.Parameters.AddWithValue("@lstRoles", tableRoles);
                tvpRoles.SqlDbType = SqlDbType.Structured;
                tvpRoles.TypeName = "LISTA_GENERAL_NUM";

                var tableIdiomas = ConstruirTablaListaGeneralNum(request.Idiomas);
                var tvpIdiomas = cmd.Parameters.AddWithValue("@lstIdiomas", tableIdiomas);
                tvpIdiomas.SqlDbType = SqlDbType.Structured;
                tvpIdiomas.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<UsuarioCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EditarUsuarioAsync(UsuarioGeneral usuarioLogueado, InfoUsuarioEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Usuario_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdUsuarioEditar", SqlDbType.Int).Value = request.IdUsuario;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 50).Value = request.Nombres;
                cmd.Parameters.Add("@vchApellidoPaterno", SqlDbType.VarChar, 50).Value = request.ApellidoPaterno;
                cmd.Parameters.Add("@vchApellidoMaterno", SqlDbType.VarChar, 50).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 100).Value = request.Email;

                var tableRoles = ConstruirTablaListaGeneralNum(request.Roles);
                var tvpRoles = cmd.Parameters.AddWithValue("@lstRoles", tableRoles);
                tvpRoles.SqlDbType = SqlDbType.Structured;
                tvpRoles.TypeName = "LISTA_GENERAL_NUM";

                var tableIdiomas = ConstruirTablaListaGeneralNum(request.Idiomas);
                var tvpIdiomas = cmd.Parameters.AddWithValue("@lstIdiomas", tableIdiomas);
                tvpIdiomas.SqlDbType = SqlDbType.Structured;
                tvpIdiomas.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<UsuarioCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioEliminar)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Usuario_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdUsuarioEliminar", SqlDbType.Int).Value = idUsuarioEliminar;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<EliminarUsuarioResult>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<EliminarUsuarioResult>()
                };
            }
        }

        public async Task<Respuesta> ListarUsuariosAsync(UsuarioGeneral usuarioActual, string? filtro, int? numPag)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Usuario_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@vchFiltro", SqlDbType.VarChar, 255).Value = (object?)filtro ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)numPag ?? DBNull.Value;

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
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<UsuarioListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new UsuarioListaResult()
                        : new UsuarioListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new UsuarioListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new UsuarioListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioConsulta)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Usuario_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
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
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarCortaAsync(UsuarioGeneral usuarioActual, int idRolFiltro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("UsuarioListar_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdRolFiltro", SqlDbType.Int).Value = idRolFiltro;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<UsuarioListaCortaItem>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioListaCortaItem>()
                };
            }
        }

        public async Task<Respuesta> ActualizarSubAsync(int idUsuario, string sub)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Usuario_Actualizar_COGNITO", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@vchSub", SqlDbType.VarChar, 255).Value = sub;

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
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = null
                };
            }
        }
    }
}