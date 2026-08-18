using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;

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

        private static string? GetNullableString(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        public async Task<Respuesta> AutenticarAsync(UsuarioGeneral usuarioActual)
        {
            var respuesta = new Respuesta();

            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_Usuario_Auth", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@vchSub", usuarioActual.Sub);

                await cn.OpenAsync();

                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 3;

                    respuesta.Mensaje = dr["Mensaje"]?.ToString();

                    var lista = new List<UsuarioLoginResponse>();

                    if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    {
                        var usuarioLogin = new UsuarioLoginResponse
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]),
                            Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                            Correo = dr["Correo"]?.ToString() ?? string.Empty,
                            Usuario = dr["Usuario"]?.ToString() ?? string.Empty
                        };

                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                usuarioLogin.Roles.Add(new Roles
                                {
                                    IdRol = Convert.ToInt32(dr["IdRol"]),
                                    Rol = GetNullableString(dr, "Rol"),
                                    Descripcion = GetNullableString(dr, "Descripcion")
                                });
                            }
                        }

                        lista.Add(usuarioLogin);
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    _logger.LogWarning("SP_Usuario_Auth no devolvio ninguna fila para {Usuario}.", usuarioActual.Usuario);

                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new List<UsuarioLoginResponse>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado autenticando al usuario {Usuario}.", usuarioActual.Usuario);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = ex.Message;
                respuesta.Result = new List<UsuarioLoginResponse>();
            }

            return respuesta;
        }
    }
}