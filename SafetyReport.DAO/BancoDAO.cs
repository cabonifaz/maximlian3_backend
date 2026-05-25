using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class BancoDAO
    {
        private readonly DbConfig _dbConfig;

        public BancoDAO(DbConfig dbConfig)
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

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<BancoCrear> lstBancos)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Banco_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                var tvp = new DataTable();
                tvp.Columns.Add("IdPais", typeof(int));
                tvp.Columns.Add("Nombre", typeof(string));
                tvp.Columns.Add("Telefono", typeof(string));
                foreach (var item in lstBancos)
                    tvp.Rows.Add(item.IdPais, item.Nombre, (object?)item.Telefono ?? DBNull.Value);

                var paramTvp = cmd.Parameters.Add("@tvpBancos", SqlDbType.Structured);
                paramTvp.TypeName = "LISTA_BANCO_INSERTAR";
                paramTvp.Value = tvp;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<BancoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoCreado>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, BancoEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Banco_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdBanco", SqlDbType.Int).Value = request.IdBanco;
                cmd.Parameters.Add("@intIdPais", SqlDbType.Int).Value = request.IdPais;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar, 255).Value = request.Nombre;
                cmd.Parameters.Add("@vchTelefono", SqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<BancoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoCreado>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, BancoObtenerRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Banco_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdBanco", SqlDbType.Int).Value = (object?)request.IdBanco ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar, 255).Value = (object?)request.Nombre ?? DBNull.Value;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<BancoConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroBanco filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Banco_Listar", cn) { CommandType = CommandType.StoredProcedure };
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
                        ? JsonSerializer.Deserialize<BancoListaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new BancoListaResult()
                        : new BancoListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new BancoListaResult();
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new BancoListaResult() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idBanco)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Banco_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdBanco", SqlDbType.Int).Value = idBanco;
                await cn.OpenAsync();
                return await LeerRespuestaAsync<BancoEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoEliminado>() };
            }
        }
    }
}
