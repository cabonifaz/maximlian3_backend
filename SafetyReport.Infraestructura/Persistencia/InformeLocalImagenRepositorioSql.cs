using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.InformeLocalImagen;
using SafetyReport.Application.Puertos.Informe;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class InformeLocalImagenRepositorioSql : IInformeLocalImagenRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeLocalImagenRepositorioSql> _logger;

        public InformeLocalImagenRepositorioSql(DbConfig dbConfig, ILogger<InformeLocalImagenRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static string? GetNullableString(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private async Task<Respuesta> LeerCabeceraAsync(MySqlDataReader dr, string commandText)
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
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_jsonIds", MySqlDbType.JSON).Value = ConstruirJsonIds(ids);
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
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_jsonIds", MySqlDbType.JSON).Value = ConstruirJsonIds(ids);
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
            AgregarParametrosAuditoria(cmd, u);
            cmd.Parameters.Add("@p_intIdInformeLocalImagen", MySqlDbType.Int32).Value = idInformeLocalImagen;
            cmd.Parameters.Add("@p_vchImagenURL", MySqlDbType.VarChar, 2048).Value = imagenUrl;
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

            if (respuesta.IdTipoMensaje != 2)
            {
                throw new Exception(respuesta.Mensaje);
            }
        }

        // Shape esperado por SP_InformeLocalImagen_ObtenerUrls/_ActualizarEstadoCarga (p_jsonIds):
        // array de {IdInformeLocalImagen}.
        private static string ConstruirJsonIds(List<int> ids) =>
            JsonSerializer.Serialize(ids.Select(id => new { IdInformeLocalImagen = id }));

        private static void AgregarParametrosAuditoria(MySqlCommand cmd, UsuarioGeneral u)
        {
            cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = u.IdUsuario;
            cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = u.Usuario;
            cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = u.IdEmpresa;
            cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = u.IdRol;
        }
    }
}
