using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;

namespace SafetyReport.DAO
{
    public class TablaMaestraDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<TablaMaestraDAO> _logger;

        public TablaMaestraDAO(DbConfig dbConfig, ILogger<TablaMaestraDAO> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje. Sin columna Result.
        private async Task<Respuesta> LeerCabeceraAsync(DbDataReader dr, string procedimiento)
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
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", procedimiento);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
            }

            return respuesta;
        }

        private static int? GetNullableInt(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static DateTime? GetNullableDateTime(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDateTime(dr[columna]);

        private static string? GetNullableString(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static TablaMaestraItem LeerTablaMaestraItem(DbDataReader dr) => new()
        {
            IdEmpresa = GetNullableInt(dr, "IdEmpresa"),
            IdTablaMaestra = GetNullableInt(dr, "IdTablaMaestra"),
            IdMaestro = GetNullableInt(dr, "IdMaestro"),
            Descripcion = GetNullableString(dr, "Descripcion"),
            Num1 = GetNullableInt(dr, "Num1"),
            Num2 = GetNullableDecimal(dr, "Num2"),
            Num3 = GetNullableDecimal(dr, "Num3"),
            String1 = GetNullableString(dr, "String1"),
            String2 = GetNullableString(dr, "String2"),
            String3 = GetNullableString(dr, "String3"),
            String4 = GetNullableString(dr, "String4"),
            String5 = GetNullableString(dr, "String5"),
            String6 = GetNullableString(dr, "String6"),
            String7 = GetNullableString(dr, "String7"),
            Date1 = GetNullableDateTime(dr, "Date1"),
            Date2 = GetNullableDateTime(dr, "Date2"),
            Date3 = GetNullableDateTime(dr, "Date3")
        };

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, string? idsMaestro, string? busqueda, int? numPag)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_TablaMaestra_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchIdsMaestro", (object?)idsMaestro ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new TablaMaestraListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        var gruposPorId = new Dictionary<int, TablaMaestraGroup>();
                        while (await dr.ReadAsync())
                        {
                            var item = LeerTablaMaestraItem(dr);
                            var idMaestro = item.IdMaestro ?? 0;

                            if (!gruposPorId.TryGetValue(idMaestro, out var grupo))
                            {
                                grupo = new TablaMaestraGroup { IdMaestro = idMaestro, Items = new List<TablaMaestraItem>() };
                                gruposPorId[idMaestro] = grupo;
                                resultado.lstTablaMaestra.Add(grupo);
                            }

                            grupo.Items!.Add(item);
                        }
                    }
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new TablaMaestraListaResult()
                };
            }
        }

        public async Task<Respuesta> ListarInventarioAsync(UsuarioGeneral usuarioLogueado, int? idMaestro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_InventarioMaestros_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdMaestro", (object?)idMaestro ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<InventarioMaestroItem>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        lista.Add(new InventarioMaestroItem
                        {
                            IdInventario = Convert.ToInt32(dr["IdInventario"]),
                            IdMaestro = Convert.ToInt32(dr["IdMaestro"]),
                            Descripcion = GetNullableString(dr, "Descripcion"),
                            Num1 = GetNullableString(dr, "Num1"),
                            Num2 = GetNullableString(dr, "Num2"),
                            Num3 = GetNullableString(dr, "Num3"),
                            String1 = GetNullableString(dr, "String1"),
                            String2 = GetNullableString(dr, "String2"),
                            String3 = GetNullableString(dr, "String3"),
                            Date1 = GetNullableString(dr, "Date1"),
                            Date2 = GetNullableString(dr, "Date2"),
                            Date3 = GetNullableString(dr, "Date3")
                        });
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<InventarioMaestroItem>()
                };
            }
        }

        public async Task<Respuesta> ListaCortaAsync(UsuarioGeneral usuarioLogueado, int idMaestro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_TablaMaestra_ListaCorta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdMaestro", idMaestro);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<TablaMaestraCortaItem>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        lista.Add(new TablaMaestraCortaItem
                        {
                            Num1 = Convert.ToInt32(dr["Num1"]),
                            String1 = GetNullableString(dr, "String1"),
                            String2 = GetNullableString(dr, "String2"),
                            String3 = GetNullableString(dr, "String3")
                        });
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraCortaItem>()
                };
            }
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TablaMaestraRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_TablaMaestra_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                cmd.Parameters.AddWithValue("@intIdMaestro", request.IdMaestro);
                cmd.Parameters.AddWithValue("@vchDescripcion", request.Descripcion);
                cmd.Parameters.AddWithValue("@intNum1", (object?)request.Num1 ?? DBNull.Value);

                var decNum2 = cmd.Parameters.AddWithValue("@decNum2", (object?)request.Num2 ?? DBNull.Value);
                decNum2.Precision = 18;
                decNum2.Scale = 6;

                var decNum3 = cmd.Parameters.AddWithValue("@decNum3", (object?)request.Num3 ?? DBNull.Value);
                decNum3.Precision = 18;
                decNum3.Scale = 6;

                cmd.Parameters.AddWithValue("@vchString1", (object?)request.String1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString2", (object?)request.String2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString3", (object?)request.String3 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString4", (object?)request.String4 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString5", (object?)request.String5 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString6", (object?)request.String6 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString7", (object?)request.String7 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtDate1", (object?)request.Date1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtDate2", (object?)request.Date2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtDate3", (object?)request.Date3 ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<TablaMaestraResultado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new TablaMaestraResultado { IdTablaMaestra = Convert.ToInt32(dr["IdTablaMaestra"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarTablaMaestraRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_TablaMaestra_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);

                cmd.Parameters.AddWithValue("@intIdMaestro", request.IdMaestro);
                cmd.Parameters.AddWithValue("@intNum1", (object?)request.Num1 ?? DBNull.Value);

                var decNum2 = cmd.Parameters.AddWithValue("@decNum2", (object?)request.Num2 ?? DBNull.Value);
                decNum2.Precision = 18;
                decNum2.Scale = 6;

                var decNum3 = cmd.Parameters.AddWithValue("@decNum3", (object?)request.Num3 ?? DBNull.Value);
                decNum3.Precision = 18;
                decNum3.Scale = 6;

                cmd.Parameters.AddWithValue("@vchString1", (object?)request.String1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString2", (object?)request.String2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString3", (object?)request.String3 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString4", (object?)request.String4 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString5", (object?)request.String5 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString6", (object?)request.String6 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchString7", (object?)request.String7 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtDate1", (object?)request.Date1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtDate2", (object?)request.Date2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtDate3", (object?)request.Date3 ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<TablaMaestraResultado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new TablaMaestraResultado { IdTablaMaestra = Convert.ToInt32(dr["IdTablaMaestra"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ObtenerTablaMaestraRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_TablaMaestra_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdMaestro", request.idMaestro);
                cmd.Parameters.AddWithValue("@intIdBusqueda", (object?)request.idBusqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)request.vchBusqueda ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<TablaMaestraItem>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                        lista.Add(LeerTablaMaestraItem(dr));
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraItem>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idTablaMaestra)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_TablaMaestra_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdTablaMaestra", idTablaMaestra);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<TablaMaestraResultado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                    lista.Add(new TablaMaestraResultado { IdTablaMaestra = Convert.ToInt32(dr["IdTablaMaestra"]) });

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }
    }
}
