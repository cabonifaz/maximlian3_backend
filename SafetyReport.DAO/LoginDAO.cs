using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class LoginDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<LoginDAO> _logger;

        public LoginDAO(DbConfig dbConfig, ILogger<LoginDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
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
                        : 3;

                    respuesta.Mensaje = dr["Mensaje"]?.ToString();

                    var resultJson = dr["Result"]?.ToString();

                    respuesta.Result = !string.IsNullOrWhiteSpace(resultJson)
                        ? JsonSerializer.Deserialize<List<UsuarioLoginResponse>>(resultJson) ?? new List<UsuarioLoginResponse>()
                        : new List<UsuarioLoginResponse>();
                }
                else
                {
                    _logger.LogWarning("Usuario_AUTH no devolvio ninguna fila para {Usuario}.", usuarioActual.Usuario);

                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new List<UsuarioLoginResponse>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado autenticando al usuario {Usuario}.", usuarioActual.Usuario);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = $"Error al autenticar: {ex.Message}";
                respuesta.Result = new List<UsuarioLoginResponse>();
            }

            return respuesta;
        }
    }
}