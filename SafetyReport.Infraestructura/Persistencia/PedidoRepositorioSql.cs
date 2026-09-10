using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Pedido;
using SafetyReport.Application.Puertos.PedidoFactura;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class PedidoRepositorioSql : IPedidoRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<PedidoRepositorioSql> _logger;

        public PedidoRepositorioSql(DbConfig dbConfig, ILogger<PedidoRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // Shape esperado por SP_Pedido_Obtener (p_jsonIdEstado): array de {NUM1} — solo se usa en un IN(),
        // no requiere orden ni ID explícito.
        private static string ConstruirJsonListaGeneralNum(List<int>? valores) =>
            JsonSerializer.Serialize((valores ?? new List<int>()).Select(v => new { NUM1 = v }));

        // Shape esperado por SP_Pedido_ListarParaPrefactura (p_jsonAnioMes): array de {Anio, Mes}.
        private static string ConstruirJsonListaAnioMes(List<AnioMesFiltro>? valores) =>
            JsonSerializer.Serialize((valores ?? new List<AnioMesFiltro>()).Select(v => new { v.Anio, v.Mes }));

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje. Sin columna Result.
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

        private static int? GetNullableInt(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static bool? GetNullableBool(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToBoolean(dr[columna]);

        private static DateTime? GetNullableDateTime(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDateTime(dr[columna]);

        private static string? GetNullableString(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Pedido request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@p_vchCodigo", MySqlDbType.VarChar, 50).Value = request.Codigo;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_vchNumeroDocumento", MySqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombreCliente", MySqlDbType.VarChar, 255).Value = (object?)request.NombreCliente ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTipoPersona", MySqlDbType.Int32).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@p_intIdEmpresaAtencion", MySqlDbType.Int32).Value = request.IdEmpresaAtencion;
                cmd.Parameters.Add("@p_vchNumeroDocumentoInvestigado", MySqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchInvestigarRazonSocialNombres", MySqlDbType.VarChar).Value = request.InvestigarRazonSocialNombres;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = request.IdCompania;
                cmd.Parameters.Add("@p_intIdTarifario", MySqlDbType.Int32).Value = request.IdTarifario;
                cmd.Parameters.Add("@p_intIdPlantilla", MySqlDbType.Int32).Value = request.IdPlantilla;
                cmd.Parameters.Add("@p_intIdIdioma", MySqlDbType.Int32).Value = request.IdIdioma;
                cmd.Parameters.Add("@p_intIdClaseInforme", MySqlDbType.Int32).Value = request.IdClaseInforme;
                cmd.Parameters.Add("@p_vchNumReferencia", MySqlDbType.VarChar, 32).Value = (object?)request.NumReferencia ?? DBNull.Value;

                cmd.Parameters.Add("@p_decMontoCredito", MySqlDbType.Decimal).Value = (object?)request.MontoCredito ?? DBNull.Value;
                cmd.Parameters["@p_decMontoCredito"].Precision = 18;
                cmd.Parameters["@p_decMontoCredito"].Scale = 2;

                cmd.Parameters.Add("@p_intPlazoCredito", MySqlDbType.Int32).Value = (object?)request.PlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTipoPlazoCredito", MySqlDbType.Int32).Value = (object?)request.IdTipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchTipoPlazoCredito", MySqlDbType.VarChar).Value = (object?)request.TipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtFchDesde", MySqlDbType.DateTime).Value = (object?)request.FchDesde ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtFchHasta", MySqlDbType.DateTime).Value = (object?)request.FchHasta ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchComentario", MySqlDbType.VarChar).Value = (object?)request.Comentario ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = request.IdEstado;
                cmd.Parameters.Add("@p_bitImprimeLogoSafety", MySqlDbType.Bool).Value = request.ImprimeLogoSafety;

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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = request.IdPedido;
                cmd.Parameters.Add("@p_vchCodigo", MySqlDbType.VarChar, 50).Value = request.Codigo;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_vchNumeroDocumento", MySqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombreCliente", MySqlDbType.VarChar, 255).Value = (object?)request.NombreCliente ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTipoPersona", MySqlDbType.Int32).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@p_vchNumeroDocumentoInvestigado", MySqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchInvestigarRazonSocialNombres", MySqlDbType.VarChar).Value = request.InvestigarRazonSocialNombres;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = request.IdCompania;
                cmd.Parameters.Add("@p_intIdTarifario", MySqlDbType.Int32).Value = request.IdTarifario;
                cmd.Parameters.Add("@p_intIdPlantilla", MySqlDbType.Int32).Value = request.IdPlantilla;
                cmd.Parameters.Add("@p_intIdIdioma", MySqlDbType.Int32).Value = request.IdIdioma;
                cmd.Parameters.Add("@p_intIdClaseInforme", MySqlDbType.Int32).Value = request.IdClaseInforme;
                cmd.Parameters.Add("@p_vchNumReferencia", MySqlDbType.VarChar, 32).Value = (object?)request.NumReferencia ?? DBNull.Value;

                cmd.Parameters.Add("@p_decMontoCredito", MySqlDbType.Decimal).Value = (object?)request.MontoCredito ?? DBNull.Value;
                cmd.Parameters["@p_decMontoCredito"].Precision = 18;
                cmd.Parameters["@p_decMontoCredito"].Scale = 2;

                cmd.Parameters.Add("@p_intPlazoCredito", MySqlDbType.Int32).Value = (object?)request.PlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTipoPlazoCredito", MySqlDbType.Int32).Value = (object?)request.IdTipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchTipoPlazoCredito", MySqlDbType.VarChar).Value = (object?)request.TipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtFchDesde", MySqlDbType.DateTime).Value = (object?)request.FchDesde ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtFchHasta", MySqlDbType.DateTime).Value = (object?)request.FchHasta ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchComentario", MySqlDbType.VarChar).Value = (object?)request.Comentario ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = request.IdEstado;
                cmd.Parameters.Add("@p_bitImprimeLogoSafety", MySqlDbType.Bool).Value = request.ImprimeLogoSafety;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = (object?)request.idPedido ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = (object?)request.idCliente ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTarifario", MySqlDbType.Int32).Value = (object?)request.idTarifario ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombreInvestigado", MySqlDbType.VarChar, 255).Value = (object?)request.nombreInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNumRef", MySqlDbType.VarChar, 50).Value = (object?)request.numRef ?? DBNull.Value;

                cmd.Parameters.Add("@p_jsonIdEstado", MySqlDbType.JSON).Value = ConstruirJsonListaGeneralNum(request.idEstado);

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
                            ImprimeLogoSafety = Convert.ToBoolean(dr["ImprimeLogoSafety"]),
                            IdEmpresaAtencion = Convert.ToInt32(dr["IdEmpresaAtencion"])
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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = (object?)request.idCliente ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchIdEstado", MySqlDbType.VarChar).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)request.numPag ?? DBNull.Value;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = (object?)request.idPedido ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchIdEstado", MySqlDbType.VarChar).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@p_IdEstadoAsignacion", MySqlDbType.Int32).Value = (object?)request.idEstadoAsignacion ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)request.numPag ?? DBNull.Value;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = idPedido;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = idPedido;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

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

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;
                cmd.Parameters.Add("@p_intIdTipoTramite", MySqlDbType.Int32).Value = request.idTipoTramite;
                cmd.Parameters.Add("@p_intAnio", MySqlDbType.Int32).Value = (object?)request.anio ?? DBNull.Value;
                cmd.Parameters.Add("@p_intMes", MySqlDbType.Int32).Value = (object?)request.mes ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchIdPais", MySqlDbType.VarChar, 200).Value = request.idsPais is { Count: > 0 } idsPais ? (object)string.Join(",", idsPais) : DBNull.Value;
                cmd.Parameters.Add("@p_intIdMoneda", MySqlDbType.Int32).Value = (object?)request.idMoneda ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoListaFacturacionResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.Pedidos.Add(new PedidoListaFacturacionConsulta
                        {
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                            NumReferencia = dr["NumReferencia"]?.ToString() ?? string.Empty,
                            Investigado = GetNullableString(dr, "Investigado"),
                            IdPais = GetNullableInt(dr, "IdPais"),
                            Pais = GetNullableString(dr, "Pais"),
                            AplicaPenalidad = GetNullableString(dr, "AplicaPenalidad"),
                            IdTipoTramite = GetNullableInt(dr, "IdTipoTramite"),
                            TipoTramite = GetNullableString(dr, "TipoTramite"),
                            Fecha = Convert.ToDateTime(dr["Fecha"]),
                            IdTarifario = GetNullableInt(dr, "IdTarifario"),
                            Penalidad = GetNullableDecimal(dr, "Penalidad"),
                            Precio = GetNullableDecimal(dr, "Precio"),
                            IdMoneda = GetNullableInt(dr, "IdMoneda"),
                            Moneda = GetNullableString(dr, "Moneda")
                        });
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

        // Endpoint nuevo, separado de ListarParaFacturacionAsync (que sigue apuntando a
        // SP_Pedido_ListarParaFacturacion, la versión desplegada que queda como fallback).
        public async Task<Respuesta> ListarParaFacturacionConGruposAsync(UsuarioGeneral usuarioLogueado, ListarPedidosFacturacionConGruposRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_ListarParaFacturacionConGrupos", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;
                cmd.Parameters.Add("@p_dtFchInicio", MySqlDbType.Date).Value = request.fchInicio.ToDateTime(TimeOnly.MinValue);
                cmd.Parameters.Add("@p_dtFchFin", MySqlDbType.Date).Value = request.fchFin.ToDateTime(TimeOnly.MinValue);
                cmd.Parameters.Add("@p_intIdTipoTramite", MySqlDbType.Int32).Value = (object?)request.idTipoTramite ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchIdPais", MySqlDbType.VarChar, 200).Value = request.idsPais is { Count: > 0 } idsPais ? (object)string.Join(",", idsPais) : DBNull.Value;
                cmd.Parameters.Add("@p_intIdMoneda", MySqlDbType.Int32).Value = (object?)request.idMoneda ?? DBNull.Value;
                cmd.Parameters.Add("@p_bitFinalizadoEnFecha", MySqlDbType.Bool).Value = (object?)request.finalizadoEnFecha ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoListaFacturacionConGruposResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.Pedidos.Add(new PedidoListaFacturacionConGruposConsulta
                        {
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                            NumReferencia = dr["NumReferencia"]?.ToString() ?? string.Empty,
                            Investigado = GetNullableString(dr, "Investigado"),
                            IdPais = GetNullableInt(dr, "IdPais"),
                            Pais = GetNullableString(dr, "Pais"),
                            IdTipoTramite = GetNullableInt(dr, "IdTipoTramite"),
                            TipoTramite = GetNullableString(dr, "TipoTramite"),
                            Fecha = Convert.ToDateTime(dr["Fecha"]),
                            IdTarifario = GetNullableInt(dr, "IdTarifario"),
                            Penalidad = GetNullableDecimal(dr, "Penalidad"),
                            Precio = GetNullableDecimal(dr, "Precio"),
                            IdMoneda = GetNullableInt(dr, "IdMoneda"),
                            Moneda = GetNullableString(dr, "Moneda"),
                            Vigencia = GetNullableBool(dr, "Vigencia"),
                            GroupId = Convert.ToInt32(dr["GroupId"])
                        });
                }

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.Grupos.Add(new GrupoFacturacionConsulta
                        {
                            GroupId = Convert.ToInt32(dr["GroupId"]),
                            Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                            Descripcion = dr["Descripcion"]?.ToString() ?? string.Empty,
                            Precio = Convert.ToDecimal(dr["Precio"]),
                            Descuento = Convert.ToDecimal(dr["Descuento"]),
                            Cantidad = Convert.ToInt32(dr["Cantidad"])
                        });
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
                    Result = new PedidoListaFacturacionConGruposResult()
                };
            }
        }

        public async Task<Respuesta> ListarPorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_ListarPorDocumentoElectronico", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdDocumentoElectronico", MySqlDbType.Int32).Value = idDocumentoElectronico;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoPorDocumentoElectronicoResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.Pedidos.Add(new PedidoPorDocumentoElectronicoConsulta
                        {
                            Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                            NumReferencia = dr["NumReferencia"]?.ToString() ?? string.Empty,
                            Investigado = GetNullableString(dr, "Investigado"),
                            TipoTramite = dr["TipoTramite"]?.ToString() ?? string.Empty,
                            Pais = dr["Pais"]?.ToString() ?? string.Empty,
                            ValorUnitario = dr["ValorUnitario"]?.ToString() ?? string.Empty,
                            Descuento = dr["Descuento"]?.ToString() ?? string.Empty
                        });
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
                    Result = new PedidoPorDocumentoElectronicoResult()
                };
            }
        }

        // Variante sin usuario logueado (flujo público por token, ver PedidoFacturaHandler.
        // ListarPedidosPorTokenPublicoAsync) — SP_Pedido_ListarPorDocumentoElectronicoPublico no valida
        // rol, solo toma idEmpresa/idDocumentoElectronico ya resueltos desde ms-facturación.
        public async Task<Respuesta> ListarPorDocumentoElectronicoPublicoAsync(int idEmpresa, int idDocumentoElectronico)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_ListarPorDocumentoElectronicoPublico", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = idEmpresa;
                cmd.Parameters.Add("@p_intIdDocumentoElectronico", MySqlDbType.Int32).Value = idDocumentoElectronico;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoPorDocumentoElectronicoResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        resultado.Pedidos.Add(new PedidoPorDocumentoElectronicoConsulta
                        {
                            Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                            NumReferencia = dr["NumReferencia"]?.ToString() ?? string.Empty,
                            Investigado = GetNullableString(dr, "Investigado"),
                            TipoTramite = dr["TipoTramite"]?.ToString() ?? string.Empty,
                            Pais = dr["Pais"]?.ToString() ?? string.Empty,
                            ValorUnitario = dr["ValorUnitario"]?.ToString() ?? string.Empty,
                            Descuento = dr["Descuento"]?.ToString() ?? string.Empty
                        });
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
                    Result = new PedidoPorDocumentoElectronicoResult()
                };
            }
        }

        public async Task<Respuesta> ListarParaPrefacturaAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoPrefactura request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Pedido_ListarParaPrefactura", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_dtFchInicio", MySqlDbType.Date).Value = (object?)request.FchInicio?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtFchFin", MySqlDbType.Date).Value = (object?)request.FchFin?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value;

                cmd.Parameters.Add("@p_jsonAnioMes", MySqlDbType.JSON).Value = ConstruirJsonListaAnioMes(request.Meses);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new PedidoPrefacturaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    // Por posición, no por nombre: el SP bifurca por idioma (headers distintos).
                    for (var i = 0; i < dr.FieldCount; i++)
                        resultado.Headers.Add(dr.GetName(i));

                    while (await dr.ReadAsync())
                    {
                        resultado.Items.Add(new PedidoPrefacturaConsulta
                        {
                            Nr = Convert.ToInt32(dr[0]),
                            ReferenceNumber = dr[1]?.ToString() ?? string.Empty,
                            Company = dr[2]?.ToString() ?? string.Empty,
                            DateOfRequest = dr[3]?.ToString() ?? string.Empty,
                            DeliveryDate = dr[4]?.ToString() ?? string.Empty,
                            Amount = Convert.ToDecimal(dr[5]),
                            Currency = dr[6]?.ToString() ?? string.Empty,
                            Status = dr[7]?.ToString() ?? string.Empty,
                            Country = dr[8]?.ToString() ?? string.Empty,
                            Observation = dr[9]?.ToString() ?? string.Empty
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
                    Result = new PedidoPrefacturaResult()
                };
            }
        }

    }
}
