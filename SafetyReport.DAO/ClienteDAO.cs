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
            table.Columns.Add("NOMBRES", typeof(string));
            table.Columns.Add("IDTIPOCONTACTO", typeof(int));
            table.Columns.Add("AREATRABAJO", typeof(int));
            table.Columns.Add("TELEFONO", typeof(string));
            table.Columns.Add("EMAIL", typeof(string));

            int i = 1;

            if (contactos != null)
            {
                foreach (var contacto in contactos)
                {
                    table.Rows.Add(
                        i++,
                        contacto.Nombres ?? string.Empty,
                        contacto.IdTipoContacto,
                        contacto.AreaTrabajo,
                        (object?)contacto.Telefono ?? DBNull.Value,
                        (object?)contacto.Email ?? DBNull.Value
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

                respuesta.Result = !string.IsNullOrWhiteSpace(json)
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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Cliente_INS", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@IdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar).Value = request.Nombre;
                cmd.Parameters.Add("@NombreCorto", SqlDbType.VarChar, 512).Value = (object?)request.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@IdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@IdRegistroTributario", SqlDbType.Int).Value = request.IdRegistroTributario;
                cmd.Parameters.Add("@NumRegistroTributario", SqlDbType.VarChar, 50).Value = (object?)request.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 50).Value = (object?)request.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@WebSite", SqlDbType.VarChar, 200).Value = (object?)request.WebSite ?? DBNull.Value;
                cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 32).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@Fax", SqlDbType.VarChar, 50).Value = (object?)request.Fax ?? DBNull.Value;
                cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 512).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@Recomendacion", SqlDbType.VarChar).Value = (object?)request.Recomendacion ?? DBNull.Value;
                cmd.Parameters.Add("@IdEmpresaAtencion", SqlDbType.Int).Value = request.IdEmpresaAtencion;
                cmd.Parameters.Add("@IdIdioma", SqlDbType.Int).Value = request.IdIdioma;
                cmd.Parameters.Add("@LogoClienteUrl", SqlDbType.VarChar).Value = (object?)request.LogoClienteUrl ?? DBNull.Value;
                cmd.Parameters.Add("@ImprimeLogoSafety", SqlDbType.Bit).Value = request.ImprimeLogoSafety;
                cmd.Parameters.Add("@IdFormatoDocumento", SqlDbType.Int).Value = request.IdFormatoDocumento;
                cmd.Parameters.Add("@IdMoneda", SqlDbType.Int).Value = request.IdMoneda;
                cmd.Parameters.Add("@IdIdiomaFacturacion", SqlDbType.Int).Value = request.IdIdiomaFacturacion;
                cmd.Parameters.Add("@AplicaPenalidad", SqlDbType.Bit).Value = request.AplicaPenalidad;
                cmd.Parameters.Add("@IdPlantilla", SqlDbType.Int).Value = request.IdPlantilla;

                var table = ConstruirTablaContactos(request.Contactos);
                var tvp = cmd.Parameters.AddWithValue("@lstContactos", table);
                tvp.SqlDbType = SqlDbType.Structured;
                tvp.TypeName = "LISTA_CLIENTE_CONTACTO";

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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Cliente_UPD", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@IdTipoPersona", SqlDbType.Int).Value = request.InfoCliente.IdTipoPersona;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar).Value = request.InfoCliente.Nombre;
                cmd.Parameters.Add("@NombreCorto", SqlDbType.VarChar, 512).Value = (object?)request.InfoCliente.NombreCorto ?? DBNull.Value;
                cmd.Parameters.Add("@IdPais", SqlDbType.Int).Value = request.InfoCliente.IdPais;
                cmd.Parameters.Add("@IdRegistroTributario", SqlDbType.Int).Value = request.InfoCliente.IdRegistroTributario;
                cmd.Parameters.Add("@NumRegistroTributario", SqlDbType.VarChar, 50).Value = (object?)request.InfoCliente.NumRegistroTributario ?? DBNull.Value;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 50).Value = (object?)request.InfoCliente.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@WebSite", SqlDbType.VarChar, 200).Value = (object?)request.InfoCliente.WebSite ?? DBNull.Value;
                cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 32).Value = (object?)request.InfoCliente.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@Fax", SqlDbType.VarChar, 50).Value = (object?)request.InfoCliente.Fax ?? DBNull.Value;
                cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 512).Value = (object?)request.InfoCliente.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@Recomendacion", SqlDbType.VarChar).Value = (object?)request.InfoCliente.Recomendacion ?? DBNull.Value;
                cmd.Parameters.Add("@IdEmpresaAtencion", SqlDbType.Int).Value = request.InfoCliente.IdEmpresaAtencion;
                cmd.Parameters.Add("@IdIdioma", SqlDbType.Int).Value = request.InfoCliente.IdIdioma;
                cmd.Parameters.Add("@LogoClienteUrl", SqlDbType.VarChar).Value = (object?)request.InfoCliente.LogoClienteUrl ?? DBNull.Value;
                cmd.Parameters.Add("@ImprimeLogoSafety", SqlDbType.Bit).Value = request.InfoCliente.ImprimeLogoSafety;
                cmd.Parameters.Add("@IdFormatoDocumento", SqlDbType.Int).Value = request.InfoCliente.IdFormatoDocumento;
                cmd.Parameters.Add("@IdMoneda", SqlDbType.Int).Value = request.InfoCliente.IdMoneda;
                cmd.Parameters.Add("@IdIdiomaFacturacion", SqlDbType.Int).Value = request.InfoCliente.IdIdiomaFacturacion;
                cmd.Parameters.Add("@AplicaPenalidad", SqlDbType.Bit).Value = request.InfoCliente.AplicaPenalidad;
                cmd.Parameters.Add("@IdPlantilla", SqlDbType.Int).Value = request.InfoCliente.IdPlantilla;

                var table = ConstruirTablaContactos(request.InfoCliente.Contactos);
                var tvp = cmd.Parameters.AddWithValue("@lstContactos", table);
                tvp.SqlDbType = SqlDbType.Structured;
                tvp.TypeName = "LISTA_CLIENTE_CONTACTO";

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
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Cliente_SEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;

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

        public async Task<Respuesta> ListarClientesAsync(UsuarioGeneral usuarioLogueado, string? filtro)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(_dbConfig.ConnectionString);
                using SqlCommand cmd = new SqlCommand("Cliente_LST", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchFiltro", SqlDbType.VarChar, 255).Value = (object?)filtro ?? DBNull.Value;

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
    }
}