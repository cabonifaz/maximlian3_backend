using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

namespace SafetyReport.DAO
{
    public class ClienteDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<ClienteDAO> _logger;
        private readonly IConfiguration _configuration;

        public ClienteDAO(DbConfig dbConfig, ILogger<ClienteDAO> logger, IConfiguration configuration)
        {
            _dbConfig = dbConfig;
            _logger = logger;
            _configuration = configuration;
        }

        // Segunda conexión, a la base de ms-facturacion (maximilian_facturacion_staging) — usada solo para
        // sincronizar el Cliente vía SP_Cliente_InsertarDesdeBackend/SP_Cliente_ActualizarDesdeBackend.
        private string MsFacturacionConnectionString =>
            _configuration.GetConnectionString("MsFacturacionConnection")
                ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'MsFacturacionConnection'.");

        // Best-effort: ms-facturacion es un sistema secundario (facturación electrónica), no hay transacción
        // distribuida entre las dos bases — si esta sincronización falla, se loguea pero NO se revierte ni se
        // reporta como error el alta/edición del Cliente en maximlian3_staging, que ya se confirmó exitosa.
        // IdInquilino (ms-facturacion) = IdEmpresa (backend): 1 Empresa del backend = 1 Inquilino allá.
        private async Task SincronizarClienteFacturacionInsertarAsync(
            UsuarioGeneral usuarioLogueado, int idRegistroTributario, string? numRegistroTributario,
            string nombre, string? correo, string? direccion, int idPais)
        {
            try
            {
                using SqlConnection cn = new(MsFacturacionConnectionString);
                using SqlCommand cmd = new("SP_Cliente_InsertarDesdeBackend", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@vchUsuarioEjecutor", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdInquilino", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRegistroTributario", SqlDbType.Int).Value = idRegistroTributario;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 15).Value = (object?)numRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar, 255).Value = nombre;
                cmd.Parameters.Add("@vchCorreo", SqlDbType.VarChar, 255).Value = (object?)correo ?? DBNull.Value;
                cmd.Parameters.Add("@vchDireccion", SqlDbType.VarChar, 255).Value = (object?)direccion ?? DBNull.Value;
                cmd.Parameters.Add("@intPaisCodigo", SqlDbType.Int).Value = idPais;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo sincronizar el alta del Cliente con ms-facturacion: {Mensaje}", respuesta.Mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al sincronizar el alta del Cliente con ms-facturacion.");
            }
        }

