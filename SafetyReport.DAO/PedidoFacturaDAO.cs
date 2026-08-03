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

        private static DataTable ConstruirTablaListaGeneralNum(List<int> valores)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("NUM1", typeof(int));

            int i = 1;
            foreach (var valor in valores)
                table.Rows.Add(i++, valor);

            return table;
        }

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

        private static string? GetNullableString(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        // Une SP_Cliente_ObtenerParaFacturacion + SP_Pedido_ObtenerParaFacturacion en un solo viaje.
        // idCliente es NULL cuando el llamador solo necesita resolver pedidos (GuardarCambiosFacturaAsync).
        public async Task<Respuesta> ObtenerDatosBorradorAsync(UsuarioGeneral usuarioLogueado, int? idCliente, List<int> idPedidos)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Facturacion_ObtenerDatosBorrador", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = (object?)idCliente ?? DBNull.Value;

                var tvpIdPedido = cmd.Parameters.AddWithValue("@lstIdPedido", ConstruirTablaListaGeneralNum(idPedidos));
                tvpIdPedido.SqlDbType = SqlDbType.Structured;
                tvpIdPedido.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    ClienteParaFacturacionConsulta? cliente = null;
                    if (await dr.ReadAsync())
                    {
                        cliente = new ClienteParaFacturacionConsulta
                        {
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            IdTipoDocumentoSunat = Convert.ToInt32(dr["IdTipoDocumentoSunat"]),
                            NumeroDocumento = dr["NumeroDocumento"]?.ToString() ?? string.Empty,
                            Nombre = GetNullableString(dr, "Nombre"),
                            Correo = GetNullableString(dr, "Correo"),
                            Direccion = GetNullableString(dr, "Direccion"),
                            IdPais = Convert.ToInt32(dr["IdPais"])
                        };
                    }

                    var pedidos = new List<PedidoParaFacturacionConsulta>();
                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                            pedidos.Add(new PedidoParaFacturacionConsulta
                            {
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                                NombreCliente = GetNullableString(dr, "NombreCliente"),
                                NumReferencia = GetNullableString(dr, "NumReferencia")
                            });
                    }

                    respuesta.Result = new DatosBorradorFacturaConsulta { Cliente = cliente ?? new(), Pedidos = pedidos };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ObtenerIdDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFactura_ObtenerIdDocumentoElectronico", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    respuesta.Result = new PedidoFacturaIdDocumentoConsulta
                    {
                        IdPedido = Convert.ToInt32(dr["IdPedido"]),
                        IdDocumentoElectronico = Convert.ToInt32(dr["IdDocumentoElectronico"]),
                        IdEstadoFacturacion = Convert.ToInt32(dr["IdEstadoFacturacion"])
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> RegistrarEnvioAsync(
            UsuarioGeneral usuarioLogueado, List<int> idPedidos, int idDocumentoElectronico, int? idEstadoFacturacion)
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

                var tvpIdPedido = cmd.Parameters.AddWithValue("@lstIdPedido", ConstruirTablaListaGeneralNum(idPedidos));
                tvpIdPedido.SqlDbType = SqlDbType.Structured;
                tvpIdPedido.TypeName = "LISTA_GENERAL_NUM";

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

        // Desvincula (IdDocumentoElectronico = NULL) los pedidos que ya no vienen en las líneas del
        // documento (línea eliminada en un GuardarCambios).
        public async Task<Respuesta> DesvincularAsync(
            UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, List<int> idPedidosVigentes)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFactura_Desvincular", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdDocumentoElectronico", SqlDbType.Int).Value = idDocumentoElectronico;

                var tvpIdPedido = cmd.Parameters.AddWithValue("@lstIdPedido", ConstruirTablaListaGeneralNum(idPedidosVigentes));
                tvpIdPedido.SqlDbType = SqlDbType.Structured;
                tvpIdPedido.TypeName = "LISTA_GENERAL_NUM";

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

        public async Task<Respuesta> ActualizarEstadoAsync(UsuarioGeneral usuarioLogueado, int idPedido, int idEstadoFacturacion)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoFactura_ActualizarEstado", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;
                cmd.Parameters.Add("@intIdEstadoFacturacion", SqlDbType.Int).Value = idEstadoFacturacion;

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
