using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class DirectorioEjecutivoDAO
    {
        private readonly DbConfig _dbConfig;

        public DirectorioEjecutivoDAO(DbConfig dbConfig)
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

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<DirectorioEjecutivoCrear> lstDirectorios)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("DirectorioEjecutivo_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                var tvp = new DataTable();
                tvp.Columns.Add("IdTipoPersona", typeof(int));
                tvp.Columns.Add("NombreCompleto", typeof(string));
                tvp.Columns.Add("IdPais", typeof(int));
                tvp.Columns.Add("Direccion", typeof(string));
                tvp.Columns.Add("Ubigeo", typeof(string));
                tvp.Columns.Add("CodigoPostal", typeof(string));
                tvp.Columns.Add("IdTipoDocumento", typeof(int));
                tvp.Columns.Add("NumeroDocumento", typeof(string));
                tvp.Columns.Add("TaxIdType", typeof(int));
                tvp.Columns.Add("TaxNum", typeof(string));
                tvp.Columns.Add("IdNacionalidad", typeof(int));
                tvp.Columns.Add("FechaNacimiento", typeof(DateTime));
                tvp.Columns.Add("IdEstadoCivil", typeof(int));
                tvp.Columns.Add("IdProfesion", typeof(int));
                tvp.Columns.Add("Referencias", typeof(string));

                foreach (var item in lstDirectorios)
                    tvp.Rows.Add(
                        (object?)item.IdTipoPersona    ?? DBNull.Value,
                        (object?)item.NombreCompleto   ?? DBNull.Value,
                        (object?)item.IdPais           ?? DBNull.Value,
                        (object?)item.Direccion        ?? DBNull.Value,
                        (object?)item.Ubigeo           ?? DBNull.Value,
                        (object?)item.CodigoPostal     ?? DBNull.Value,
                        (object?)item.IdTipoDocumento  ?? DBNull.Value,
                        (object?)item.NumeroDocumento  ?? DBNull.Value,
                        (object?)item.TaxIdType        ?? DBNull.Value,
                        (object?)item.TaxNum           ?? DBNull.Value,
                        (object?)item.IdNacionalidad   ?? DBNull.Value,
                        (object?)item.FechaNacimiento  ?? DBNull.Value,
                        (object?)item.IdEstadoCivil    ?? DBNull.Value,
                        (object?)item.IdProfesion      ?? DBNull.Value,
                        (object?)item.Referencias      ?? DBNull.Value);

                var paramTvp = cmd.Parameters.Add("@tvpDirectorios", SqlDbType.Structured);
                paramTvp.TypeName = "LISTA_DIRECTORIO_EJECUTIVO_INSERTAR";
                paramTvp.Value = tvp;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<DirectorioEjecutivoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoCreado>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, DirectorioEjecutivoEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("DirectorioEjecutivo_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdDirectorioEjecutivo", SqlDbType.Int).Value = request.IdDirectorioEjecutivo;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = (object?)request.IdTipoPersona ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCompleto", SqlDbType.VarChar, 255).Value = (object?)request.NombreCompleto ?? DBNull.Value;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = (object?)request.IdPais ?? DBNull.Value;
                cmd.Parameters.Add("@vchDireccion", SqlDbType.VarChar, 255).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@vchUbigeo", SqlDbType.VarChar, 150).Value = (object?)request.Ubigeo ?? DBNull.Value;
                cmd.Parameters.Add("@vchCodigoPostal", SqlDbType.VarChar, 50).Value = (object?)request.CodigoPostal ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoDocumento", SqlDbType.Int).Value = (object?)request.IdTipoDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 100).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@intTaxIdType", SqlDbType.Int).Value = (object?)request.TaxIdType ?? DBNull.Value;
                cmd.Parameters.Add("@vchTaxNum", SqlDbType.VarChar, 100).Value = (object?)request.TaxNum ?? DBNull.Value;
                cmd.Parameters.Add("@intIdNacionalidad", SqlDbType.Int).Value = (object?)request.IdNacionalidad ?? DBNull.Value;
                cmd.Parameters.Add("@dtmFechaNacimiento", SqlDbType.DateTime).Value = (object?)request.FechaNacimiento ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstadoCivil", SqlDbType.Int).Value = (object?)request.IdEstadoCivil ?? DBNull.Value;
                cmd.Parameters.Add("@intIdProfesion", SqlDbType.Int).Value = (object?)request.IdProfesion ?? DBNull.Value;
                cmd.Parameters.Add("@vchReferencias", SqlDbType.VarChar, 255).Value = (object?)request.Referencias ?? DBNull.Value;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<DirectorioEjecutivoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoCreado>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, DirectorioEjecutivoObtenerRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("DirectorioEjecutivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdDirectorioEjecutivo", SqlDbType.Int).Value = (object?)request.IdDirectorioEjecutivo ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCompleto", SqlDbType.VarChar, 255).Value = (object?)request.NombreCompleto ?? DBNull.Value;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 100).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<DirectorioEjecutivoConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroDirectorioEjecutivo filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("DirectorioEjecutivo_Listar", cn) { CommandType = CommandType.StoredProcedure };
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
                        ? JsonSerializer.Deserialize<DirectorioEjecutivoListaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new DirectorioEjecutivoListaResult()
                        : new DirectorioEjecutivoListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new DirectorioEjecutivoListaResult();
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new DirectorioEjecutivoListaResult() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idDirectorioEjecutivo)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("DirectorioEjecutivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdDirectorioEjecutivo", SqlDbType.Int).Value = idDirectorioEjecutivo;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<DirectorioEjecutivoEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoEliminado>() };
            }
        }
    }
}
