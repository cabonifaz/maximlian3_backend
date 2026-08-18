using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class InformeLocalImagenDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeLocalImagenDAO> _logger;

        public InformeLocalImagenDAO(DbConfig dbConfig, ILogger<InformeLocalImagenDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static string? GetNullableString(DbDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private async Task<Respuesta> LeerCabeceraAsync(DbDataReader dr, string commandText)
        {
            var respuesta = new Respuesta();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 3;

                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
            }
            else
            {
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", commandText);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
            }

            return respuesta;
        }

        public async Task<Respuesta> ObtenerUrlsImagenesAsync(UsuarioGeneral u, List<int> ids)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeLocalImagen_ObtenerUrls", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@jsonIds", JsonSerializer.Serialize(ids));
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<InformeLocalImagenUrl>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(new InformeLocalImagenUrl
                        {
                            IdInformeLocalImagen = Convert.ToInt32(dr["IdInformeLocalImagen"]),
                            ImagenURL = GetNullableString(dr, "ImagenURL") ?? string.Empty,
                            Nombre = GetNullableString(dr, "Nombre") ?? string.Empty
                        });
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<InformeLocalImagenUrl>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeLocalImagenUrl>() };
            }
        }

        public async Task<Respuesta> ActualizarEstadoCargaAsync(UsuarioGeneral u, List<int> ids)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeLocalImagen_ActualizarEstadoCarga", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@lstIds", JsonSerializer.Serialize(ids));
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);
                respuesta.Result = new List<object>();

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task ActualizarImagenUrlAsync(UsuarioGeneral u, int idInformeLocalImagen, string imagenUrl)
        {
            using MySqlConnection cn = new(_dbConfig.ConnectionString);
            using MySqlCommand cmd = new("SP_InformeLocalImagen_ActualizarUrl", cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
            cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
            cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
            cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
            cmd.Parameters.AddWithValue("@intIdInformeLocalImagen", idInformeLocalImagen);
            cmd.Parameters.AddWithValue("@vchImagenURL", imagenUrl);
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

            if (respuesta.IdTipoMensaje != 2)
            {
                throw new Exception(respuesta.Mensaje);
            }
        }

    }
}
