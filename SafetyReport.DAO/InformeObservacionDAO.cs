using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class InformeObservacionDAO
    {
        private readonly DbConfig _dbConfig;

        public InformeObservacionDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<Respuesta> ListarObservacionesAsync(UsuarioGeneral u, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeObservacion_Listar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<InformeObservacionConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeObservacionConsulta>() };
            }
        }

        public async Task<Respuesta> InsertarObservacionesLoteAsync(UsuarioGeneral u, int idInforme, int idPedido, List<InformeObservacionItem> observaciones)
        {
            try
            {
                var t = new DataTable();
                t.Columns.Add("Observacion", typeof(string));
                t.Columns.Add("Checked", typeof(bool));
                foreach (var o in observaciones)
                    t.Rows.Add((object?)o.Observacion ?? DBNull.Value, o.Checked);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeObservacion_InsertarLote", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = idInforme;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                AgregarTvp(cmd, "@lstObservaciones", t, "LISTA_INFORME_OBSERVACION");
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EditarObservacionAsync(UsuarioGeneral u, InformeObservacionEditarRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeObservacion_Editar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeObservacion", SqlDbType.Int).Value = request.IdInformeObservacion;
                cmd.Parameters.Add("@vchObservacion", SqlDbType.VarChar, 500).Value = (object?)request.Observacion ?? DBNull.Value;
                cmd.Parameters.Add("@bitChecked", SqlDbType.Bit).Value = request.Checked;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EliminarObservacionAsync(UsuarioGeneral u, int idInformeObservacion)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeObservacion_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@intIdInformeObservacion", SqlDbType.Int).Value = idInformeObservacion;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
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
