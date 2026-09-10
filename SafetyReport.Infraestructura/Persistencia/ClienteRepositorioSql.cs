using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Cliente;
using SafetyReport.Application.Puertos.ClienteContacto;
using SafetyReport.Application.Puertos.PedidoFacturaLinea;
using SafetyReport.Application.Puertos.Informe;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class ClienteRepositorioSql : IClienteRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<ClienteRepositorioSql> _logger;

        public ClienteRepositorioSql(DbConfig dbConfig, ILogger<ClienteRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // Shape esperado por SP_ClienteContacto_InsertarLote (ver scripts-mysql/ClienteContacto.sql):
        // array de {Codigo, Nombres, IdTipoPersonaContacto, IdTipoContacto, AreaTrabajo, Telefono, Correo, EnviarCorreo}.
        // TipoContacto (solo de exhibición) no es consumido por el SP y se omite.
        private static string ConstruirJsonContactos(List<ClienteContactoRequest>? contactos) =>
            JsonSerializer.Serialize((contactos ?? new List<ClienteContactoRequest>()).Select(c => new
            {
                c.Codigo,
                Nombres = c.Nombres ?? string.Empty,
                c.IdTipoPersonaContacto,
                c.IdTipoContacto,
                c.AreaTrabajo,
                c.Telefono,
                c.Correo,
                c.EnviarCorreo
            }));

        // Shape esperado por SP_Cliente_Insertar/_Actualizar (p_json_formatos): array de {IdFormatoDocumento}.
        private static string ConstruirJsonFormatoDocumento(List<int>? ids) =>
            JsonSerializer.Serialize((ids ?? new List<int>()).Select(id => new { IdFormatoDocumento = id }));

        // Shape esperado por SP_Tarifario_InsertarLote (ver scripts-mysql/05_Tarifario.sql):
        // array de {IdProducto, IdTipoTramite, IdPais, IdMoneda, DiasMax, DiasMin, Precio, Penalidad}.
        private static string ConstruirJsonTarifario(List<ClienteTarifarioRequest>? tarifas) =>
            JsonSerializer.Serialize((tarifas ?? new List<ClienteTarifarioRequest>()).Select(t => new
            {
                t.IdProducto,
                t.IdTipoTramite,
                t.IdPais,
                t.IdMoneda,
                t.DiasMax,
                t.DiasMin,
                t.Precio,
                t.Penalidad
            }));

        private static int? GetNullableInt(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static string? GetNullableString(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private static decimal? GetNullableDecimal(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : Convert.ToDecimal(value);
        }

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

        private static async Task<List<T>> LeerIdsAsync<T>(MySqlDataReader dr, string columnName, Func<int?, T> factory)
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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@p_intIdTipoPersona", MySqlDbType.Int32).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@p_vchNombre", MySqlDbType.VarChar).Value = request.Nombre;
                cmd.Parameters.Add("@p_vchNombreCorto", MySqlDbType.VarChar, 512).Value = (object?)request.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = request.IdPais;
                cmd.Parameters.Add("@p_intIdRegistroTributario", MySqlDbType.Int32).Value = request.IdRegistroTributario;
                cmd.Parameters.Add("@p_vchNumRegistroTributario", MySqlDbType.VarChar, 50).Value = (object?)request.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCorreo", MySqlDbType.VarChar, 50).Value = (object?)request.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchWebSite", MySqlDbType.VarChar, 200).Value = (object?)request.WebSite ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchTelefono", MySqlDbType.VarChar, 32).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchFax", MySqlDbType.VarChar, 50).Value = (object?)request.Fax ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchDireccion", MySqlDbType.VarChar, 512).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchRecomendacion", MySqlDbType.VarChar).Value = (object?)request.Recomendacion ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEmpresaAtencion", MySqlDbType.Int32).Value = request.IdEmpresaAtencion;
                cmd.Parameters.Add("@p_intIdIdioma", MySqlDbType.Int32).Value = request.IdIdioma;
                cmd.Parameters.Add("@p_vchLogoClienteUrl", MySqlDbType.VarChar).Value = (object?)request.LogoClienteUrl ?? DBNull.Value;
                cmd.Parameters.Add("@p_bitImprimeLogoSafety", MySqlDbType.Bool).Value = request.ImprimeLogoSafety;
                cmd.Parameters.Add("@p_intIdMoneda", MySqlDbType.Int32).Value = request.IdMoneda;
                cmd.Parameters.Add("@p_intIdIdiomaFacturacion", MySqlDbType.Int32).Value = request.IdIdiomaFacturacion;
                cmd.Parameters.Add("@p_bitAplicaPenalidad", MySqlDbType.Bool).Value = request.AplicaPenalidad;
                cmd.Parameters.Add("@p_intIdPlantilla", MySqlDbType.Int32).Value = request.IdPlantilla;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = request.IdEstado;
                cmd.Parameters.Add("@p_bitEmitirPrefactura", MySqlDbType.Bool).Value = request.EmitirPrefactura;

                cmd.Parameters.Add("@p_json_formatos", MySqlDbType.JSON).Value = ConstruirJsonFormatoDocumento(request.LstIdFormatoDocumento);
                cmd.Parameters.Add("@p_json_contactos", MySqlDbType.JSON).Value = ConstruirJsonContactos(request.Contactos);
                cmd.Parameters.Add("@p_json_tarifario", MySqlDbType.JSON).Value = ConstruirJsonTarifario(request.Tarifario);

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

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_intIdTipoPersona", MySqlDbType.Int32).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@p_vchNombre", MySqlDbType.VarChar).Value = request.Nombre;
                cmd.Parameters.Add("@p_vchNombreCorto", MySqlDbType.VarChar, 512).Value = (object?)request.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = request.IdPais;
                cmd.Parameters.Add("@p_intIdRegistroTributario", MySqlDbType.Int32).Value = request.IdRegistroTributario;
                cmd.Parameters.Add("@p_vchNumRegistroTributario", MySqlDbType.VarChar, 50).Value = (object?)request.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCorreo", MySqlDbType.VarChar, 50).Value = (object?)request.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchWebSite", MySqlDbType.VarChar, 200).Value = (object?)request.WebSite ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchTelefono", MySqlDbType.VarChar, 32).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchFax", MySqlDbType.VarChar, 50).Value = (object?)request.Fax ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchDireccion", MySqlDbType.VarChar, 512).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchRecomendacion", MySqlDbType.VarChar).Value = (object?)request.Recomendacion ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEmpresaAtencion", MySqlDbType.Int32).Value = request.IdEmpresaAtencion;
                cmd.Parameters.Add("@p_intIdIdioma", MySqlDbType.Int32).Value = request.IdIdioma;
                cmd.Parameters.Add("@p_vchLogoClienteUrl", MySqlDbType.VarChar).Value = (object?)request.LogoClienteUrl ?? DBNull.Value;
                cmd.Parameters.Add("@p_bitImprimeLogoSafety", MySqlDbType.Bool).Value = request.ImprimeLogoSafety;
                cmd.Parameters.Add("@p_intIdMoneda", MySqlDbType.Int32).Value = request.IdMoneda;
                cmd.Parameters.Add("@p_intIdIdiomaFacturacion", MySqlDbType.Int32).Value = request.IdIdiomaFacturacion;
                cmd.Parameters.Add("@p_bitAplicaPenalidad", MySqlDbType.Bool).Value = request.AplicaPenalidad;
                cmd.Parameters.Add("@p_intIdPlantilla", MySqlDbType.Int32).Value = request.IdPlantilla;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = request.IdEstado;
                cmd.Parameters.Add("@p_bitEmitirPrefactura", MySqlDbType.Bool).Value = request.EmitirPrefactura;

                cmd.Parameters.Add("@p_json_formatos", MySqlDbType.JSON).Value = ConstruirJsonFormatoDocumento(request.LstIdFormatoDocumento);

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = idCliente;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdDocumentoElectronico", MySqlDbType.Int32).Value = idDocumentoElectronico;

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

        // Payload reducido de ObtenerClientePorDocumentoElectronicoAsync + las líneas vivas del documento
        // (PEDIDO_FACTURA_LINEA, no el snapshot ya persistido en ms-facturación) — insumo para reabrir un
        // borrador mostrando el estado actual — ver SP_Cliente_ObtenerConLineasPorDocumentoElectronico.
        public async Task<Respuesta> ObtenerConLineasPorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_ObtenerConLineasPorDocumentoElectronico", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdDocumentoElectronico", MySqlDbType.Int32).Value = idDocumentoElectronico;

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    var cliente = new ClienteConLineasConsulta
                    {
                        IdCliente = Convert.ToInt32(dr["IdCliente"]),
                        IdTipoDocumentoSunat = dr["IdTipoDocumentoSunat"] is DBNull ? null : Convert.ToInt32(dr["IdTipoDocumentoSunat"])
                    };

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            cliente.Lineas.Add(new PedidoFacturaLineaParaBorradorConsulta
                            {
                                IdPedidoFacturaLinea = Convert.ToInt32(dr["IdPedidoFacturaLinea"]),
                                IdDocumentoElectronico = GetNullableInt(dr, "IdDocumentoElectronico"),
                                Codigo = dr["Codigo"] as string,
                                Descripcion = Convert.ToString(dr["Descripcion"]) ?? string.Empty,
                                Cantidad = Convert.ToInt32(dr["Cantidad"]),
                                ValorUnitario = Convert.ToDecimal(dr["ValorUnitario"]),
                                Descuento = Convert.ToDecimal(dr["Descuento"])
                            });
                        }
                    }

                    respuesta.Result = cliente;
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
                    Result = new ClienteConLineasConsulta()
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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = (object?)idPais ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = (object?)idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)numPag ?? DBNull.Value;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = idCliente;

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

        public async Task<Respuesta> ActivarDesactivarClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente, int idEstado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_ActivarDesactivar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = idCliente;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = idEstado;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new List<ClienteEstadoActualizado>();

                    while (await dr.ReadAsync())
                    {
                        resultado.Add(new ClienteEstadoActualizado
                        {
                            IdCliente = GetNullableInt(dr, "IdCliente") ?? 0,
                            IdEstado = GetNullableInt(dr, "IdEstado") ?? 0
                        });
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new List<ClienteEstadoActualizado>();
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
                    Result = new List<ClienteEstadoActualizado>()
                };
            }
        }

        public async Task<Respuesta> ListarClienteShortAsync(UsuarioGeneral usuarioLogueado, string? correoBusqueda)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_ListaCorta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchCorreoBusqueda", MySqlDbType.VarChar, 100).Value = (object?)correoBusqueda ?? DBNull.Value;

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

        public async Task<Respuesta> ListarClientesFacturacionAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? emitirPrefactura, int? idIdiomaFacturacion)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_ListarFacturacion", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)numPag ?? DBNull.Value;
                cmd.Parameters.Add("@p_intEmitirPrefactura", MySqlDbType.Int32).Value = (object?)emitirPrefactura ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdIdiomaFacturacion", MySqlDbType.Int32).Value = (object?)idIdiomaFacturacion ?? DBNull.Value;

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
                                IdIdiomaFacturacion = GetNullableString(dr, "IdIdiomaFacturacion")
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

        public async Task<Respuesta> ObtenerResumenClientesAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Cliente_Resumen", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

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
