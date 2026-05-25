using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class CompaniaDAO
    {
        private readonly DbConfig _dbConfig;

        public CompaniaDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();
            using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"]) : 0;
                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                var json = dr["Result"]?.ToString();
                respuesta.Result = respuesta.IdTipoMensaje == 2 && !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>()
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

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<CompaniaCrear> lstCompanias)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Compania_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                var tvp = new DataTable();
                tvp.Columns.Add("IdTipoPersona", typeof(int));
                tvp.Columns.Add("IdTipoDocumento", typeof(int));
                tvp.Columns.Add("NumeroDocumento", typeof(string));
                tvp.Columns.Add("NombreCompleto", typeof(string));
                tvp.Columns.Add("IdPais", typeof(int));
                tvp.Columns.Add("Telefono", typeof(string));
                tvp.Columns.Add("ExisteInformacion", typeof(bool));
                foreach (var item in lstCompanias)
                    tvp.Rows.Add(
                        (object?)item.IdTipoPersona     ?? DBNull.Value,
                        (object?)item.IdTipoDocumento   ?? DBNull.Value,
                        (object?)item.NumeroDocumento   ?? DBNull.Value,
                        (object?)item.NombreCompleto    ?? DBNull.Value,
                        (object?)item.IdPais            ?? DBNull.Value,
                        (object?)item.Telefono          ?? DBNull.Value,
                        (object?)item.ExisteInformacion ?? DBNull.Value);

                var paramTvp = cmd.Parameters.Add("@tvpCompanias", SqlDbType.Structured);
                paramTvp.TypeName = "LISTA_COMPANIA_INSERTAR";
                paramTvp.Value = tvp;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<CompaniaCreada>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, CompaniaEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Compania_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = request.IdCompania;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = (object?)request.IdTipoPersona ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoDocumento", SqlDbType.Int).Value = (object?)request.IdTipoDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 255).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCompleto", SqlDbType.VarChar, 255).Value = (object?)request.NombreCompleto ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = (object?)request.IdPais ?? DBNull.Value;
                cmd.Parameters.Add("@vchTelefono", SqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@bitExisteInformacion", SqlDbType.Bit).Value = (object?)request.ExisteInformacion ?? DBNull.Value;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<CompaniaCreada>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, CompaniaObtenerRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Compania_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)request.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@vchNumDocumento", SqlDbType.VarChar, 255).Value = (object?)request.NumDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar, 255).Value = (object?)request.Nombre ?? DBNull.Value;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<CompaniaConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroCompania filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Compania_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = filtro.NumPag;
                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value ? Convert.ToInt32(dr["IdTipoMensaje"]) : 0;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
                    var json = dr["Result"]?.ToString();
                    respuesta.Result = respuesta.IdTipoMensaje == 2 && !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<CompaniaListaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CompaniaListaResult()
                        : new CompaniaListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new CompaniaListaResult();
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaListaResult() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idCompania)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Compania_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = idCompania;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<CompaniaEliminada>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaEliminada>() };
            }
        }
    }
}
