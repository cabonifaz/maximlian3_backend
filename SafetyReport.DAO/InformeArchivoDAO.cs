using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class InformeArchivoDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeArchivoDAO> _logger;

        public InformeArchivoDAO(DbConfig dbConfig, ILogger<InformeArchivoDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        public async Task<Respuesta> ObtenerArchivoAsync(UsuarioGeneral u, int idInformeArchivo)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeArchivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInformeArchivo", idInformeArchivo);
                await cn.OpenAsync();
                return await LeerRespuestaAsync<InformeArchivoConsulta>(cmd);
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
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInformeArchivo", idInformeArchivo);
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
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
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInformeArchivo", r.IdInformeArchivo);
                cmd.Parameters.AddWithValue("@intIdTipoArchivo", (object?)r.IdTipoArchivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdFaseEvidencia", (object?)r.IdFaseEvidencia ?? DBNull.Value);
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
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
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
                cmd.Parameters.AddWithValue("@lstArchivos", JsonSerializer.Serialize(archivos));
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        private async Task<Respuesta> LeerRespuestaAsync<T>(DbCommand cmd)
        {
            var respuesta = new Respuesta();
            using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 3;
                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                var json = dr["Result"]?.ToString();
                respuesta.Result = !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>()
                    : new List<T>();
            }
            else
            {
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", cmd.CommandText);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                respuesta.Result = new List<T>();
            }
            return respuesta;
        }
    }
}
