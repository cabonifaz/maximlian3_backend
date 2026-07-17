using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class CompaniaDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<CompaniaDAO> _logger;

        public CompaniaDAO(DbConfig dbConfig, ILogger<CompaniaDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        private static int? GetNullableInt(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static long? GetNullableLong(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (long?)null : Convert.ToInt64(value);
        }

        private static string? GetNullableString(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private static bool? GetNullableBool(SqlDataReader dr, string columnName)
        {
            var value = dr[columnName];
            return value == DBNull.Value ? (bool?)null : Convert.ToBoolean(value);
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

        private static CompaniaConsulta LeerCompaniaConsulta(SqlDataReader dr)
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
                ExisteInformacion = GetNullableBool(dr, "ExisteInformacion")
            };
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<CompaniaCrear> lstCompanias)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Compania_Insertar", cn) { CommandType = CommandType.StoredProcedure };
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Compania_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Compania_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)request.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@vchNumDocumento", SqlDbType.VarChar, 255).Value = (object?)request.NumDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombre", SqlDbType.VarChar, 255).Value = (object?)request.Nombre ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Compania_Listar", cn) { CommandType = CommandType.StoredProcedure };
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
                            resultado.lstCompanias.Add(LeerCompaniaConsulta(dr));
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

        public async Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<CompaniaMatchItem> lista)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Compania_ObtenerCoincidencias", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@jsonLista", SqlDbType.NVarChar, -1).Value = JsonSerializer.Serialize(lista);

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Compania_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = idCompania;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticia_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = request.IdCompania;
                cmd.Parameters.Add("@vchTitulo", SqlDbType.VarChar, 1024).Value = (object?)request.Titulo ?? DBNull.Value;
                cmd.Parameters.Add("@vchDescripcion", SqlDbType.VarChar, -1).Value = (object?)request.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@dtFechaNoticia", SqlDbType.DateTime).Value = (object?)request.FechaNoticia ?? DBNull.Value;
                cmd.Parameters.Add("@vchCategoria", SqlDbType.VarChar, 255).Value = (object?)request.Categoria ?? DBNull.Value;

                var tvpArchivos = new DataTable();
                tvpArchivos.Columns.Add("IdCompaniaNoticiaArchivo", typeof(int));
                tvpArchivos.Columns.Add("IdTipoArchivo", typeof(int));
                tvpArchivos.Columns.Add("ArchivoUrl", typeof(string));
                tvpArchivos.Columns.Add("NombreDocumento", typeof(string));
                foreach (var archivo in request.Archivos)
                    tvpArchivos.Rows.Add(
                        (object?)archivo.IdCompaniaNoticiaArchivo ?? DBNull.Value,
                        archivo.IdTipoArchivo,
                        (object?)archivo.ArchivoUrl ?? DBNull.Value,
                        (object?)(archivo.NombreDocumento ?? archivo.NombreArchivo) ?? DBNull.Value);

                var paramTvp = cmd.Parameters.Add("@lstArchivos", SqlDbType.Structured);
                paramTvp.TypeName = "LISTA_COMPANIA_NOTICIA_ARCHIVO";
                paramTvp.Value = tvpArchivos;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticia_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompaniaNoticia", SqlDbType.Int).Value = request.IdCompaniaNoticia;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = request.IdCompania;
                cmd.Parameters.Add("@vchTitulo", SqlDbType.VarChar, 1024).Value = (object?)request.Titulo ?? DBNull.Value;
                cmd.Parameters.Add("@vchDescripcion", SqlDbType.VarChar, -1).Value = (object?)request.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@dtFechaNoticia", SqlDbType.DateTime).Value = (object?)request.FechaNoticia ?? DBNull.Value;
                cmd.Parameters.Add("@vchCategoria", SqlDbType.VarChar, 255).Value = (object?)request.Categoria ?? DBNull.Value;

                var tvpArchivos = new DataTable();
                tvpArchivos.Columns.Add("IdCompaniaNoticiaArchivo", typeof(int));
                tvpArchivos.Columns.Add("IdTipoArchivo", typeof(int));
                tvpArchivos.Columns.Add("ArchivoUrl", typeof(string));
                tvpArchivos.Columns.Add("NombreDocumento", typeof(string));
                foreach (var archivo in request.Archivos)
                    tvpArchivos.Rows.Add(
                        (object?)archivo.IdCompaniaNoticiaArchivo ?? DBNull.Value,
                        archivo.IdTipoArchivo,
                        (object?)archivo.ArchivoUrl ?? DBNull.Value,
                        (object?)(archivo.NombreDocumento ?? archivo.NombreArchivo) ?? DBNull.Value);

                var paramTvp = cmd.Parameters.Add("@lstArchivos", SqlDbType.Structured);
                paramTvp.TypeName = "LISTA_COMPANIA_NOTICIA_ARCHIVO";
                paramTvp.Value = tvpArchivos;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticia_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompaniaNoticia", SqlDbType.Int).Value = (object?)request.IdCompaniaNoticia ?? DBNull.Value;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)request.IdCompania ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticiaArchivo_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompaniaNoticiaArchivo", SqlDbType.Int).Value = idCompaniaNoticiaArchivo;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticiaArchivo_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompaniaNoticiaArchivo", SqlDbType.Int).Value = idCompaniaNoticiaArchivo;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticia_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@dtmFchInicio", SqlDbType.Date).Value = (object?)filtro.FchInicio ?? DBNull.Value;
                cmd.Parameters.Add("@dtmFchFin", SqlDbType.Date).Value = (object?)filtro.FchFin ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = filtro.NumPag;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticia_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompaniaNoticia", SqlDbType.Int).Value = idCompaniaNoticia;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticiaBalance_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@vchTipoEstadoFinanciero", SqlDbType.VarChar, -1).Value = (object?)filtro.TipoEstadoFinanciero ?? DBNull.Value;
                cmd.Parameters.Add("@vchEstado", SqlDbType.VarChar, -1).Value = (object?)filtro.Estado ?? DBNull.Value;
                cmd.Parameters.Add("@dtmFchInicio", SqlDbType.Date).Value = (object?)filtro.FchInicio ?? DBNull.Value;
                cmd.Parameters.Add("@dtmFchFin", SqlDbType.Date).Value = (object?)filtro.FchFin ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = filtro.NumPag;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticiaBalance_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdInformeBalance", SqlDbType.Int).Value = (object?)request.IdInformeBalance ?? DBNull.Value;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)request.IdCompania ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticiaDetalle_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@vchPaises", SqlDbType.VarChar, -1).Value = (object?)filtro.Paises ?? DBNull.Value;
                cmd.Parameters.Add("@vchActividades", SqlDbType.VarChar, -1).Value = (object?)filtro.Actividades ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = filtro.NumPag;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_CompaniaNoticiaDetalle_Exportar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = (object?)filtro.IdCompania ?? DBNull.Value;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;

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
