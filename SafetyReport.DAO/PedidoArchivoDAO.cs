using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class PedidoArchivoDAO
    {
        private readonly DbConfig _dbConfig;

        public PedidoArchivoDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();

            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 3;

                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                var json = dr["Result"]?.ToString();

                respuesta.Result = !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<T>()
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

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoCrear request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("PedidoArchivo_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.IdPedido;
                cmd.Parameters.Add("@vchDocumentoURL", SqlDbType.VarChar).Value = request.DocumentoURL;
                cmd.Parameters.Add("@vchNombreDocumento", SqlDbType.VarChar, 255).Value = request.NombreDocumento;
                cmd.Parameters.Add("@vchFormatoDocumento", SqlDbType.VarChar, 255).Value = request.FormatoDocumento;
                cmd.Parameters.Add("@bigTamanoArchivo", SqlDbType.BigInt).Value = request.TamanoArchivo;
                cmd.Parameters.Add("@intIdTipoArchivo", SqlDbType.Int).Value = request.IdTipoArchivo;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoArchivoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("PedidoArchivo_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedidoArchivo", SqlDbType.Int).Value = request.IdPedidoArchivo;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.IdPedido;
                cmd.Parameters.Add("@vchDocumentoURL", SqlDbType.VarChar).Value = request.DocumentoURL;
                cmd.Parameters.Add("@vchNombreDocumento", SqlDbType.VarChar, 255).Value = request.NombreDocumento;
                cmd.Parameters.Add("@vchFormatoDocumento", SqlDbType.VarChar, 255).Value = request.FormatoDocumento;
                cmd.Parameters.Add("@bigTamanoArchivo", SqlDbType.BigInt).Value = request.TamanoArchivo;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoArchivoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("PedidoArchivo_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedidoArchivo", SqlDbType.Int).Value = request.IdPedidoArchivo;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.IdPedido;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoArchivoConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoArchivo request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("PedidoArchivo_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.idPedido;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 3;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<PedidoArchivoListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new PedidoArchivoListaResult()
                        : new PedidoArchivoListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 3;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new PedidoArchivoListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoArchivoListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("PedidoArchivo_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedidoArchivo", SqlDbType.Int).Value = request.IdPedidoArchivo;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.IdPedido;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoArchivoEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoEliminado>()
                };
            }
        }

    }
}