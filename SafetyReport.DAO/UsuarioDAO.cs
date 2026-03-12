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

        public async Task<Respuesta> CrearUsuarioAsync(CrearUsuario request)
        {
            var respuesta = new Respuesta();

            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_INS", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = request.UsuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = request.UsuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = request.UsuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = request.UsuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 50).Value = request.Nombres;
                cmd.Parameters.Add("@vchApellidoPaterno", SqlDbType.VarChar, 50).Value = request.ApellidoPaterno;
                cmd.Parameters.Add("@vchApellidoMaterno", SqlDbType.VarChar, 50).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 100).Value = request.Email;

                var table = new DataTable();
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("NUM1", typeof(int));

                int i = 1;
                if (request.Roles != null)
                {
                    foreach (var rol in request.Roles)
                    {
                        table.Rows.Add(i++, rol);
                    }
                }

                var tvp = cmd.Parameters.AddWithValue("@lstRoles", table);
                tvp.SqlDbType = SqlDbType.Structured;
                tvp.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 0;

                    respuesta.Mensaje = dr["Mensaje"]?.ToString();

                    var json = dr["Result"]?.ToString();

                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<List<UsuarioCreado>>(json) ?? new List<UsuarioCreado>()
                        : new List<UsuarioCreado>();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new List<UsuarioCreado>();
                }
            }
            catch (Exception ex)
            {
                respuesta.IdTipoMensaje = 1;
                respuesta.Mensaje = ex.Message;
                respuesta.Result = new List<UsuarioCreado>();
            }

            return respuesta;
        }

        public async Task<Respuesta> ActualizarSubAsync(int idUsuario, string sub)
        {
            var respuesta = new Respuesta();

            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_UPD_COGNITO", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@Sub", SqlDbType.VarChar, 255).Value = sub;

                await cn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                respuesta.IdTipoMensaje = 2;
                respuesta.Mensaje = "Sub actualizado correctamente.";
                respuesta.Result = null;
            }
            catch (Exception ex)
            {
                respuesta.IdTipoMensaje = 1;
                respuesta.Mensaje = ex.Message;
                respuesta.Result = null;
            }

            return respuesta;
        }
    }
}