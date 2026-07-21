using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

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
        private async Task<Respuesta> LeerCabeceraAsync(SqlDataReader dr, string procedimiento)
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

        private static int? GetNullableInt(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static decimal? GetNullableDecimal(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        private static DateTime? GetNullableDateTime(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDateTime(dr[columna]);

        private static string? GetNullableString(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static TablaMaestraItem LeerTablaMaestraItem(SqlDataReader dr) => new()
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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_TablaMaestra_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchIdsMaestro", SqlDbType.VarChar, -1).Value = (object?)idsMaestro ?? DBNull.Value;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)numPag ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_InventarioMaestros_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdMaestro", SqlDbType.Int).Value = (object?)idMaestro ?? DBNull.Value;

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

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TablaMaestraRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_TablaMaestra_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdMaestro", SqlDbType.Int).Value = request.IdMaestro;
                cmd.Parameters.Add("@vchDescripcion", SqlDbType.VarChar, 255).Value = request.Descripcion;
                cmd.Parameters.Add("@intNum1", SqlDbType.Int).Value = (object?)request.Num1 ?? DBNull.Value;

                cmd.Parameters.Add("@decNum2", SqlDbType.Decimal).Value = (object?)request.Num2 ?? DBNull.Value;
                cmd.Parameters["@decNum2"].Precision = 18;
                cmd.Parameters["@decNum2"].Scale = 6;

                cmd.Parameters.Add("@decNum3", SqlDbType.Decimal).Value = (object?)request.Num3 ?? DBNull.Value;
                cmd.Parameters["@decNum3"].Precision = 18;
                cmd.Parameters["@decNum3"].Scale = 6;

                cmd.Parameters.Add("@vchString1", SqlDbType.VarChar, 255).Value = (object?)request.String1 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString2", SqlDbType.VarChar, 255).Value = (object?)request.String2 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString3", SqlDbType.NVarChar, 255).Value = (object?)request.String3 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString4", SqlDbType.VarChar, 255).Value = (object?)request.String4 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString5", SqlDbType.VarChar, 255).Value = (object?)request.String5 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString6", SqlDbType.VarChar, 255).Value = (object?)request.String6 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString7", SqlDbType.VarChar, 255).Value = (object?)request.String7 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate1", SqlDbType.DateTime).Value = (object?)request.Date1 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate2", SqlDbType.DateTime).Value = (object?)request.Date2 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate3", SqlDbType.DateTime).Value = (object?)request.Date3 ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_TablaMaestra_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdMaestro", SqlDbType.Int).Value = request.IdMaestro;
                cmd.Parameters.Add("@intNum1", SqlDbType.Int).Value = (object?)request.Num1 ?? DBNull.Value;

                cmd.Parameters.Add("@decNum2", SqlDbType.Decimal).Value = (object?)request.Num2 ?? DBNull.Value;
                cmd.Parameters["@decNum2"].Precision = 18;
                cmd.Parameters["@decNum2"].Scale = 6;

                cmd.Parameters.Add("@decNum3", SqlDbType.Decimal).Value = (object?)request.Num3 ?? DBNull.Value;
                cmd.Parameters["@decNum3"].Precision = 18;
                cmd.Parameters["@decNum3"].Scale = 6;

                cmd.Parameters.Add("@vchString1", SqlDbType.VarChar, 255).Value = (object?)request.String1 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString2", SqlDbType.VarChar, 255).Value = (object?)request.String2 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString3", SqlDbType.NVarChar, 255).Value = (object?)request.String3 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString4", SqlDbType.VarChar, 255).Value = (object?)request.String4 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString5", SqlDbType.VarChar, 255).Value = (object?)request.String5 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString6", SqlDbType.VarChar, 255).Value = (object?)request.String6 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString7", SqlDbType.VarChar, 255).Value = (object?)request.String7 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate1", SqlDbType.DateTime).Value = (object?)request.Date1 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate2", SqlDbType.DateTime).Value = (object?)request.Date2 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate3", SqlDbType.DateTime).Value = (object?)request.Date3 ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_TablaMaestra_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario",  SqlDbType.Int).Value         = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario",    SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa",  SqlDbType.Int).Value         = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol",      SqlDbType.Int).Value         = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdMaestro",  SqlDbType.Int).Value         = request.idMaestro;
                cmd.Parameters.Add("@intIdBusqueda", SqlDbType.Int).Value         = (object?)request.idBusqueda ?? DBNull.Value;
                cmd.Parameters.Add("@vchBusqueda",   SqlDbType.VarChar).Value     = (object?)request.vchBusqueda ?? DBNull.Value;

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_TablaMaestra_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdTablaMaestra", SqlDbType.Int).Value = idTablaMaestra;

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
