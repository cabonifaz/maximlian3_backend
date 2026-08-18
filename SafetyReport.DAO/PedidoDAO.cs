using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class PedidoDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<PedidoDAO> _logger;

        public PedidoDAO(DbConfig dbConfig, ILogger<PedidoDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
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

        private static int? GetNullableInt(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static bool? GetNullableBool(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToBoolean(dr[columna]);

        private static DateTime? GetNullableDateTime(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDateTime(dr[columna]);

        private static string? GetNullableString(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Pedido request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                cmd.Parameters.AddWithValue("@vchCodigo", request.Codigo);
                cmd.Parameters.AddWithValue("@intIdCliente", request.IdCliente);
                cmd.Parameters.AddWithValue("@vchNumeroDocumento", (object?)request.NumeroDocumento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNombreCliente", (object?)request.NombreCliente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTipoPersona", request.IdTipoPersona);
                cmd.Parameters.AddWithValue("@intIdEmpresaAtencion", request.IdEmpresaAtencion);
                cmd.Parameters.AddWithValue("@vchNumeroDocumentoInvestigado", (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchInvestigarRazonSocialNombres", request.InvestigarRazonSocialNombres);
                cmd.Parameters.AddWithValue("@intIdCompania", request.IdCompania);
                cmd.Parameters.AddWithValue("@intIdTarifario", request.IdTarifario);
                cmd.Parameters.AddWithValue("@intIdPlantilla", request.IdPlantilla);
                cmd.Parameters.AddWithValue("@intIdIdioma", request.IdIdioma);
                cmd.Parameters.AddWithValue("@intIdClaseInforme", request.IdClaseInforme);
                cmd.Parameters.AddWithValue("@vchNumReferencia", (object?)request.NumReferencia ?? DBNull.Value);

                var decMontoCredito = cmd.Parameters.AddWithValue("@decMontoCredito", (object?)request.MontoCredito ?? DBNull.Value);
                decMontoCredito.Precision = 18;
                decMontoCredito.Scale = 2;

                cmd.Parameters.AddWithValue("@intPlazoCredito", (object?)request.PlazoCredito ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTipoPlazoCredito", (object?)request.IdTipoPlazoCredito ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchTipoPlazoCredito", (object?)request.TipoPlazoCredito ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFchDesde", (object?)request.FchDesde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFchHasta", (object?)request.FchHasta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchComentario", (object?)request.Comentario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEstado", request.IdEstado);
                cmd.Parameters.AddWithValue("@bitImprimeLogoSafety", request.ImprimeLogoSafety);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoCreado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new PedidoCreado { IdPedido = Convert.ToInt32(dr["IdPedido"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoCreado>()
                };
            }
        }
        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarPedido request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                cmd.Parameters.AddWithValue("@intIdPedido", request.IdPedido);
                cmd.Parameters.AddWithValue("@vchCodigo", request.Codigo);
                cmd.Parameters.AddWithValue("@intIdCliente", request.IdCliente);
                cmd.Parameters.AddWithValue("@vchNumeroDocumento", (object?)request.NumeroDocumento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNombreCliente", (object?)request.NombreCliente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTipoPersona", request.IdTipoPersona);
                cmd.Parameters.AddWithValue("@vchNumeroDocumentoInvestigado", (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchInvestigarRazonSocialNombres", request.InvestigarRazonSocialNombres);
                cmd.Parameters.AddWithValue("@intIdCompania", request.IdCompania);
                cmd.Parameters.AddWithValue("@intIdTarifario", request.IdTarifario);
                cmd.Parameters.AddWithValue("@intIdPlantilla", request.IdPlantilla);
                cmd.Parameters.AddWithValue("@intIdIdioma", request.IdIdioma);
                cmd.Parameters.AddWithValue("@intIdClaseInforme", request.IdClaseInforme);
                cmd.Parameters.AddWithValue("@vchNumReferencia", (object?)request.NumReferencia ?? DBNull.Value);

                var decMontoCredito = cmd.Parameters.AddWithValue("@decMontoCredito", (object?)request.MontoCredito ?? DBNull.Value);
                decMontoCredito.Precision = 18;
                decMontoCredito.Scale = 2;

                cmd.Parameters.AddWithValue("@intPlazoCredito", (object?)request.PlazoCredito ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTipoPlazoCredito", (object?)request.IdTipoPlazoCredito ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchTipoPlazoCredito", (object?)request.TipoPlazoCredito ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFchDesde", (object?)request.FchDesde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFchHasta", (object?)request.FchHasta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchComentario", (object?)request.Comentario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEstado", request.IdEstado);
                cmd.Parameters.AddWithValue("@bitImprimeLogoSafety", request.ImprimeLogoSafety);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoCreado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new PedidoCreado { IdPedido = Convert.ToInt32(dr["IdPedido"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoObtener request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", (object?)request.idPedido ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdCliente", (object?)request.idCliente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTarifario", (object?)request.idTarifario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNombreInvestigado", (object?)request.nombreInvestigado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNumRef", (object?)request.numRef ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@lstIdEstado", JsonSerializer.Serialize(request.idEstado ?? new List<int>()));

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        lista.Add(new PedidoConsulta
                        {
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                            NombreCliente = GetNullableString(dr, "NombreCliente"),
                            IdTipoPersona = Convert.ToInt32(dr["IdTipoPersona"]),
                            IdCompania = Convert.ToInt32(dr["IdCompania"]),
                            NumeroDocumentoInvestigado = GetNullableString(dr, "NumeroDocumentoInvestigado"),
                            InvestigarRazonSocialNombres = GetNullableString(dr, "InvestigarRazonSocialNombres"),
                            IdTarifario = Convert.ToInt32(dr["IdTarifario"]),
                            IdPlantilla = Convert.ToInt32(dr["IdPlantilla"]),
                            IdIdioma = Convert.ToInt32(dr["IdIdioma"]),
                            IdClaseInforme = Convert.ToInt32(dr["IdClaseInforme"]),
                            NumReferencia = GetNullableString(dr, "NumReferencia"),
                            MontoCredito = GetNullableDecimal(dr, "MontoCredito"),
                            PlazoCredito = GetNullableInt(dr, "PlazoCredito"),
                            IdTipoPlazoCredito = GetNullableInt(dr, "IdTipoPlazoCredito"),
                            FchDesde = GetNullableDateTime(dr, "FchDesde"),
                            FchHasta = GetNullableDateTime(dr, "FchHasta"),
                            Comentario = GetNullableString(dr, "Comentario"),
                            IdEstado = Convert.ToInt32(dr["IdEstado"]),
                            ImprimeLogoSafety = Convert.ToBoolean(dr["ImprimeLogoSafety"])
                        });
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedido request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)request.busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdCliente", (object?)request.idCliente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchIdEstado", (object?)request.idEstado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)request.numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                        resultado.Pendiente = Convert.ToInt32(dr["Pendiente"]);
                        resultado.Aprobado = Convert.ToInt32(dr["Aprobado"]);
                        resultado.Cancelado = Convert.ToInt32(dr["Cancelado"]);
                    }

                    var pedidosPorId = new Dictionary<int, PedidoListaConsulta>();
                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var pedido = new PedidoListaConsulta
                            {
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                Cliente = GetNullableString(dr, "Cliente"),
                                Investigado = GetNullableString(dr, "Investigado"),
                                Idioma = GetNullableString(dr, "Idioma"),
                                RequiereTraduccion = GetNullableInt(dr, "RequiereTraduccion"),
                                LogoImprimible = GetNullableString(dr, "LogoImprimible"),
                                Estado = Convert.ToInt32(dr["Estado"]),
                                DescripcionEstado = GetNullableString(dr, "DescripcionEstado"),
                                ColorLetra = GetNullableString(dr, "ColorLetra"),
                                ColorFondo = GetNullableString(dr, "ColorFondo"),
                                FechaMod = GetNullableString(dr, "FechaMod"),
                                Asignaciones = new List<PedidoAsignacionResumen>()
                            };
                            pedidosPorId[pedido.IdPedido] = pedido;
                            resultado.lstPedido.Add(pedido);
                        }
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var idPedido = Convert.ToInt32(dr["IdPedido"]);
                            if (pedidosPorId.TryGetValue(idPedido, out var pedido))
                                pedido.Asignaciones!.Add(new PedidoAsignacionResumen
                                {
                                    IdEstadoAsignacion = Convert.ToInt32(dr["IdEstadoAsignacion"]),
                                    DescripcionAsignacion = GetNullableString(dr, "DescripcionAsignacion"),
                                    IdEstadoInforme = GetNullableInt(dr, "IdEstadoInforme"),
                                    DescripcionEstadoInforme = GetNullableString(dr, "DescripcionEstadoInforme")
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

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoListaResult()
                };
            }
        }

        public async Task<Respuesta> ListarAsignacionAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoAsignacion request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_PedidoAsignacion_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)request.busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPedido", (object?)request.idPedido ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchIdEstado", (object?)request.idEstado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdEstadoAsignacion", (object?)request.idEstadoAsignacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)request.numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoAsignacionListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    var pedidosPorId = new Dictionary<int, PedidoAsignacionListaConsulta>();
                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var pedido = new PedidoAsignacionListaConsulta
                            {
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                Nombre = GetNullableString(dr, "Nombre"),
                                Investigado = GetNullableString(dr, "Investigado"),
                                Idioma = GetNullableString(dr, "Idioma"),
                                TipoTramite = GetNullableString(dr, "TipoTramite"),
                                DiasMin = GetNullableInt(dr, "DiasMin"),
                                DiasMax = GetNullableInt(dr, "DiasMax"),
                                Vigencia = GetNullableString(dr, "Vigencia"),
                                Asignaciones = new List<PedidoAsignacionResumen>()
                            };
                            pedidosPorId[pedido.IdPedido] = pedido;
                            resultado.lstPedido.Add(pedido);
                        }
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var idPedido = Convert.ToInt32(dr["IdPedido"]);
                            if (pedidosPorId.TryGetValue(idPedido, out var pedido))
                                pedido.Asignaciones!.Add(new PedidoAsignacionResumen
                                {
                                    IdEstadoAsignacion = Convert.ToInt32(dr["IdEstadoAsignacion"]),
                                    DescripcionAsignacion = GetNullableString(dr, "DescripcionAsignacion")
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

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoAsignacionListaResult()
                };
            }
        }

        public async Task<Respuesta> CancelarAsync(UsuarioGeneral usuarioLogueado, int idPedido)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Cancelar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoEliminado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new PedidoEliminado { IdPedido = Convert.ToInt32(dr["IdPedido"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idPedido)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdPedido", idPedido);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoEliminado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new PedidoEliminado { IdPedido = Convert.ToInt32(dr["IdPedido"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerResumenAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Resumen", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<PedidoEstadoResumenItem>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new PedidoEstadoResumenItem
                        {
                            IdEstado = Convert.ToInt32(dr["IdEstado"]),
                            DescripcionEstado = GetNullableString(dr, "DescripcionEstado"),
                            ColorLetra = GetNullableString(dr, "ColorLetra"),
                            ColorFondo = GetNullableString(dr, "ColorFondo"),
                            Cantidad = Convert.ToInt32(dr["Cantidad"])
                        });
                    }
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEstadoResumenItem>()
                };
            }
        }

        public async Task<Respuesta> ListarParaFacturacionAsync(UsuarioGeneral usuarioLogueado, ListarPedidosFacturacionRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_ListarParaFacturacion", cn);

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", request.idCliente);
                cmd.Parameters.AddWithValue("@intIdTipoTramite", (object?)request.idTipoTramite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFechaInicio", (object?)request.fechaInicio?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFechaFin", (object?)request.fechaFin?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdDocumentoElectronico", request.idDocumentoElectronico);
                cmd.Parameters.AddWithValue("@numPag", request.numPag);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoListaFacturacionResult();
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
                            resultado.Pedidos.Add(new PedidoListaFacturacionConsulta
                            {
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                                NumReferencia = dr["NumReferencia"]?.ToString() ?? string.Empty,
                                Investigado = GetNullableString(dr, "Investigado"),
                                AplicaPenalidad = GetNullableString(dr, "AplicaPenalidad"),
                                TipoTramite = GetNullableString(dr, "TipoTramite"),
                                Fecha = Convert.ToDateTime(dr["Fecha"]),
                                Penalidad = GetNullableDecimal(dr, "Penalidad"),
                                Precio = GetNullableDecimal(dr, "Precio"),
                                DescuentoPorcentaje = GetNullableDecimal(dr, "DescuentoPorcentaje")
                            });
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
                    Result = new PedidoListaFacturacionResult()
                };
            }
        }

    }
}
