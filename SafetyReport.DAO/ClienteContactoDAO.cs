using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class ClienteContactoDAO
    {
        private readonly DbConfig _dbConfig;

        public ClienteContactoDAO(DbConfig dbConfig)
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

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, ClienteContactoCrear request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("ClienteContacto_INS", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 32).Value = (object?)request.Codigo ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 255).Value = request.Nombres;
                cmd.Parameters.Add("@intIdTipoContacto", SqlDbType.Int).Value = request.IdTipoContacto;
                cmd.Parameters.Add("@intAreaTrabajo", SqlDbType.Int).Value = request.AreaTrabajo;
                cmd.Parameters.Add("@vchTelefono", SqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 100).Value = (object?)request.Email ?? DBNull.Value;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteContactoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoCreado>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoFiltro request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("ClienteContacto_LST", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.NumPag ?? DBNull.Value;

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
                        ? JsonSerializer.Deserialize<ClienteContactoListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new ClienteContactoListaResult()
                        : new ClienteContactoListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new ClienteContactoListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new ClienteContactoListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ClienteContactoIdRequest request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("ClienteContacto_SEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdClienteContacto", SqlDbType.Int).Value = request.IdClienteContacto;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteContactoSeleccionado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoSeleccionado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoEditar request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("ClienteContacto_UPD", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdClienteContacto", SqlDbType.Int).Value = request.IdClienteContacto;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 32).Value = (object?)request.Codigo ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 255).Value = request.Nombres;
                cmd.Parameters.Add("@intIdTipoContacto", SqlDbType.Int).Value = request.IdTipoContacto;
                cmd.Parameters.Add("@intAreaTrabajo", SqlDbType.Int).Value = request.AreaTrabajo;
                cmd.Parameters.Add("@vchTelefono", SqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 100).Value = (object?)request.Email ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteContactoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoIdRequest request)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("ClienteContacto_DEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdClienteContacto", SqlDbType.Int).Value = request.IdClienteContacto;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteContactoEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoEliminado>()
                };
            }
        }
    }
}