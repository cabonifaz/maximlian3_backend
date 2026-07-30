using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

namespace SafetyReport.DAO
{
    public class DashboardDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<DashboardDAO> _logger;

        public DashboardDAO(DbConfig dbConfig, ILogger<DashboardDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static decimal? GetNullableDecimal(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

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

        public async Task<Respuesta> ObtenerResumenClientesAsync(UsuarioGeneral usuarioActual)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Dashboard_ResumenClientes", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new ResumenClientesDashboard();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalClientes = Convert.ToInt32(dr["TotalClientes"]);
                        resultado.TotalActivos = Convert.ToInt32(dr["TotalActivos"]);
                        resultado.TotalInactivos = Convert.ToInt32(dr["TotalInactivos"]);
                        resultado.PorcentajeActivos = GetNullableDecimal(dr, "PorcentajeActivos");
                        resultado.PorcentajeCrecimiento = GetNullableDecimal(dr, "PorcentajeCrecimiento");
                        resultado.FechaActualizacion = Convert.ToDateTime(dr["FechaActualizacion"]);
                    }
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ResumenClientesDashboard()
                };
            }
        }
    }
}
