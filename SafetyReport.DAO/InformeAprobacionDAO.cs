using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

namespace SafetyReport.DAO
{
    public class InformeAprobacionDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeAprobacionDAO> _logger;

        public InformeAprobacionDAO(DbConfig dbConfig, ILogger<InformeAprobacionDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static string? GetNullableString(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje.
        private async Task<Respuesta> LeerCabeceraAsync(SqlDataReader dr, string procedimiento)
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

        public async Task<Respuesta> ListarPendientesAsync(UsuarioGeneral u, FiltroInformeAprobacionPendientes filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_InformeAprobacion_ListarPendientes", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = u.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = u.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = u.IdRol;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)filtro.NumPag ?? DBNull.Value;
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

        public async Task<Respuesta> AprobarAsync(UsuarioGeneral u, InformeAprobacionAprobarRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_InformeAprobacion_Aprobar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = u.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = u.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = u.IdRol;
                cmd.Parameters.Add("@intIdInforme", SqlDbType.Int).Value = request.IdInforme;
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
    }
}