        // numeroDocumentoAnterior = NumRegistroTributario vigente ANTES de esta edición (obtenido de
        // SP_Cliente_Obtener justo antes de aplicar el cambio) — identifica la fila en ms-facturacion, que
        // nunca guarda el IdCliente del backend. numRegistroTributario es el valor nuevo (puede o no cambiar).
        private async Task SincronizarClienteFacturacionActualizarAsync(
            UsuarioGeneral usuarioLogueado, string? numeroDocumentoAnterior, int idRegistroTributario,
            string? numRegistroTributario, string nombre, string? correo, string? direccion, int idPais)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumentoAnterior))
            {
                _logger.LogWarning(
                    "No se pudo sincronizar la edición del Cliente con ms-facturacion: no se encontró el número de documento anterior.");
                return;
            }

            try
            {
                using SqlConnection cn = new(MsFacturacionConnectionString);
                using SqlCommand cmd = new("SP_Cliente_ActualizarDesdeBackend", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@vchUsuarioEjecutor", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdInquilino", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@vchNumeroDocumentoAnterior", SqlDbType.VarChar, 15).Value = numeroDocumentoAnterior;
                cmd.Parameters.Add("@intIdRegistroTributario", SqlDbType.Int).Value = idRegistroTributario;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 15).Value = (object?)numRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar, 255).Value = nombre;
                cmd.Parameters.Add("@vchCorreo", SqlDbType.VarChar, 255).Value = (object?)correo ?? DBNull.Value;
                cmd.Parameters.Add("@vchDireccion", SqlDbType.VarChar, 255).Value = (object?)direccion ?? DBNull.Value;
                cmd.Parameters.Add("@intPaisCodigo", SqlDbType.Int).Value = idPais;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje != 2)
                {
                    _logger.LogWarning(
                        "No se pudo sincronizar la edición del Cliente con ms-facturacion: {Mensaje}", respuesta.Mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al sincronizar la edición del Cliente con ms-facturacion.");
            }
        }

        private static DataTable ConstruirTablaContactos(List<ClienteContactoRequest>? contactos)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("CODIGO", typeof(string));
            table.Columns.Add("NOMBRES", typeof(string));
            table.Columns.Add("IDTIPOPERSONACONTACTO", typeof(int));
            table.Columns.Add("IDTIPOCONTACTO", typeof(int));
            table.Columns.Add("TIPOCONTACTO", typeof(string));
            table.Columns.Add("AREATRABAJO", typeof(int));
            table.Columns.Add("TELEFONO", typeof(string));
            table.Columns.Add("CORREO", typeof(string));
            table.Columns.Add("ENVIARCORREO", typeof(bool));

            int i = 1;

            if (contactos != null)
            {
                foreach (var contacto in contactos)
                {
                    table.Rows.Add(
                        i++,
                        (object?)contacto.Codigo ?? DBNull.Value,
                        contacto.Nombres ?? string.Empty,
                        contacto.IdTipoPersonaContacto,
                        contacto.IdTipoContacto,
                        contacto.TipoContacto,
                        contacto.AreaTrabajo,
                        (object?)contacto.Telefono ?? DBNull.Value,
                        (object?)contacto.Correo ?? DBNull.Value,
                        contacto.EnviarCorreo
                    );
                }
            }

            return table;
        }

        private static DataTable ConstruirTablaFormatoDocumento(List<int>? ids)
        {
            var table = new DataTable();
            table.Columns.Add("IdFormatoDocumento", typeof(int));

            if (ids != null)
            {
                foreach (var id in ids)
                    table.Rows.Add(id);
            }

            return table;
        }

        private static DataTable ConstruirTablaTarifario(List<ClienteTarifarioRequest>? tarifas)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("IDPRODUCTO", typeof(int));
            table.Columns.Add("IDTIPOTRAMITE", typeof(int));
            table.Columns.Add("IDPAIS", typeof(int));
            table.Columns.Add("IDMONEDA", typeof(int));
            table.Columns.Add("DIASMAX", typeof(int));
            table.Columns.Add("DIASMIN", typeof(int));
            table.Columns.Add("PRECIO", typeof(decimal));
            table.Columns.Add("PENALIDAD", typeof(decimal));

            int i = 1;

            if (tarifas != null)
            {
                foreach (var tarifa in tarifas)
                {
                    table.Rows.Add(
                        i++,
                        tarifa.IdProducto,
                        tarifa.IdTipoTramite,
                        tarifa.IdPais,
                        tarifa.IdMoneda,
                        tarifa.DiasMax,
                        tarifa.DiasMin,
                        tarifa.Precio,
                        tarifa.Penalidad
                    );
                }
            }

            return table;
        }

        private static int? GetNullableInt(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static string? GetNullableString(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
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

        private static async Task<List<T>> LeerIdsAsync<T>(SqlDataReader dr, string columnName, Func<int?, T> factory)
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar).Value = request.Nombre;
                cmd.Parameters.Add("@vchNombreCorto", SqlDbType.VarChar, 512).Value = (object?)request.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@intIdRegistroTributario", SqlDbType.Int).Value = request.IdRegistroTributario;
                cmd.Parameters.Add("@vchNumRegistroTributario", SqlDbType.VarChar, 50).Value = (object?)request.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@vchCorreo", SqlDbType.VarChar, 50).Value = (object?)request.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@vchWebSite", SqlDbType.VarChar, 200).Value = (object?)request.WebSite ?? DBNull.Value;
                cmd.Parameters.Add("@vchTelefono", SqlDbType.VarChar, 32).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@vchFax", SqlDbType.VarChar, 50).Value = (object?)request.Fax ?? DBNull.Value;
                cmd.Parameters.Add("@vchDireccion", SqlDbType.VarChar, 512).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@vchRecomendacion", SqlDbType.VarChar).Value = (object?)request.Recomendacion ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEmpresaAtencion", SqlDbType.Int).Value = request.IdEmpresaAtencion;
                cmd.Parameters.Add("@intIdIdioma", SqlDbType.Int).Value = request.IdIdioma;
                cmd.Parameters.Add("@vchLogoClienteUrl", SqlDbType.VarChar).Value = (object?)request.LogoClienteUrl ?? DBNull.Value;
                cmd.Parameters.Add("@bitImprimeLogoSafety", SqlDbType.Bit).Value = request.ImprimeLogoSafety;
                cmd.Parameters.Add("@intIdMoneda", SqlDbType.Int).Value = request.IdMoneda;
                cmd.Parameters.Add("@intIdIdiomaFacturacion", SqlDbType.Int).Value = request.IdIdiomaFacturacion;
                cmd.Parameters.Add("@bitAplicaPenalidad", SqlDbType.Bit).Value = request.AplicaPenalidad;
                cmd.Parameters.Add("@intIdPlantilla", SqlDbType.Int).Value = request.IdPlantilla;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;
                cmd.Parameters.Add("@bitEmitirPrefactura", SqlDbType.Bit).Value = request.EmitirPrefactura;

                var tableFormatoDocumento = ConstruirTablaFormatoDocumento(request.LstIdFormatoDocumento);
                var tvpFormatoDocumento = cmd.Parameters.AddWithValue("@lstIdFormatoDocumento", tableFormatoDocumento);
                tvpFormatoDocumento.SqlDbType = SqlDbType.Structured;
                tvpFormatoDocumento.TypeName = "LISTA_CLIENTE_FORMATO_DOCUMENTO";

                var tableContactos = ConstruirTablaContactos(request.Contactos);
                var tvpContactos = cmd.Parameters.AddWithValue("@lstContactos", tableContactos);
                tvpContactos.SqlDbType = SqlDbType.Structured;
                tvpContactos.TypeName = "LISTA_CLIENTE_CONTACTO";

                var tableTarifario = ConstruirTablaTarifario(request.Tarifario);
                var tvpTarifario = cmd.Parameters.AddWithValue("@lstTarifario", tableTarifario);
                tvpTarifario.SqlDbType = SqlDbType.Structured;
                tvpTarifario.TypeName = "LISTA_CLIENTE_TARIFARIO";

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

                if (respuesta.IdTipoMensaje == 2)
                {
                    await SincronizarClienteFacturacionInsertarAsync(
                        usuarioLogueado, request.IdRegistroTributario, request.NumRegistroTributario,
                        request.Nombre, request.Correo, request.Direccion, request.IdPais);
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar).Value = request.Nombre;
                cmd.Parameters.Add("@vchNombreCorto", SqlDbType.VarChar, 512).Value = (object?)request.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@intIdRegistroTributario", SqlDbType.Int).Value = request.IdRegistroTributario;
                cmd.Parameters.Add("@vchNumRegistroTributario", SqlDbType.VarChar, 50).Value = (object?)request.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@vchCorreo", SqlDbType.VarChar, 50).Value = (object?)request.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@vchWebSite", SqlDbType.VarChar, 200).Value = (object?)request.WebSite ?? DBNull.Value;
                cmd.Parameters.Add("@vchTelefono", SqlDbType.VarChar, 32).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@vchFax", SqlDbType.VarChar, 50).Value = (object?)request.Fax ?? DBNull.Value;
                cmd.Parameters.Add("@vchDireccion", SqlDbType.VarChar, 512).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@vchRecomendacion", SqlDbType.VarChar).Value = (object?)request.Recomendacion ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEmpresaAtencion", SqlDbType.Int).Value = request.IdEmpresaAtencion;
                cmd.Parameters.Add("@intIdIdioma", SqlDbType.Int).Value = request.IdIdioma;
                cmd.Parameters.Add("@vchLogoClienteUrl", SqlDbType.VarChar).Value = (object?)request.LogoClienteUrl ?? DBNull.Value;
                cmd.Parameters.Add("@bitImprimeLogoSafety", SqlDbType.Bit).Value = request.ImprimeLogoSafety;
                cmd.Parameters.Add("@intIdMoneda", SqlDbType.Int).Value = request.IdMoneda;
                cmd.Parameters.Add("@intIdIdiomaFacturacion", SqlDbType.Int).Value = request.IdIdiomaFacturacion;
                cmd.Parameters.Add("@bitAplicaPenalidad", SqlDbType.Bit).Value = request.AplicaPenalidad;
                cmd.Parameters.Add("@intIdPlantilla", SqlDbType.Int).Value = request.IdPlantilla;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;
                cmd.Parameters.Add("@bitEmitirPrefactura", SqlDbType.Bit).Value = request.EmitirPrefactura;

                var tableFormatoDocumento = ConstruirTablaFormatoDocumento(request.LstIdFormatoDocumento);
                var tvpFormatoDocumento = cmd.Parameters.AddWithValue("@lstIdFormatoDocumento", tableFormatoDocumento);
                tvpFormatoDocumento.SqlDbType = SqlDbType.Structured;
                tvpFormatoDocumento.TypeName = "LISTA_CLIENTE_FORMATO_DOCUMENTO";

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

                // Result set 3: NumRegistroTributario vigente ANTES de este cambio (ver SP_Cliente_Actualizar) —
                // identifica la fila correspondiente en ms-facturacion, que no guarda el IdCliente del backend.
                string? numRegistroTributarioAnterior = null;

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    numRegistroTributarioAnterior = GetNullableString(dr, "NumRegistroTributarioAnterior");
                }

                if (respuesta.IdTipoMensaje == 2)
                {
                    await SincronizarClienteFacturacionActualizarAsync(
                        usuarioLogueado, numRegistroTributarioAnterior, request.IdRegistroTributario,
                        request.NumRegistroTributario, request.Nombre, request.Correo, request.Direccion, request.IdPais);
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;

                await cn.OpenAsync();

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

        public async Task<Respuesta> ListarClientesAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? idPais, int? idEstado)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = (object?)idPais ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = (object?)idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)numPag ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_Listar_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchCorreoBusqueda", SqlDbType.VarChar, 100).Value = (object?)correoBusqueda ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_ListarFacturacion", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)numPag ?? DBNull.Value;
                cmd.Parameters.Add("@intEmitirPrefactura", SqlDbType.Int).Value = (object?)emitirPrefactura ?? DBNull.Value;
                cmd.Parameters.Add("@intIdIdiomaFacturacion", SqlDbType.Int).Value = (object?)idIdiomaFacturacion ?? DBNull.Value;
                cmd.Parameters.Add("@intEstadoFacturacion", SqlDbType.Int).Value = (object?)estadoFacturacion ?? DBNull.Value;

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
                                EstadoFacturacion = GetNullableString(dr, "EstadoFacturacion")
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Cliente_ListarPedidosFacturacion", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)numPag ?? DBNull.Value;

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
    }
}