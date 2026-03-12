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

        public async Task<Respuesta> AutenticarAsync(CognitoLoginRequest request)
        {
            var respuesta = new Respuesta();
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Usuario_AUTH", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = int.Parse(request.IdUsuario);
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = request.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = int.Parse(request.IdEmpresa);

                await cn.OpenAsync();

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 0;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString();

                    var resultJson = dr["Result"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(resultJson))
                    {
                        respuesta.Result = JsonSerializer.Deserialize<List<UsuarioLoginResponse>>(resultJson)
                                           ?? new List<UsuarioLoginResponse>();
                    }
                    else
                    {
                        respuesta.Result = new List<UsuarioLoginResponse>();
                    }
                }
            } catch (Exception ex)
            {
                respuesta.IdTipoMensaje = 1; // Error
                respuesta.Mensaje = $"Error al autenticar: {ex.Message}";
                respuesta.Result = null;
            }


            return respuesta;
        }
    }

    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}