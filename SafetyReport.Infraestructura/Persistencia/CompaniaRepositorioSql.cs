using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Compania;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class CompaniaRepositorioSql : ICompaniaRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<CompaniaRepositorioSql> _logger;

        public CompaniaRepositorioSql(DbConfig dbConfig, ILogger<CompaniaRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static int? GetNullableInt(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static long? GetNullableLong(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (long?)null : Convert.ToInt64(value);
        }

        private static string? GetNullableString(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private static bool? GetNullableBool(MySqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (bool?)null : Convert.ToBoolean(value);
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

        private static CompaniaConsulta LeerCompaniaConsulta(MySqlDataReader dr)
        {
            return new CompaniaConsulta
            {
                IdCompania = Convert.ToInt32(dr["IdCompania"]),
                IdTipoPersona = GetNullableInt(dr, "IdTipoPersona"),
                TipoPersona = GetNullableString(dr, "TipoPersona"),
                IdTipoDocumento = GetNullableInt(dr, "IdTipoDocumento"),
                TipoDocumento = GetNullableString(dr, "TipoDocumento"),
                NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                IdPais = GetNullableInt(dr, "IdPais"),
                Pais = GetNullableString(dr, "Pais"),
                Telefono = GetNullableString(dr, "Telefono"),
                Direccion = GetNullableString(dr, "Direccion"),
                CiudadProvinciaEstado = GetNullableString(dr, "CiudadProvinciaEstado"),
                CodigoPostal = GetNullableString(dr, "CodigoPostal"),
                ExisteInformacion = GetNullableBool(dr, "ExisteInformacion")
            };
        }

        private static CompaniaListaConsulta LeerCompaniaListaConsulta(MySqlDataReader dr)
        {
            return new CompaniaListaConsulta
            {
                IdCompania = Convert.ToInt32(dr["IdCompania"]),
                TipoDocumento = GetNullableString(dr, "TipoDocumento"),
                NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                Pais = GetNullableString(dr, "Pais"),
                Telefono = GetNullableString(dr, "Telefono"),
                ExisteInformacion = GetNullableString(dr, "ExisteInformacion")
            };
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<CompaniaCrear> lstCompanias)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Compania_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

                var json = JsonSerializer.Serialize(lstCompanias.Select(item => new
                {
                    item.IdTipoPersona,
                    item.IdTipoDocumento,
                    item.NumeroDocumento,
                    item.NombreCompleto,
                    item.IdPais,
                    item.Telefono,
                    item.Direccion,
                    item.CiudadProvinciaEstado,
                    item.CodigoPostal,
                    item.ExisteInformacion
                }));
                cmd.Parameters.Add("@p_json_companias", MySqlDbType.JSON).Value = json;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCompania", id => new CompaniaCreada { IdCompania = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<CompaniaCreada>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, CompaniaEditar request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Compania_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = request.IdCompania;
                cmd.Parameters.Add("@p_intIdTipoPersona", MySqlDbType.Int32).Value = (object?)request.IdTipoPersona ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdTipoDocumento", MySqlDbType.Int32).Value = (object?)request.IdTipoDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNumeroDocumento", MySqlDbType.VarChar, 255).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombreCompleto", MySqlDbType.VarChar, 255).Value = (object?)request.NombreCompleto ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = (object?)request.IdPais ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchTelefono", MySqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchDireccion", MySqlDbType.VarChar, 255).Value = (object?)request.Direccion ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCiudadProvinciaEstado", MySqlDbType.VarChar, 255).Value = (object?)request.CiudadProvinciaEstado ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCodigoPostal", MySqlDbType.VarChar, 32).Value = (object?)request.CodigoPostal ?? DBNull.Value;
                cmd.Parameters.Add("@p_bitExisteInformacion", MySqlDbType.Bool).Value = (object?)request.ExisteInformacion ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCompania", id => new CompaniaCreada { IdCompania = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<CompaniaCreada>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, CompaniaObtenerRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Compania_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = (object?)request.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNumDocumento", MySqlDbType.VarChar, 255).Value = (object?)request.NumDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombre", MySqlDbType.VarChar, 255).Value = (object?)request.Nombre ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<CompaniaConsulta>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(LeerCompaniaConsulta(dr));
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<CompaniaConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroCompania filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Compania_Listar", cn) { CommandType = CommandType.StoredProcedure };
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
                    var resultado = new CompaniaListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstCompanias.Add(LeerCompaniaListaConsulta(dr));
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new CompaniaListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaListaResult() };
            }
        }

        public async Task<Respuesta> BuscarAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaBusqueda filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Compania_Buscar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = (object?)filtro.IdPais ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<CompaniaBusquedaItem>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new CompaniaBusquedaItem
                        {
                            IdCompania = Convert.ToInt32(dr["IdCompania"]),
                            NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                            NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                            NombreComercial = GetNullableString(dr, "NombreComercial"),
                            TipoPersona = GetNullableString(dr, "TipoPersona")
                        });
                    }
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaBusquedaItem>() };
            }
        }

        public async Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<CompaniaMatchItem> lista)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Compania_ObtenerCoincidencias", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_json_lista", MySqlDbType.JSON).Value = JsonSerializer.Serialize(lista);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCompania", id => new CompaniaMatchResultItem { IdCompania = id });
                }
                else
                {
                    respuesta.Result = new List<CompaniaMatchResultItem>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaMatchResultItem>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idCompania)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Compania_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = idCompania;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCompania", id => new CompaniaEliminada { IdCompania = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<CompaniaEliminada>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaEliminada>() };
            }
        }

        public async Task<Respuesta> CrearNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaCrear request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticia_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = request.IdCompania;
                cmd.Parameters.Add("@p_vchTitulo", MySqlDbType.VarChar, 1024).Value = (object?)request.Titulo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchDescripcion", MySqlDbType.VarChar, -1).Value = (object?)request.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtFechaNoticia", MySqlDbType.DateTime).Value = (object?)request.FechaNoticia ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCategoria", MySqlDbType.VarChar, 255).Value = (object?)request.Categoria ?? DBNull.Value;

                var json = JsonSerializer.Serialize(request.Archivos.Select(archivo => new
                {
                    archivo.IdCompaniaNoticiaArchivo,
                    archivo.IdTipoArchivo,
                    archivo.ArchivoUrl,
                    NombreDocumento = archivo.NombreDocumento ?? archivo.NombreArchivo
                }));
                cmd.Parameters.Add("@p_json_archivos", MySqlDbType.JSON).Value = json;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCompaniaNoticia", id => new CompaniaNoticiaCreada { IdCompaniaNoticia = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaCreada>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> EditarNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaEditar request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticia_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompaniaNoticia", MySqlDbType.Int32).Value = request.IdCompaniaNoticia;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = request.IdCompania;
                cmd.Parameters.Add("@p_vchTitulo", MySqlDbType.VarChar, 1024).Value = (object?)request.Titulo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchDescripcion", MySqlDbType.VarChar, -1).Value = (object?)request.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtFechaNoticia", MySqlDbType.DateTime).Value = (object?)request.FechaNoticia ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCategoria", MySqlDbType.VarChar, 255).Value = (object?)request.Categoria ?? DBNull.Value;

                var json = JsonSerializer.Serialize(request.Archivos.Select(archivo => new
                {
                    archivo.IdCompaniaNoticiaArchivo,
                    archivo.IdTipoArchivo,
                    archivo.ArchivoUrl,
                    NombreDocumento = archivo.NombreDocumento ?? archivo.NombreArchivo
                }));
                cmd.Parameters.Add("@p_json_archivos", MySqlDbType.JSON).Value = json;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCompaniaNoticia", id => new CompaniaNoticiaCreada { IdCompaniaNoticia = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaCreada>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaObtenerRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticia_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompaniaNoticia", MySqlDbType.Int32).Value = (object?)request.IdCompaniaNoticia ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = (object?)request.IdCompania ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var noticias = new List<CompaniaNoticiaConsulta>();

                    while (await dr.ReadAsync())
                    {
                        noticias.Add(new CompaniaNoticiaConsulta
                        {
                            IdCompaniaNoticia = Convert.ToInt32(dr["IdCompaniaNoticia"]),
                            IdCompania = Convert.ToInt32(dr["IdCompania"]),
                            Titulo = GetNullableString(dr, "Titulo"),
                            Descripcion = GetNullableString(dr, "Descripcion"),
                            FechaNoticia = GetNullableString(dr, "FechaNoticia"),
                            Categoria = GetNullableString(dr, "Categoria")
                        });
                    }

                    if (await dr.NextResultAsync())
                    {
                        var porNoticia = noticias.ToDictionary(n => n.IdCompaniaNoticia);

                        while (await dr.ReadAsync())
                        {
                            var idNoticia = Convert.ToInt32(dr["IdCompaniaNoticia"]);

                            if (porNoticia.TryGetValue(idNoticia, out var noticia))
                            {
                                noticia.Archivos.Add(new CompaniaNoticiaArchivoConsulta
                                {
                                    IdCompaniaNoticiaArchivo = Convert.ToInt32(dr["IdCompaniaNoticiaArchivo"]),
                                    IdCompaniaNoticia = idNoticia,
                                    IdTipoArchivo = Convert.ToInt32(dr["IdTipoArchivo"]),
                                    TipoArchivo = GetNullableString(dr, "TipoArchivo"),
                                    NombreDocumento = GetNullableString(dr, "NombreDocumento")
                                });
                            }
                        }
                    }

                    respuesta.Result = noticias;
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaConsulta>() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticiaArchivo)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticiaArchivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompaniaNoticiaArchivo", MySqlDbType.Int32).Value = idCompaniaNoticiaArchivo;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<CompaniaNoticiaArchivoDescargaConsulta>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(new CompaniaNoticiaArchivoDescargaConsulta
                        {
                            IdCompaniaNoticiaArchivo = Convert.ToInt32(dr["IdCompaniaNoticiaArchivo"]),
                            IdCompaniaNoticia = Convert.ToInt32(dr["IdCompaniaNoticia"]),
                            IdTipoArchivo = Convert.ToInt32(dr["IdTipoArchivo"]),
                            ArchivoUrl = GetNullableString(dr, "ArchivoUrl"),
                            NombreDocumento = GetNullableString(dr, "NombreDocumento")
                        });
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaArchivoDescargaConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaArchivoDescargaConsulta>() };
            }
        }

        public async Task<Respuesta> EliminarNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticiaArchivo)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticiaArchivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompaniaNoticiaArchivo", MySqlDbType.Int32).Value = idCompaniaNoticiaArchivo;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<CompaniaNoticiaArchivoEliminado>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(new CompaniaNoticiaArchivoEliminado
                        {
                            IdCompaniaNoticiaArchivo = Convert.ToInt32(dr["IdCompaniaNoticiaArchivo"]),
                            IdCompaniaNoticia = Convert.ToInt32(dr["IdCompaniaNoticia"]),
                            ArchivoUrl = GetNullableString(dr, "ArchivoUrl")
                        });
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaArchivoEliminado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaArchivoEliminado>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticia filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticia_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtmFchInicio", MySqlDbType.Date).Value = (object?)filtro.FchInicio ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtmFchFin", MySqlDbType.Date).Value = (object?)filtro.FchFin ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = filtro.NumPag;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new CompaniaNoticiaListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstCompaniaNoticias.Add(new CompaniaNoticiaListaConsulta
                            {
                                IdCompaniaNoticia = Convert.ToInt32(dr["IdCompaniaNoticia"]),
                                IdCompania = Convert.ToInt32(dr["IdCompania"]),
                                NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                                Titulo = GetNullableString(dr, "Titulo"),
                                Descripcion = GetNullableString(dr, "Descripcion"),
                                FechaNoticia = GetNullableString(dr, "FechaNoticia"),
                                Categoria = GetNullableString(dr, "Categoria")
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new CompaniaNoticiaListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaNoticiaListaResult() };
            }
        }

        public async Task<Respuesta> EliminarNoticiaAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticia)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticia_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompaniaNoticia", MySqlDbType.Int32).Value = idCompaniaNoticia;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdCompaniaNoticia", id => new CompaniaNoticiaEliminada { IdCompaniaNoticia = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaEliminada>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaEliminada>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasBalanceAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaBalance filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticiaBalance_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchTipoEstadoFinanciero", MySqlDbType.VarChar, -1).Value = (object?)filtro.TipoEstadoFinanciero ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchEstado", MySqlDbType.VarChar, -1).Value = (object?)filtro.Estado ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtmFchInicio", MySqlDbType.Date).Value = (object?)filtro.FchInicio ?? DBNull.Value;
                cmd.Parameters.Add("@p_dtmFchFin", MySqlDbType.Date).Value = (object?)filtro.FchFin ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = filtro.NumPag;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new CompaniaNoticiaBalanceListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstCompaniaNoticiasBalance.Add(new CompaniaNoticiaBalanceListaConsulta
                            {
                                IdInformeBalance = Convert.ToInt32(dr["IdInformeBalance"]),
                                IdCompania = Convert.ToInt32(dr["IdCompania"]),
                                NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                                FechaInicio = GetNullableString(dr, "FechaInicio"),
                                FechaFin = GetNullableString(dr, "FechaFin"),
                                Pais = GetNullableString(dr, "Pais"),
                                TipoEstadoFinanciero = GetNullableString(dr, "TipoEstadoFinanciero"),
                                Estado = GetNullableString(dr, "Estado")
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new CompaniaNoticiaBalanceListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaNoticiaBalanceListaResult() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaBalanceAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaBalanceObtenerRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticiaBalance_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdInformeBalance", MySqlDbType.Int32).Value = (object?)request.IdInformeBalance ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = (object?)request.IdCompania ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<CompaniaNoticiaBalanceConsulta>();

                    while (await dr.ReadAsync())
                    {
                        var detalleTexto = GetNullableString(dr, "DetalleBalance");

                        lista.Add(new CompaniaNoticiaBalanceConsulta
                        {
                            IdInformeBalance = Convert.ToInt32(dr["IdInformeBalance"]),
                            IdCompania = Convert.ToInt32(dr["IdCompania"]),
                            NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                            FechaInicio = GetNullableDateTime(dr, "FechaInicio"),
                            FechaFin = GetNullableDateTime(dr, "FechaFin"),
                            Pais = GetNullableString(dr, "Pais"),
                            IdTipoEstadoFinanciero = GetNullableInt(dr, "IdTipoEstadoFinanciero"),
                            TipoEstadoFinanciero = GetNullableString(dr, "TipoEstadoFinanciero"),
                            DetalleBalance = string.IsNullOrWhiteSpace(detalleTexto)
                                ? null
                                : JsonDocument.Parse(detalleTexto).RootElement
                        });
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaBalanceConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaBalanceConsulta>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticiaDetalle_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchPaises", MySqlDbType.VarChar, -1).Value = (object?)filtro.Paises ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchActividades", MySqlDbType.VarChar, -1).Value = (object?)filtro.Actividades ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = filtro.NumPag;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new CompaniaNoticiaDetalleListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstCompaniaNoticiasDetalle.Add(new CompaniaNoticiaDetalleListaConsulta
                            {
                                IdCompania = Convert.ToInt32(dr["IdCompania"]),
                                NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                                NumeroDocumento = GetNullableString(dr, "NumeroDocumento"),
                                Pais = GetNullableString(dr, "Pais"),
                                Bandera = GetNullableString(dr, "Bandera"),
                                Direccion = GetNullableString(dr, "Direccion"),
                                Telefono = GetNullableString(dr, "Telefono"),
                                ActividadComercial = GetNullableString(dr, "ActividadComercial")
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new CompaniaNoticiaDetalleListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaNoticiaDetalleListaResult() };
            }
        }

        public async Task<Respuesta> ExportarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_CompaniaNoticiaDetalle_Exportar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<CompaniaNoticiaDetalleListaConsulta>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(new CompaniaNoticiaDetalleListaConsulta
                        {
                            IdCompania = Convert.ToInt32(dr["IdCompania"]),
                            NombreCompleto = GetNullableString(dr, "NombreCompleto"),
                            Pais = GetNullableString(dr, "Pais"),
                            Bandera = GetNullableString(dr, "Bandera"),
                            Direccion = GetNullableString(dr, "Direccion"),
                            Telefono = GetNullableString(dr, "Telefono"),
                            ActividadComercial = GetNullableString(dr, "ActividadComercial")
                        });
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<CompaniaNoticiaDetalleListaConsulta>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaDetalleListaConsulta>() };
            }
        }

    }
}
