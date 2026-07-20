using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;
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

        private static DataTable ConstruirTablaListaGeneralNum(List<int>? valores)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("NUM1", typeof(int));

            int i = 1;
            if (valores != null)
            {
                foreach (var valor in valores)
                    table.Rows.Add(i++, valor);
            }

            return table;
        }

        private async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
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
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", cmd.CommandText);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                respuesta.Result = new List<T>();
            }

            return respuesta;
        }

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje. Sin columna Result.
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

        private static int? GetNullableInt(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static bool? GetNullableBool(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToBoolean(dr[columna]);

        private static DateTime? GetNullableDateTime(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDateTime(dr[columna]);

        private static string? GetNullableString(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Pedido request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 50).Value = request.Codigo;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCliente", SqlDbType.VarChar, 255).Value = (object?)request.NombreCliente ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@intIdEmpresaAtencion", SqlDbType.Int).Value = request.IdEmpresaAtencion;
                cmd.Parameters.Add("@vchNumeroDocumentoInvestigado", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@vchInvestigarRazonSocialNombres", SqlDbType.VarChar).Value = request.InvestigarRazonSocialNombres;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.IdTarifario;
                cmd.Parameters.Add("@intIdPlantilla", SqlDbType.Int).Value = request.IdPlantilla;
                cmd.Parameters.Add("@intIdIdioma", SqlDbType.Int).Value = request.IdIdioma;
                cmd.Parameters.Add("@intIdClaseInforme", SqlDbType.Int).Value = request.IdClaseInforme;
                cmd.Parameters.Add("@vchNumReferencia", SqlDbType.VarChar, 32).Value = (object?)request.NumReferencia ?? DBNull.Value;

                cmd.Parameters.Add("@decMontoCredito", SqlDbType.Decimal).Value = (object?)request.MontoCredito ?? DBNull.Value;
                cmd.Parameters["@decMontoCredito"].Precision = 18;
                cmd.Parameters["@decMontoCredito"].Scale = 2;

                cmd.Parameters.Add("@intPlazoCredito", SqlDbType.Int).Value = (object?)request.PlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPlazoCredito", SqlDbType.Int).Value = (object?)request.IdTipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@vchTipoPlazoCredito", SqlDbType.VarChar).Value = (object?)request.TipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchDesde", SqlDbType.DateTime).Value = (object?)request.FchDesde ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchHasta", SqlDbType.DateTime).Value = (object?)request.FchHasta ?? DBNull.Value;
                cmd.Parameters.Add("@vchComentario", SqlDbType.VarChar).Value = (object?)request.Comentario ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;
                cmd.Parameters.Add("@bitImprimeLogoSafety", SqlDbType.Bit).Value = request.ImprimeLogoSafety;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.IdPedido;
                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 50).Value = request.Codigo;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCliente", SqlDbType.VarChar, 255).Value = (object?)request.NombreCliente ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = request.IdCompania;
                cmd.Parameters.Add("@vchNumeroDocumentoInvestigado", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@vchInvestigarRazonSocialNombres", SqlDbType.VarChar).Value = request.InvestigarRazonSocialNombres;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.IdTarifario;
                cmd.Parameters.Add("@intIdPlantilla", SqlDbType.Int).Value = request.IdPlantilla;
                cmd.Parameters.Add("@intIdIdioma", SqlDbType.Int).Value = request.IdIdioma;
                cmd.Parameters.Add("@intIdClaseInforme", SqlDbType.Int).Value = request.IdClaseInforme;
                cmd.Parameters.Add("@vchNumReferencia", SqlDbType.VarChar, 32).Value = (object?)request.NumReferencia ?? DBNull.Value;

                cmd.Parameters.Add("@decMontoCredito", SqlDbType.Decimal).Value = (object?)request.MontoCredito ?? DBNull.Value;
                cmd.Parameters["@decMontoCredito"].Precision = 18;
                cmd.Parameters["@decMontoCredito"].Scale = 2;

                cmd.Parameters.Add("@intPlazoCredito", SqlDbType.Int).Value = (object?)request.PlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPlazoCredito", SqlDbType.Int).Value = (object?)request.IdTipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@vchTipoPlazoCredito", SqlDbType.VarChar).Value = (object?)request.TipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchDesde", SqlDbType.DateTime).Value = (object?)request.FchDesde ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchHasta", SqlDbType.DateTime).Value = (object?)request.FchHasta ?? DBNull.Value;
                cmd.Parameters.Add("@vchComentario", SqlDbType.VarChar).Value = (object?)request.Comentario ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;
                cmd.Parameters.Add("@bitImprimeLogoSafety", SqlDbType.Bit).Value = request.ImprimeLogoSafety;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = (object?)request.idPedido ?? DBNull.Value;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = (object?)request.idCliente ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = (object?)request.idTarifario ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreInvestigado", SqlDbType.VarChar, 255).Value = (object?)request.nombreInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@vchNumRef", SqlDbType.VarChar, 50).Value = (object?)request.numRef ?? DBNull.Value;

                var tableIdEstado = ConstruirTablaListaGeneralNum(request.idEstado);
                var tvpIdEstado = cmd.Parameters.AddWithValue("@lstIdEstado", tableIdEstado);
                tvpIdEstado.SqlDbType = SqlDbType.Structured;
                tvpIdEstado.TypeName = "LISTA_GENERAL_NUM";

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = (object?)request.idCliente ?? DBNull.Value;
                cmd.Parameters.Add("@vchIdEstado", SqlDbType.VarChar).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_PedidoAsignacion_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = (object?)request.idPedido ?? DBNull.Value;
                cmd.Parameters.Add("@vchIdEstado", SqlDbType.VarChar).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@IdEstadoAsignacion", SqlDbType.Int).Value = (object?)request.idEstadoAsignacion ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_Cancelar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Pedido_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;

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

    }
}
