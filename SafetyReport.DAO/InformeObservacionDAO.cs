using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class InformeObservacionDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<InformeObservacionDAO> _logger;

        public InformeObservacionDAO(DbConfig dbConfig, ILogger<InformeObservacionDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        public async Task<Respuesta> ListarObservacionesAsync(UsuarioGeneral u, int idPedido)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeObservacion_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeObservacionConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        lista.Add(new InformeObservacionConsulta
                        {
                            IdInformeObservacion = Convert.ToInt32(dr["IdInformeObservacion"]),
                            IdInforme = Convert.ToInt32(dr["IdInforme"]),
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            Observacion = GetNullableString(dr, "Observacion"),
                            Checked = Convert.ToBoolean(dr["Checked"])
                        });
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeObservacionConsulta>() };
            }
        }

        public async Task<Respuesta> InsertarObservacionesLoteAsync(UsuarioGeneral u, int idInforme, int idPedido, List<InformeObservacionItem> observaciones)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeObservacion_InsertarLote", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInforme", idInforme);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
                cmd.Parameters.AddWithValue("@lstObservaciones", JsonSerializer.Serialize(observaciones));
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeIdResult>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new InformeIdResult { IdInforme = Convert.ToInt32(dr["IdInforme"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EditarObservacionAsync(UsuarioGeneral u, InformeObservacionEditarRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeObservacion_Editar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInformeObservacion", request.IdInformeObservacion);
                cmd.Parameters.AddWithValue("@vchObservacion", (object?)request.Observacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bitChecked", request.Checked);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeObservacionIdResult>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new InformeObservacionIdResult { IdInformeObservacion = Convert.ToInt32(dr["IdInformeObservacion"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EliminarObservacionAsync(UsuarioGeneral u, int idInformeObservacion)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InformeObservacion_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", u.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", u.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", u.IdRol);
                cmd.Parameters.AddWithValue("@intIdInformeObservacion", idInformeObservacion);
                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InformeObservacionIdResult>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new InformeObservacionIdResult { IdInformeObservacion = Convert.ToInt32(dr["IdInformeObservacion"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje. Sin columna Result.
        private async Task<Respuesta> LeerCabeceraAsync(DbDataReader dr, string procedimiento)
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

        private static string? GetNullableString(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

    }
}
