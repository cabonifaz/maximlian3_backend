using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class InformeLocalImagenDAO
    {
        private readonly DbConfig _dbConfig;

        public InformeLocalImagenDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<Respuesta> ObtenerUrlsImagenesAsync(UsuarioGeneral u, List<int> ids)
        {
            try
            {
                var t = new DataTable();
                t.Columns.Add("IdInformeLocalImagen", typeof(int));
                foreach (var id in ids)
                    t.Rows.Add(id);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeLocalImagen_ObtenerUrls", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarTvp(cmd, "@lstIds", t, "LISTA_INFORME_LOCAL_IMAGEN_ID");
                await cn.OpenAsync();
                return await LeerRespuestaAsync<InformeLocalImagenUrl>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeLocalImagenUrl>() };
            }
        }

        public async Task<Respuesta> ActualizarEstadoCargaAsync(UsuarioGeneral u, List<int> ids)
        {
            try
            {
                var t = new DataTable();
                t.Columns.Add("IdInformeLocalImagen", typeof(int));
                foreach (var id in ids)
                    t.Rows.Add(id);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InformeLocalImagen_ActualizarEstadoCarga", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarTvp(cmd, "@lstIds", t, "LISTA_INFORME_LOCAL_IMAGEN_ID");
                await cn.OpenAsync();
                return await LeerRespuestaAsync<object>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task ActualizarImagenUrlAsync(UsuarioGeneral u, int idInformeLocalImagen, string imagenUrl)
        {
            using SqlConnection cn = new(_dbConfig.ConnectionString);
            using SqlCommand cmd = new("InformeLocalImagen_ActualizarUrl", cn) { CommandType = CommandType.StoredProcedure };
            AgregarParametrosAuditoria(cmd, u);
            cmd.Parameters.Add("@intIdInformeLocalImagen", SqlDbType.Int).Value = idInformeLocalImagen;
            cmd.Parameters.Add("@vchImagenURL", SqlDbType.VarChar, 2048).Value = imagenUrl;
            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();
            using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 0;
                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                var json = dr["Result"]?.ToString();
                respuesta.Result = !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>()
                    : new List<T>();
            }
            else
            {
                respuesta.IdTipoMensaje = 1;
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
