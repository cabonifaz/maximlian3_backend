using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

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

        private static DateTime? GetNullableDateTime(DbDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        private async Task<Respuesta> LeerCabeceraAsync(DbDataReader dr, string commandText)
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

        private static async Task<List<T>> LeerIdsAsync<T>(DbDataReader dr, string columnName, Func<int?, T> factory)
        {
            var lista = new List<T>();

            while (await dr.ReadAsync())
            {
                lista.Add(factory(GetNullableInt(dr, columnName)));
            }

            return lista;
        }

        private static DirectorioEjecutivoConsulta LeerConsulta(DbDataReader dr)
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

        private static DirectorioEjecutivoListaConsulta LeerConsultaLista(DbDataReader dr)
        {
            return new DirectorioEjecutivoListaConsulta
            {
                IdDirectorioEjecutivo = Convert.ToInt32(dr["IdDirectorioEjecutivo"]),
                NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                TipoDocumento = GetNullableString(dr, "TipoDocumento"),
                NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                Pais = GetNullableString(dr, "Pais"),
                TaxNum = GetNullableString(dr, "TaxNum")
            };
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<DirectorioEjecutivoCrear> lstDirectorios)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_DirectorioEjecutivo_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@tvpDirectorios", JsonSerializer.Serialize(lstDirectorios));

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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_DirectorioEjecutivo_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdDirectorioEjecutivo", request.IdDirectorioEjecutivo);
                cmd.Parameters.AddWithValue("@intIdTipoPersona", (object?)request.IdTipoPersona ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNombreCompleto", (object?)request.NombreCompleto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdPais", (object?)request.IdPais ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchDireccion", (object?)request.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchUbigeo", (object?)request.Ubigeo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchCodigoPostal", (object?)request.CodigoPostal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdTipoDocumento", (object?)request.IdTipoDocumento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNumeroDocumento", (object?)request.NumeroDocumento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intTaxIdType", (object?)request.TaxIdType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchTaxNum", (object?)request.TaxNum ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdNacionalidad", (object?)request.IdNacionalidad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtmFechaNacimiento", (object?)request.FechaNacimiento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEstadoCivil", (object?)request.IdEstadoCivil ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdProfesion", (object?)request.IdProfesion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchReferencias", (object?)request.Referencias ?? DBNull.Value);

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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_DirectorioEjecutivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdDirectorioEjecutivo", (object?)request.IdDirectorioEjecutivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNombreCompleto", (object?)request.NombreCompleto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchNumeroDocumento", (object?)request.NumeroDocumento ?? DBNull.Value);

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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_DirectorioEjecutivo_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)filtro.Busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", filtro.NumPag);

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
                            resultado.lstDirectoriosEjecutivos.Add(LeerConsultaLista(dr));
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
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_DirectorioEjecutivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdDirectorioEjecutivo", idDirectorioEjecutivo);

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
