using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.InformeAprobacion;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class InformeAprobacionRepositorioSql : IInformeAprobacionRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeAprobacionRepositorioSql> _logger;

        public InformeAprobacionRepositorioSql(DbConfig dbConfig, ILogger<InformeAprobacionRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        public async Task<Respuesta> ListarPendientesAsync(UsuarioGeneral u, FiltroInformeAprobacionPendientes filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeAprobacion_ListarPendientes", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = (object?)filtro.IdPais ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPlantilla", MySqlDbType.Int32).Value = (object?)filtro.IdPlantilla ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdIdioma", MySqlDbType.Int32).Value = (object?)filtro.IdIdioma ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtmFchInicio", MySqlDbType.DateTime).Value = (object?)filtro.FchInicio?.ToUniversalTime() ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtmFchFin", MySqlDbType.DateTime).Value = (object?)filtro.FchFin?.ToUniversalTime() ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)filtro.NumPag ?? DBNull.Value;
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new InformeAprobacionPendientesListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstInformes.Add(new InformeAprobacionPendienteConsulta
                            {
                                IdInforme = Convert.ToInt32(dr["IdInforme"]),
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                Investigado = GetNullableString(dr, "Investigado"),
                                Pais = GetNullableString(dr, "Pais"),
                                Plantilla = GetNullableString(dr, "Plantilla"),
                                Idioma = GetNullableString(dr, "Idioma"),
                                Usuario = GetNullableString(dr, "Usuario"),
                                Fecha = GetNullableString(dr, "Fecha")
                            });
                        }
                    }
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new InformeAprobacionPendientesListaResult() };
            }
        }

        public async Task<Respuesta> AprobarAsync(UsuarioGeneral u, List<int> idInformes)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeAprobacion_Aprobar", cn) { CommandType = CommandType.StoredProcedure };
                AgregarParametrosAuditoria(cmd, u);
                cmd.Parameters.Add("@p_json_informes", MySqlDbType.JSON).Value = JsonSerializer.Serialize(idInformes ?? new List<int>());
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                return await LeerCabeceraAsync(dr, cmd.CommandText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje.
        private async Task<Respuesta> LeerCabeceraAsync(MySqlDataReader dr, string procedimiento)
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
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", procedimiento);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
            }

            return respuesta;
        }

        private static string? GetNullableString(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static void AgregarParametrosAuditoria(MySqlCommand cmd, UsuarioGeneral u)
        {
            cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = u.IdUsuario;
            cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = u.Usuario;
            cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = u.IdEmpresa;
            cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = u.IdRol;
        }
    }
}
