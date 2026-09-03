using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

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

        private static decimal? GetNullableDecimal(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static int? GetNullableInt(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        // Une SP_Cliente_ObtenerParaFacturacion + SP_Pedido_ObtenerParaFacturacion en un solo viaje.
        // idCliente es NULL cuando el llamador solo necesita resolver pedidos (GuardarCambiosFacturaAsync).
        public async Task<Respuesta> ObtenerDatosBorradorAsync(UsuarioGeneral usuarioLogueado, int? idCliente, List<int> idPedidos)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Facturacion_ObtenerDatosBorrador", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", (object?)idCliente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@jsonIdPedidos", JsonSerializer.Serialize(idPedidos));

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
                                NumReferencia = GetNullableString(dr, "NumReferencia"),
                                Precio = GetNullableDecimal(dr, "Precio")
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_ObtenerIdDocumentoElectronico", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);

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

        // idPedidoFacturaLinea: líneas ya libres a asociar a idDocumentoElectronico (no pedidos —
        // ver PLAN_Lineas_Facturacion.md). El SP fija IdEstadoFacturacion=1 (PendienteEnvio) él
        // solo, ya no se pasa como parámetro.
        public async Task<Respuesta> RegistrarEnvioAsync(
            UsuarioGeneral usuarioLogueado, List<int> idPedidoFacturaLinea, int idDocumentoElectronico)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_RegistrarEnvio", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@lstIdPedidoFacturaLinea", JsonSerializer.Serialize(idPedidoFacturaLinea));
                cmd.Parameters.AddWithValue("@intIdDocumentoElectronico", idDocumentoElectronico);

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

        // Libera (IdPedidoFacturaLinea = NULL en sus pedidos, SoftDelete=1 en la línea) toda línea
        // que hoy esté en idDocumentoElectronico pero ya no venga en idPedidoFacturaLineaVigentes
        // (línea quitada en un GuardarCambios) — composición de línea inmutable, ya no se opera
        // pedido por pedido (ver PLAN_Lineas_Facturacion.md).
        public async Task<Respuesta> DesvincularAsync(
            UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, List<int> idPedidoFacturaLineaVigentes)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_Desvincular", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdDocumentoElectronico", idDocumentoElectronico);
                cmd.Parameters.AddWithValue("@lstIdPedidoFacturaLinea", JsonSerializer.Serialize(idPedidoFacturaLineaVigentes));

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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_ActualizarEstado", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);
                cmd.Parameters.AddWithValue("@intIdEstadoFacturacion", idEstadoFacturacion);

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

        // Usado por el worker de sincronización — checkpoint por empresa (SINCRONIZACION_FACTURACION_CHECKPOINT), no requiere usuario.
        public async Task<Respuesta> ObtenerCheckpointsSincronizacionAsync()
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_ObtenerCheckpointsSincronizacion", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var checkpoints = new List<CheckpointSincronizacionConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        checkpoints.Add(new CheckpointSincronizacionConsulta
                        {
                            IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]),
                            UltimoIdEvento = Convert.ToInt32(dr["UltimoIdEvento"])
                        });

                    respuesta.Result = checkpoints;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // checkpoints: ID=IdEmpresa, NUM1=UltimoIdEvento a fijar para esa empresa.
        public async Task<Respuesta> ActualizarCheckpointSincronizacionAsync(List<(int IdEmpresa, int UltimoIdEvento)> checkpoints)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_ActualizarCheckpointSincronizacion", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                var checkpointJson = checkpoints.Select(c => new { c.IdEmpresa, c.UltimoIdEvento });
                cmd.Parameters.AddWithValue("@lstCheckpoint", JsonSerializer.Serialize(checkpointJson));

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

        // documentosConEstado: ID=IdDocumentoElectronico, NUM1=IdEstadoFacturacion resuelto (8=Anulación Aprobada, 9=Anulación Rechazada).
        public async Task<Respuesta> ActualizarEstadoPorDocumentoAsync(int idEmpresa, List<(int IdDocumentoElectronico, int IdEstadoFacturacion)> documentosConEstado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_ActualizarEstadoPorDocumento", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdEmpresa", idEmpresa);

                var documentoJson = documentosConEstado.Select(d => new { d.IdDocumentoElectronico, d.IdEstadoFacturacion });
                cmd.Parameters.AddWithValue("@lstDocumento", JsonSerializer.Serialize(documentoJson));

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

        // Solo valida que el usuario tenga rol 6 (acceso al dashboard) — el monto real ahora sale de
        // ms-facturación (ver PedidoFacturaHandler.ObtenerResumenDashboardAsync); SP_PedidoFactura_
        // ValidarAccesoResumen (renombrado de SP_PedidoFactura_Resumen) ya no calcula nada, solo autoriza.
        public async Task<Respuesta> ValidarAccesoResumenAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_ValidarAccesoResumen", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

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

        // Mismo patrón que ValidarAccesoResumenAsync (solo autoriza, no calcula ni consulta nada) — para el
        // resto de endpoints de PedidoFactura que usan ms-facturación como proxy HTTP. A diferencia de
        // ValidarAccesoResumenAsync (rol 6, exclusivo del dashboard), SP_PedidoFactura_ValidarAccesoFacturacion
        // exige rol 2 — mismo criterio que todo el módulo facturación. razon describe la acción para el
        // mensaje de error (SP_PedidoFactura_ValidarAccesoFacturacion es genérico, reusado desde muchos
        // endpoints distintos).
        public async Task<Respuesta> ValidarAccesoFacturacionAsync(UsuarioGeneral usuarioLogueado, string razon)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoFactura_ValidarAccesoFacturacion", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchRazon", razon);

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

        public async Task<Respuesta> ObtenerResumenAnaliticoAsync(UsuarioGeneral usuarioLogueado, FiltroFacturacionAnaliticaRequest filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Facturacion_ResumenAnalitico", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", (object?)filtro.idCliente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPais", (object?)filtro.idPais ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTipoTramite", (object?)filtro.idTipoTramite ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje != 2)
                {
                    return respuesta;
                }

                var resultado = new ResumenAnaliticoFacturacionConsulta();

                if (await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    resultado.Indicadores = new IndicadoresFacturacionConsulta
                    {
                        CantidadPedidosPendientes = Convert.ToInt32(dr["CantidadPedidosPendientes"]),
                        MontoPendienteFacturar = GetNullableDecimal(dr, "MontoPendienteFacturar") ?? 0m,
                        CantidadPedidosFacturados = Convert.ToInt32(dr["CantidadPedidosFacturados"])
                    };
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.DesglosePorTramite.Add(new DesgloseTramiteConsulta
                        {
                            IdTipoTramite = GetNullableInt(dr, "IdTipoTramite"),
                            TipoTramite = dr["TipoTramite"]?.ToString() ?? string.Empty,
                            CantidadPedidos = Convert.ToInt32(dr["CantidadPedidos"]),
                            MontoFacturado = Convert.ToDecimal(dr["MontoFacturado"])
                        });
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.DesglosePorPais.Add(new DesglosePaisConsulta
                        {
                            IdPais = GetNullableInt(dr, "IdPais"),
                            Pais = dr["Pais"]?.ToString() ?? string.Empty,
                            CantidadPedidos = Convert.ToInt32(dr["CantidadPedidos"]),
                            MontoFacturado = Convert.ToDecimal(dr["MontoFacturado"])
                        });
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ObtenerResumenClientesGlobalAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Facturacion_ResumenClientesGlobal", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje != 2)
                {
                    return respuesta;
                }

                var clientes = new List<ResumenClienteGlobalConsulta>();
                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        clientes.Add(new ResumenClienteGlobalConsulta
                        {
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            Cliente = dr["Cliente"]?.ToString() ?? string.Empty,
                            CantidadPedidosFacturados = Convert.ToInt32(dr["CantidadPedidosFacturados"]),
                            TotalFacturado = GetNullableDecimal(dr, "TotalFacturado") ?? 0m,
                            MontoPendienteFacturar = GetNullableDecimal(dr, "MontoPendienteFacturar") ?? 0m
                        });
                }

                respuesta.Result = clientes;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}