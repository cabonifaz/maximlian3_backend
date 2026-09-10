using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.DirectorioEjecutivo;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class DirectorioEjecutivoRepositorioSql : IDirectorioEjecutivoRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<DirectorioEjecutivoRepositorioSql> _logger;

        public DirectorioEjecutivoRepositorioSql(DbConfig dbConfig, ILogger<DirectorioEjecutivoRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

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

        private static DateTime? GetNullableDateTime(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        private async Task<Respuesta> LeerCabeceraAsync(MySqlDataReader dr, string commandText)
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

        private static async Task<List<T>> LeerIdsAsync<T>(MySqlDataReader dr, string columnName, Func<int?, T> factory)
        {
            var lista = new List<T>();

            while (await dr.ReadAsync())
            {
                lista.Add(factory(GetNullableInt(dr, columnName)));
            }

            return lista;
        }

        private static DirectorioEjecutivoConsulta LeerConsulta(MySqlDataReader dr)
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

        private static DirectorioEjecutivoListaConsulta LeerConsultaLista(MySqlDataReader dr)
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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

                var json = JsonSerializer.Serialize(lstDirectorios.Select(item => new
                {
                    item.IdTipoPersona,
                    item.NombreCompleto,
                    item.IdPais,
                    item.Direccion,
                    item.Ubigeo,
                    item.CodigoPostal,
                    item.IdTipoDocumento,
                    item.NumeroDocumento,
                    item.TaxIdType,
                    item.TaxNum,
                    item.IdNacionalidad,
                    item.FechaNacimiento,
                    item.IdEstadoCivil,
                    item.IdProfesion,
                    item.Referencias
                }));
                cmd.Parameters.Add("@p_jsonDirectorios", MySqlDbType.JSON).Value = json;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdDirectorioEjecutivo", MySqlDbType.Int32).Value = request.IdDirectorioEjecutivo;
                cmd.Parameters.Add("@p_intIdTipoPersona", MySqlDbType.Int32).Value = (object?)request.IdTipoPersona ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombreCompleto", MySqlDbType.VarChar, 255).Value = (object?)request.NombreCompleto ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = (object?)request.IdPais ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchDireccion", MySqlDbType.VarChar, 255).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchUbigeo", MySqlDbType.VarChar, 150).Value = (object?)request.Ubigeo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCodigoPostal", MySqlDbType.VarChar, 50).Value = (object?)request.CodigoPostal ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTipoDocumento", MySqlDbType.Int32).Value = (object?)request.IdTipoDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNumeroDocumento", MySqlDbType.VarChar, 100).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@p_intTaxIdType", MySqlDbType.Int32).Value = (object?)request.TaxIdType ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchTaxNum", MySqlDbType.VarChar, 100).Value = (object?)request.TaxNum ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdNacionalidad", MySqlDbType.Int32).Value = (object?)request.IdNacionalidad ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtmFechaNacimiento", MySqlDbType.DateTime).Value = (object?)request.FechaNacimiento ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEstadoCivil", MySqlDbType.Int32).Value = (object?)request.IdEstadoCivil ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdProfesion", MySqlDbType.Int32).Value = (object?)request.IdProfesion ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchReferencias", MySqlDbType.VarChar, 255).Value = (object?)request.Referencias ?? DBNull.Value;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdDirectorioEjecutivo", MySqlDbType.Int32).Value = (object?)request.IdDirectorioEjecutivo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombreCompleto", MySqlDbType.VarChar, 255).Value = (object?)request.NombreCompleto ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNumeroDocumento", MySqlDbType.VarChar, 100).Value = (object?)request.NumeroDocumento ?? DBNull.Value;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = filtro.NumPag;

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
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdDirectorioEjecutivo", MySqlDbType.Int32).Value = idDirectorioEjecutivo;

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
