using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class N8nDAO
    {
        private readonly DbConfig _dbConfig;

        public N8nDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<Respuesta> ObtenerClienteAsync(UsuarioGeneral usuarioLogueado, string? emailBusqueda)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("N8N_Cliente_SEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@vchEmailBusqueda", SqlDbType.VarChar, 100).Value = (object?)emailBusqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

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
                        ? JsonSerializer.Deserialize<List<N8nClienteConsulta>>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<N8nClienteConsulta>()
                        : new List<N8nClienteConsulta>();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new List<N8nClienteConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<N8nClienteConsulta>()
                };
            }
        }
    }
}
