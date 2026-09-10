using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Banco;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class BancoRepositorioSql : IBancoRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<BancoRepositorioSql> _logger;

        public BancoRepositorioSql(DbConfig dbConfig, ILogger<BancoRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // ── Reader helpers ────────────────────────────────────────────────────────

        private static string? GetNullableString(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static int? GetNullableInt(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje.
        private async Task<Respuesta> LeerCabeceraAsync(MySqlDataReader dr, string procedimiento)
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

        // Lee un result set de una sola columna IdBanco (result set 2 en éxito).
        private static async Task<List<T>> LeerIdBancosAsync<T>(MySqlDataReader dr, Func<int?, T> map)
        {
            var lista = new List<T>();
            while (await dr.ReadAsync())
                lista.Add(map(GetNullableInt(dr, "IdBanco")));

            return lista;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<BancoCrear> lstBancos)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Banco_Insertar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;

                var json = JsonSerializer.Serialize(lstBancos.Select(item => new { item.IdPais, item.Nombre, item.Telefono }));
                cmd.Parameters.Add("@p_json_bancos", MySqlDbType.JSON).Value = json;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var creados = new List<BancoCreado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                    creados = await LeerIdBancosAsync(dr, id => new BancoCreado { IdBanco = id ?? 0 });

                respuesta.Result = creados;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoCreado>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, BancoEditar request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Banco_Actualizar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdBanco", MySqlDbType.Int32).Value = request.IdBanco;
                cmd.Parameters.Add("@p_intIdPais", MySqlDbType.Int32).Value = request.IdPais;
                cmd.Parameters.Add("@p_vchNombre", MySqlDbType.VarChar, 255).Value = request.Nombre;
                cmd.Parameters.Add("@p_vchTelefono", MySqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var actualizados = new List<BancoCreado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                    actualizados = await LeerIdBancosAsync(dr, id => new BancoCreado { IdBanco = id ?? 0 });

                respuesta.Result = actualizados;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoCreado>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, BancoObtenerRequest request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Banco_Obtener", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdBanco", MySqlDbType.Int32).Value = (object?)request.IdBanco ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombre", MySqlDbType.VarChar, 255).Value = (object?)request.Nombre ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<BancoConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new BancoConsulta
                        {
                            IdBanco = Convert.ToInt32(dr["IdBanco"]),
                            IdPais = Convert.ToInt32(dr["IdPais"]),
                            Pais = GetNullableString(dr, "Pais"),
                            Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                            Telefono = GetNullableString(dr, "Telefono")
                        });
                    }
                }

                respuesta.Result = lista;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroBanco filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Banco_Listar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = filtro.NumPag;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new BancoListaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstBancos.Add(new BancoListaConsulta
                            {
                                IdBanco = Convert.ToInt32(dr["IdBanco"]),
                                Pais = GetNullableString(dr, "Pais"),
                                Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                                Telefono = GetNullableString(dr, "Telefono")
                            });
                        }
                    }
                }

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new BancoListaResult() };
            }
        }

        public async Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<BancoMatchItem> lista)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Banco_ObtenerCoincidencias", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_json_lista", MySqlDbType.JSON).Value = JsonSerializer.Serialize(lista);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new List<BancoMatchResultItem>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                    resultado = await LeerIdBancosAsync(dr, id => new BancoMatchResultItem { IdBanco = id });

                respuesta.Result = resultado;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoMatchResultItem>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idBanco)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Banco_Eliminar", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdBanco", MySqlDbType.Int32).Value = idBanco;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var eliminados = new List<BancoEliminado>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                    eliminados = await LeerIdBancosAsync(dr, id => new BancoEliminado { IdBanco = id ?? 0 });

                respuesta.Result = eliminados;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<BancoEliminado>() };
            }
        }
    }
}
