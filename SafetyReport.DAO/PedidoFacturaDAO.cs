using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

namespace SafetyReport.DAO
{
    public class PedidoFacturaDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<PedidoFacturaDAO> _logger;

        public PedidoFacturaDAO(DbConfig dbConfig, ILogger<PedidoFacturaDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        public async Task<Respuesta> RegistrarEnvioAsync(
            UsuarioGeneral usuarioLogueado, int idPedido, int idDocumentoElectronico, int? idEstadoFacturacion)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFactura_RegistrarEnvio", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                cmd.Parameters.Add("@intIdDocumentoElectronico", SqlDbType.Int).Value = idDocumentoElectronico;
                cmd.Parameters.Add("@intIdEstadoFacturacion", SqlDbType.Int).Value = (object?)idEstadoFacturacion ?? DBNull.Value;

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();

                if (!await dr.ReadAsync())
                {
                    return new Respuesta { IdTipoMensaje = 3, Mensaje = "El procedimiento almacenado no devolvió el resultado esperado." };
                }

                return new Respuesta
                {
                    IdTipoMensaje = Convert.ToInt32(dr["IdTipoMensaje"]),
                    Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}
