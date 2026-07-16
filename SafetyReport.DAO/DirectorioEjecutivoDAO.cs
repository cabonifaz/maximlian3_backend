using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

namespace SafetyReport.DAO
{
    public class DirectorioEjecutivoDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<DirectorioEjecutivoDAO> _logger;

        public DirectorioEjecutivoDAO(DbConfig dbConfig, ILogger<DirectorioEjecutivoDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
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

        private static DateTime? GetNullableDateTime(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        private async Task<Respuesta> LeerCabeceraAsync(SqlDataReader dr, string commandText)
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
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", commandText);

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

        private static DirectorioEjecutivoConsulta LeerConsulta(SqlDataReader dr)
        {
            return new DirectorioEjecutivoConsulta
            {
                IdDirectorioEjecutivo = Convert.ToInt32(dr["IdDirectorioEjecutivo"]),
                IdTipoPersona = GetNullableInt(dr, "IdTipoPersona"),
                TipoPersona = GetNullableString(dr, "TipoPersona"),
                NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                IdPais = GetNullableInt(dr, "IdPais"),
                Pais = GetNullableString(dr, "Pais"),
                Direccion = GetNullableString(dr, "Direccion"),
                Ubigeo = GetNullableString(dr, "Ubigeo"),
                CodigoPostal = GetNullableString(dr, "CodigoPostal"),
                IdTipoDocumento = GetNullableInt(dr, "IdTipoDocumento"),
                TipoDocumento = GetNullableString(dr, "TipoDocumento"),
                NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                TaxIdType = GetNullableInt(dr, "TaxIdType"),
                TaxNum = GetNullableString(dr, "TaxNum"),
                IdNacionalidad = GetNullableInt(dr, "IdNacionalidad"),
                Nacionalidad = GetNullableString(dr, "Nacionalidad"),
                FechaNacimiento = GetNullableDateTime(dr, "FechaNacimiento"),
                IdEstadoCivil = GetNullableInt(dr, "IdEstadoCivil"),
                EstadoCivil = GetNullableString(dr, "EstadoCivil"),
                IdProfesion = GetNullableInt(dr, "IdProfesion"),
                Profesion = GetNullableString(dr, "Profesion"),
                Referencias = GetNullableString(dr, "Referencias")
            };
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<DirectorioEjecutivoCrear> lstDirectorios)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_DirectorioEjecutivo_Insertar", cn) { CommandType = CommandType.StoredProcedure };
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

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdDirectorioEjecutivo", id => new DirectorioEjecutivoCreado { IdDirectorioEjecutivo = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<DirectorioEjecutivoCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoCreado>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, DirectorioEjecutivoEditar request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_DirectorioEjecutivo_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
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

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdDirectorioEjecutivo", id => new DirectorioEjecutivoCreado { IdDirectorioEjecutivo = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<DirectorioEjecutivoCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoCreado>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, DirectorioEjecutivoObtenerRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_DirectorioEjecutivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdDirectorioEjecutivo", SqlDbType.Int).Value = (object?)request.IdDirectorioEjecutivo ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCompleto", SqlDbType.VarChar, 255).Value = (object?)request.NombreCompleto ?? DBNull.Value;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 100).Value = (object?)request.NumeroDocumento ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<DirectorioEjecutivoConsulta>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(LeerConsulta(dr));
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<DirectorioEjecutivoConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroDirectorioEjecutivo filtro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_DirectorioEjecutivo_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = filtro.NumPag;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new DirectorioEjecutivoListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstDirectoriosEjecutivos.Add(LeerConsulta(dr));
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new DirectorioEjecutivoListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new DirectorioEjecutivoListaResult() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idDirectorioEjecutivo)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_DirectorioEjecutivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdDirectorioEjecutivo", SqlDbType.Int).Value = idDirectorioEjecutivo;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdDirectorioEjecutivo", id => new DirectorioEjecutivoEliminado { IdDirectorioEjecutivo = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<DirectorioEjecutivoEliminado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoEliminado>() };
            }
        }
    }
}
