using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeArchivo", SqlDbType.Int).Value = idInformeArchivo;
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeArchivo", SqlDbType.Int).Value = idInformeArchivo;
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeArchivo", SqlDbType.Int).Value = r.IdInformeArchivo;
                cmd.Parameters.Add("@intIdTipoArchivo",   SqlDbType.Int).Value = (object?)r.IdTipoArchivo   ?? DBNull.Value;
                cmd.Parameters.Add("@intIdFaseEvidencia", SqlDbType.Int).Value = (object?)r.IdFaseEvidencia ?? DBNull.Value;
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
                var t = new DataTable();
                t.Columns.Add("Nombre", typeof(string));
                t.Columns.Add("ArchivoUrl", typeof(string));
                t.Columns.Add("Extension", typeof(string));
                t.Columns.Add("TamanoBytes", typeof(long));
                t.Columns.Add("IdTipoArchivo", typeof(int));
                t.Columns.Add("IdFaseEvidencia", typeof(int));
                foreach (var a in archivos)
                    t.Rows.Add(a.Nombre, a.ArchivoUrl, a.Extension, a.TamanoBytes, a.IdTipoArchivo, (object?)a.IdFaseEvidencia ?? DBNull.Value);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeArchivo_InsertarLote", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdPedido",  SqlDbType.Int).Value = idPedido;
                AgregarTvp(cmd, "@lstArchivos", t, "LISTA_INFORME_ARCHIVO");
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        private async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
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

        private static void AgregarTvp(SqlCommand cmd, string paramName, DataTable table, string typeName)
        {
            var p = cmd.Parameters.AddWithValue(paramName, table);
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = typeName;
        }

        private static void AgregarParametrosAuditoria(SqlCommand cmd, UsuarioGeneral u)
        {
            cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = u.IdUsuario;
            cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = u.Usuario;
            cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = u.IdEmpresa;
            cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = u.IdRol;
        }
    }
}
