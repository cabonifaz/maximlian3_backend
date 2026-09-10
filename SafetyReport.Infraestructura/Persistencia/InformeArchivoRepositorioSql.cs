using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.InformeArchivo;
using SafetyReport.Application.Puertos.Informe;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class InformeArchivoRepositorioSql : IInformeArchivoRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeArchivoRepositorioSql> _logger;

        public InformeArchivoRepositorioSql(DbConfig dbConfig, ILogger<InformeArchivoRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static string? GetNullableString(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje.
        private async Task<Respuesta> LeerCabeceraAsync(MySqlDataReader dr, string procedimiento)
        {
            var respuesta = new Respuesta();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 3;
                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
            }
            else
            {
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", procedimiento);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
            }

            return respuesta;
        }

        public async Task<Respuesta> ObtenerArchivoAsync(UsuarioGeneral u, int idInformeArchivo)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeArchivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_intIdInformeArchivo", MySqlDbType.Int32).Value = idInformeArchivo;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeArchivoConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    lista.Add(new InformeArchivoConsulta
                    {
                        IdInformeArchivo = Convert.ToInt32(dr["IdInformeArchivo"]),
                        IdInforme = Convert.ToInt32(dr["IdInforme"]),
                        Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                        ArchivoUrl = dr["ArchivoUrl"]?.ToString() ?? string.Empty,
                        Extension = dr["Extension"]?.ToString() ?? string.Empty,
                        TamanoBytes = Convert.ToInt64(dr["TamanoBytes"]),
                        IdTipoArchivo = Convert.ToInt32(dr["IdTipoArchivo"]),
                        IdFaseEvidencia = dr["IdFaseEvidencia"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdFaseEvidencia"])
                    });
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeArchivoConsulta>() };
            }
        }

        public async Task<Respuesta> EliminarArchivoAsync(UsuarioGeneral u, int idInformeArchivo)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeArchivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_intIdInformeArchivo", MySqlDbType.Int32).Value = idInformeArchivo;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                respuesta.Result = respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync()
                    ? new { IdInformeArchivo = Convert.ToInt32(dr["IdInformeArchivo"]) }
                    : null;

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ActualizarArchivoAsync(UsuarioGeneral u, InformeArchivoActualizarRequest r)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeArchivo_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_intIdInformeArchivo", MySqlDbType.Int32).Value = r.IdInformeArchivo;
                cmd.Parameters.Add("@p_intIdTipoArchivo",   MySqlDbType.Int32).Value = (object?)r.IdTipoArchivo   ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdFaseEvidencia", MySqlDbType.Int32).Value = (object?)r.IdFaseEvidencia ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                respuesta.Result = respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync()
                    ? new { IdInformeArchivo = Convert.ToInt32(dr["IdInformeArchivo"]) }
                    : null;

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> InsertarArchivoLoteAsync(UsuarioGeneral u, int idInforme, int idPedido, List<InformeArchivoItem> archivos)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeArchivo_InsertarLote", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_intIdInforme", MySqlDbType.Int32).Value = idInforme;
                cmd.Parameters.Add("@p_intIdPedido",  MySqlDbType.Int32).Value = idPedido;

                var json = JsonSerializer.Serialize(archivos.Select(a => new
                {
                    a.Nombre,
                    a.ArchivoUrl,
                    a.Extension,
                    a.TamanoBytes,
                    a.IdTipoArchivo,
                    a.IdFaseEvidencia
                }));
                cmd.Parameters.Add("@p_jsonArchivos", MySqlDbType.JSON).Value = json;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                respuesta.Result = respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync()
                    ? new { IdInforme = Convert.ToInt32(dr["IdInforme"]) }
                    : null;

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        private static void AgregarParametrosAuditoria(MySqlCommand cmd, UsuarioGeneral u)
        {
            cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = u.IdUsuario;
            cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = u.Usuario;
            cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = u.IdEmpresa;
            cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = u.IdRol;
        }
    }
}
