using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class ClienteDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<ClienteDAO> _logger;

        public ClienteDAO(DbConfig dbConfig, ILogger<ClienteDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static int? GetNullableInt(DbDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static string? GetNullableString(DbDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private static decimal? GetNullableDecimal(DbDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : Convert.ToDecimal(value);
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

        private static async Task<List<T>> LeerIdsAsync<T>(DbDataReader dr, string columnName, Func<int?, T> factory)
        {
            var lista = new List<T>();

            while (await dr.ReadAsync())
            {
                lista.Add(factory(GetNullableInt(dr, columnName)));
            }

            return lista;
        }

        public async Task<Respuesta> CrearClienteAsync(UsuarioGeneral usuarioLogueado, Cliente request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                cmd.Parameters.AddWithValue("@intIdTipoPersona", request.IdTipoPersona);
                cmd.Parameters.AddWithValue("@vchNombre", request.Nombre);
                cmd.Parameters.AddWithValue("@vchNombreCorto", (object?)request.NombreCorto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPais", request.IdPais);
                cmd.Parameters.AddWithValue("@intIdRegistroTributario", request.IdRegistroTributario);
                cmd.Parameters.AddWithValue("@vchNumRegistroTributario", (object?)request.NumRegistroTributario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchCorreo", (object?)request.Correo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchWebSite", (object?)request.WebSite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchTelefono", (object?)request.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchFax", (object?)request.Fax ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchDireccion", (object?)request.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchRecomendacion", (object?)request.Recomendacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEmpresaAtencion", request.IdEmpresaAtencion);
                cmd.Parameters.AddWithValue("@intIdIdioma", request.IdIdioma);
                cmd.Parameters.AddWithValue("@vchLogoClienteUrl", (object?)request.LogoClienteUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bitImprimeLogoSafety", request.ImprimeLogoSafety);
                cmd.Parameters.AddWithValue("@intIdMoneda", request.IdMoneda);
                cmd.Parameters.AddWithValue("@intIdIdiomaFacturacion", request.IdIdiomaFacturacion);
                cmd.Parameters.AddWithValue("@bitAplicaPenalidad", request.AplicaPenalidad);
                cmd.Parameters.AddWithValue("@intIdPlantilla", request.IdPlantilla);
                cmd.Parameters.AddWithValue("@intIdEstado", request.IdEstado);
                cmd.Parameters.AddWithValue("@bitEmitirPrefactura", request.EmitirPrefactura);
                cmd.Parameters.AddWithValue("@lstIdFormatoDocumento", JsonSerializer.Serialize(request.LstIdFormatoDocumento ?? new List<int>()));
                cmd.Parameters.AddWithValue("@lstContactos", JsonSerializer.Serialize(request.Contactos ?? new List<ClienteContactoRequest>()));
                cmd.Parameters.AddWithValue("@lstTarifario", JsonSerializer.Serialize(request.Tarifario ?? new List<ClienteTarifarioRequest>()));

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCliente", id => new ClienteCreado { IdCliente = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<ClienteCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteCreado>()
                };
            }
        }

        public async Task<Respuesta> EditarClienteAsync(UsuarioGeneral usuarioLogueado, EditarCliente request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                cmd.Parameters.AddWithValue("@intIdCliente", request.IdCliente);
                cmd.Parameters.AddWithValue("@intIdTipoPersona", request.IdTipoPersona);
                cmd.Parameters.AddWithValue("@vchNombre", request.Nombre);
                cmd.Parameters.AddWithValue("@vchNombreCorto", (object?)request.NombreCorto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPais", request.IdPais);
                cmd.Parameters.AddWithValue("@intIdRegistroTributario", request.IdRegistroTributario);
                cmd.Parameters.AddWithValue("@vchNumRegistroTributario", (object?)request.NumRegistroTributario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchCorreo", (object?)request.Correo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchWebSite", (object?)request.WebSite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchTelefono", (object?)request.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchFax", (object?)request.Fax ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchDireccion", (object?)request.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchRecomendacion", (object?)request.Recomendacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEmpresaAtencion", request.IdEmpresaAtencion);
                cmd.Parameters.AddWithValue("@intIdIdioma", request.IdIdioma);
                cmd.Parameters.AddWithValue("@vchLogoClienteUrl", (object?)request.LogoClienteUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bitImprimeLogoSafety", request.ImprimeLogoSafety);
                cmd.Parameters.AddWithValue("@intIdMoneda", request.IdMoneda);
                cmd.Parameters.AddWithValue("@intIdIdiomaFacturacion", request.IdIdiomaFacturacion);
                cmd.Parameters.AddWithValue("@bitAplicaPenalidad", request.AplicaPenalidad);
                cmd.Parameters.AddWithValue("@intIdPlantilla", request.IdPlantilla);
                cmd.Parameters.AddWithValue("@intIdEstado", request.IdEstado);
                cmd.Parameters.AddWithValue("@bitEmitirPrefactura", request.EmitirPrefactura);
                cmd.Parameters.AddWithValue("@lstIdFormatoDocumento", JsonSerializer.Serialize(request.LstIdFormatoDocumento ?? new List<int>()));

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCliente", id => new ClienteCreado { IdCliente = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<ClienteCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", idCliente);

                await cn.OpenAsync();
                return await LeerClienteConsultaAsync(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteConsulta>()
                };
            }
        }

        // Mismo resultado que ObtenerClienteAsync, pero resuelto por IdDocumentoElectronico (la factura) en
        // vez de un IdCliente directo — ver SP_Cliente_ObtenerPorDocumentoElectronico.
        public async Task<Respuesta> ObtenerClientePorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_ObtenerPorDocumentoElectronico", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdDocumentoElectronico", idDocumentoElectronico);

                await cn.OpenAsync();
                return await LeerClienteConsultaAsync(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteConsulta>()
                };
            }
        }

        // Lectura compartida por ObtenerClienteAsync/ObtenerClientePorDocumentoElectronicoAsync — ambos SPs
        // devuelven exactamente el mismo shape (cabecera, cliente, formatos de documento), solo cambia cómo
        // se resuelve el cliente del lado del SP.
        private async Task<Respuesta> LeerClienteConsultaAsync(MySqlCommand cmd)
        {
            using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<ClienteConsulta>();

                    if (await dr.ReadAsync())
                    {
                        lista.Add(new ClienteConsulta
                        {
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            IdTipoPersona = Convert.ToInt32(dr["IdTipoPersona"]),
                            Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                            NombreCorto = GetNullableString(dr, "NombreCorto"),
                            IdPais = Convert.ToInt32(dr["IdPais"]),
                            IdRegistroTributario = Convert.ToInt32(dr["IdRegistroTributario"]),
                            NumRegistroTributario = GetNullableString(dr, "NumRegistroTributario"),
                            IdTipoDocumentoSunat = dr["IdTipoDocumentoSunat"] is DBNull ? null : Convert.ToInt32(dr["IdTipoDocumentoSunat"]),
                            Correo = GetNullableString(dr, "Correo"),
                            WebSite = GetNullableString(dr, "WebSite"),
                            Telefono = GetNullableString(dr, "Telefono"),
                            Fax = GetNullableString(dr, "Fax"),
                            Direccion = GetNullableString(dr, "Direccion"),
                            Recomendacion = GetNullableString(dr, "Recomendacion"),
                            IdEmpresaAtencion = Convert.ToInt32(dr["IdEmpresaAtencion"]),
                            IdIdioma = Convert.ToInt32(dr["IdIdioma"]),
                            LogoClienteUrl = GetNullableString(dr, "LogoClienteUrl"),
                            ImprimeLogoSafety = Convert.ToBoolean(dr["ImprimeLogoSafety"]),
                            IdMoneda = Convert.ToInt32(dr["IdMoneda"]),
                            IdIdiomaFacturacion = Convert.ToInt32(dr["IdIdiomaFacturacion"]),
                            AplicaPenalidad = Convert.ToBoolean(dr["AplicaPenalidad"]),
                            IdPlantilla = Convert.ToInt32(dr["IdPlantilla"]),
                            IdEstado = Convert.ToInt32(dr["IdEstado"]),
                            EmitirPrefactura = Convert.ToBoolean(dr["EmitirPrefactura"])
                        });
                    }

                    if (lista.Count > 0 && await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var id = GetNullableInt(dr, "IdFormatoDocumento");
                            if (id.HasValue)
                            {
                                lista[0].LstIdFormatoDocumento.Add(id.Value);
                            }
                        }
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<ClienteConsulta>();
                }

                return respuesta;
        }

        public async Task<Respuesta> ListarClientesAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? idPais, int? idEstado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPais", (object?)idPais ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEstado", (object?)idEstado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new ClienteListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstClientes.Add(new ClienteListaConsulta
                            {
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                                Correo = GetNullableString(dr, "Correo"),
                                Telefono = GetNullableString(dr, "Telefono"),
                                Pais = dr["Pais"]?.ToString() ?? string.Empty,
                                TipoPersona = dr["TipoPersona"]?.ToString() ?? string.Empty,
                                Estado = dr["Estado"]?.ToString() ?? string.Empty
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new ClienteListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", idCliente);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCliente", id => new ClienteEliminado { IdCliente = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<ClienteEliminado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteEliminado>()
                };
            }
        }

        public async Task<Respuesta> ListarClienteShortAsync(UsuarioGeneral usuarioLogueado, string? correoBusqueda)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Listar_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchCorreoBusqueda", (object?)correoBusqueda ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new ClienteListaCorta();

                    while (await dr.ReadAsync())
                    {
                        resultado.lstCliente.Add(new ClienteListaCortaItem
                        {
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            NumeroDocumento = GetNullableString(dr, "NumeroDocumento") ?? string.Empty,
                            NombreCliente = dr["NombreCliente"]?.ToString() ?? string.Empty,
                            IdIdioma = Convert.ToInt32(dr["IdIdioma"]),
                            LogoImprimible = Convert.ToBoolean(dr["LogoImprimible"]),
                            IdPlantilla = Convert.ToInt32(dr["IdPlantilla"])
                        });
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new ClienteListaCorta();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteListaCorta()
                };
            }
        }

        public async Task<Respuesta> ListarClientesFacturacionAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? emitirPrefactura, int? idIdiomaFacturacion, int? estadoFacturacion)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_ListarFacturacion", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)numPag ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intEmitirPrefactura", (object?)emitirPrefactura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdIdiomaFacturacion", (object?)idIdiomaFacturacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intEstadoFacturacion", (object?)estadoFacturacion ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new ClienteListaFacturacionResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstClientes.Add(new ClienteListaFacturacionConsulta
                            {
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                                EmitirPrefactura = GetNullableString(dr, "EmitirPrefactura"),
                                TotalPedidos = Convert.ToInt32(dr["TotalPedidos"]),
                                PedidosFacturados = Convert.ToInt32(dr["PedidosFacturados"]),
                                IdIdiomaFacturacion = GetNullableString(dr, "IdIdiomaFacturacion"),
                                EstadoFacturacion = GetNullableString(dr, "EstadoFacturacion"),
                                ColorTexto = GetNullableString(dr, "ColorTexto"),
                                ColorFondo = GetNullableString(dr, "ColorFondo")
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new ClienteListaFacturacionResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteListaFacturacionResult()
                };
            }
        }

        public async Task<Respuesta> ListarPedidosFacturacionClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente, string? busqueda, int? numPag)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_ListarPedidosFacturacion", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdCliente", idCliente);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new ClientePedidosFacturacionResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstPedidos.Add(new ClientePedidoFacturacionConsulta
                            {
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                Codigo = dr["Codigo"]?.ToString() ?? string.Empty,
                                Investigado = GetNullableString(dr, "Investigado"),
                                AplicaPenalidad = GetNullableString(dr, "AplicaPenalidad"),
                                EstadoFacturacion = dr["EstadoFacturacion"]?.ToString() ?? string.Empty
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new ClientePedidosFacturacionResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClientePedidosFacturacionResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerResumenClientesAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Resumen", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new ClienteResumen();
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
                    Result = new ClienteResumen()
                };
            }
        }
    }
}