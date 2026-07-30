using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

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

        private static string? GetNullableString(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private async Task<Respuesta> LeerCabeceraAsync(SqlDataReader dr, string commandText)
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
                var t = new DataTable();
                t.Columns.Add("IdInformeLocalImagen", typeof(int));
                foreach (var id in ids)
                    t.Rows.Add(id);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_InformeLocalImagen_ObtenerUrls", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarTvp(cmd, "@lstIds", t, "LISTA_INFORME_LOCAL_IMAGEN_ID");
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
                var t = new DataTable();
                t.Columns.Add("IdInformeLocalImagen", typeof(int));
                foreach (var id in ids)
                    t.Rows.Add(id);

                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_InformeLocalImagen_ActualizarEstadoCarga", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                AgregarTvp(cmd, "@lstIds", t, "LISTA_INFORME_LOCAL_IMAGEN_ID");
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
            using SqlConnection cn = new(_dbConfig.ConnectionString);
            using SqlCommand cmd = new("SP_InformeLocalImagen_ActualizarUrl", cn) { CommandType = CommandType.StoredProcedure };
            AgregarParametrosAuditoria(cmd, u);
            cmd.Parameters.Add("@intIdInformeLocalImagen", SqlDbType.Int).Value = idInformeLocalImagen;
            cmd.Parameters.Add("@vchImagenURL", SqlDbType.VarChar, 2048).Value = imagenUrl;
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

            if (respuesta.IdTipoMensaje != 2)
            {
                throw new Exception(respuesta.Mensaje);
            }
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
