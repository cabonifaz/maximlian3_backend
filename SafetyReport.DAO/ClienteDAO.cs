using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class ClienteDAO
    {
        private readonly DbConfig _dbConfig;

        public ClienteDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static DataTable ConstruirTablaContactos(List<ClienteContactoRequest>? contactos)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("CODIGO", typeof(string));
            table.Columns.Add("NOMBRES", typeof(string));
            table.Columns.Add("IDTIPOPERSONACONTACTO", typeof(int));
            table.Columns.Add("IDTIPOCONTACTO", typeof(int));
            table.Columns.Add("AREATRABAJO", typeof(int));
            table.Columns.Add("TELEFONO", typeof(string));
            table.Columns.Add("EMAIL", typeof(string));
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
                        contacto.AreaTrabajo,
                        (object?)contacto.Telefono ?? DBNull.Value,
                        (object?)contacto.Email ?? DBNull.Value,
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

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();

            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 0;

                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                var json = dr["Result"]?.ToString();

                respuesta.Result = respuesta.IdTipoMensaje == 2 && !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<T>()
                    : new List<T>();
            }
            else
            {
                respuesta.IdTipoMensaje = 1;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                respuesta.Result = new List<T>();
            }

            return respuesta;
        }

        public async Task<Respuesta> CrearClienteAsync(UsuarioGeneral usuarioLogueado, Cliente request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Cliente_INS", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar).Value = request.Nombre;
                cmd.Parameters.Add("@vchNombreCorto", SqlDbType.VarChar, 512).Value = (object?)request.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@intIdRegistroTributario", SqlDbType.Int).Value = request.IdRegistroTributario;
                cmd.Parameters.Add("@vchNumRegistroTributario", SqlDbType.VarChar, 50).Value = (object?)request.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 50).Value = (object?)request.Email ?? DBNull.Value;
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
                return await LeerRespuestaAsync<ClienteCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
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
                using SqlCommand cmd = new("Cliente_UPD", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar).Value = request.Nombre;
                cmd.Parameters.Add("@vchNombreCorto", SqlDbType.VarChar, 512).Value = (object?)request.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@intIdRegistroTributario", SqlDbType.Int).Value = request.IdRegistroTributario;
                cmd.Parameters.Add("@vchNumRegistroTributario", SqlDbType.VarChar, 50).Value = (object?)request.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@vchEmail", SqlDbType.VarChar, 50).Value = (object?)request.Email ?? DBNull.Value;
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

                var tableFormatoDocumento = ConstruirTablaFormatoDocumento(request.LstIdFormatoDocumento);
                var tvpFormatoDocumento = cmd.Parameters.AddWithValue("@lstIdFormatoDocumento", tableFormatoDocumento);
                tvpFormatoDocumento.SqlDbType = SqlDbType.Structured;
                tvpFormatoDocumento.TypeName = "LISTA_CLIENTE_FORMATO_DOCUMENTO";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
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
                using SqlCommand cmd = new("Cliente_SEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
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
                using SqlCommand cmd = new("Cliente_LST", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = idPais;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = idEstado;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)numPag ?? DBNull.Value;

                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 0;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = respuesta.IdTipoMensaje == 2 && !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<ClienteListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new ClienteListaResult()
                        : new ClienteListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new ClienteListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
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
                using SqlCommand cmd = new("Cliente_DEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = idCliente;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<ClienteEliminado>()
                };
            }
        }

        public async Task<Respuesta> ListarClienteShortAsync(UsuarioGeneral usuarioLogueado, int IdPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Cliente_LST_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = IdPedido;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<ClienteListaCorta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<ClienteListaCorta>()
                };
            }
        }
    }
}