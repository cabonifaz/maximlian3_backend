using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class LoginDAO
    {
        private readonly DbConfig _dbConfig;

        public LoginDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<Respuesta> AutenticarAsync(UsuarioGeneral usuarioActual)
        {
            var respuesta = new Respuesta();

            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_AUTH", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@vchSub", SqlDbType.VarChar, 255).Value = usuarioActual.Sub;

                await cn.OpenAsync();

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 0;

                    respuesta.Mensaje = dr["Mensaje"]?.ToString();

                    var resultJson = dr["Result"]?.ToString();

                    respuesta.Result = !string.IsNullOrWhiteSpace(resultJson)
                        ? JsonSerializer.Deserialize<List<UsuarioLoginResponse>>(resultJson) ?? new List<UsuarioLoginResponse>()
                        : new List<UsuarioLoginResponse>();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new List<UsuarioLoginResponse>();
                }
            }
            catch (Exception ex)
            {
                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = $"Error al autenticar: {ex.Message}";
                respuesta.Result = new List<UsuarioLoginResponse>();
            }

            return respuesta;
        }
    }
}