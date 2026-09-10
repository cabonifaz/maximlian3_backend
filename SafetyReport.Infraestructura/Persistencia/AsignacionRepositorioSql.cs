using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Asignacion;
using System.Data;
using System.Text.Json;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class AsignacionRepositorioSql : IAsignacionRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<AsignacionRepositorioSql> _logger;

        public AsignacionRepositorioSql(DbConfig dbConfig, ILogger<AsignacionRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // Shape esperado por SP_Asignacion_Insertar (p_json_ids_pedido): array de {ID, NUM1} — el SP recorre
        // el JSON con un cursor "ORDER BY jt.ID", así que el orden de inserción debe viajar explícito.
        private static string ConstruirJsonListaGeneralNum(List<int>? valores) =>
            JsonSerializer.Serialize((valores ?? new List<int>()).Select((valor, i) => new { ID = i + 1, NUM1 = valor }));

        // Shape esperado por SP_Asignacion_Insertar/_Actualizar (p_json_asignados): array de
        // {ID, IdUsuarioAsignado, IdRolAsignado, IdEstado} — mismo motivo de ID explícito que arriba.
        private static string ConstruirJsonAsignados(List<AsignacionUsuario> asignados) =>
            JsonSerializer.Serialize(asignados.Select((a, i) => new { ID = i + 1, a.IdUsuarioAsignado, a.IdRolAsignado, a.IdEstado }));

        // ── Reader helpers ────────────────────────────────────────────────────────

        private static int? GetNullableInt(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static string? GetNullableString(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

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

        // Lee un result set de una sola columna IdAsignacion (result set 2 en éxito).
        private static async Task<List<T>> LeerIdAsignacionesAsync<T>(MySqlDataReader dr, Func<int, T> map)
        {
            var lista = new List<T>();
            while (await dr.ReadAsync())
                lista.Add(map(Convert.ToInt32(dr["IdAsignacion"])));

            return lista;
        }

        public async Task<Respuesta> InsertarAsync(UsuarioGeneral usuarioActual, AsignacionCrear request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Asignacion_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioActual.IdRol;

                cmd.Parameters.Add("@p_json_ids_pedido", MySqlDbType.JSON).Value = ConstruirJsonListaGeneralNum(request.IdsPedido);
                cmd.Parameters.Add("@p_json_asignados", MySqlDbType.JSON).Value = ConstruirJsonAsignados(request.Asignados);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var creadas = new List<AsignacionCreada>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                    creadas = await LeerIdAsignacionesAsync(dr, id => new AsignacionCreada { IdAsignacion = id });

                respuesta.Result = creadas;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionCreada>()
                };
            }
        }

        public async Task<Respuesta> ActualizarAsync(UsuarioGeneral usuarioActual, AsignacionActualizar request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Asignacion_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = request.IdPedido;
                cmd.Parameters.Add("@p_json_asignados", MySqlDbType.JSON).Value = ConstruirJsonAsignados(request.Asignados);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var actualizadas = new List<AsignacionCreada>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                    actualizadas = await LeerIdAsignacionesAsync(dr, id => new AsignacionCreada { IdAsignacion = id });

                respuesta.Result = actualizadas;
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionCreada>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioActual, FiltroAsignacion request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Asignacion_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdEstado", MySqlDbType.Int32).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new AsignacionListaResult();
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
                            resultado.lstAsignaciones.Add(new AsignacionListaConsulta
                            {
                                IdPedido = Convert.ToInt32(dr["IdPedido"]),
                                Cliente = GetNullableString(dr, "Cliente"),
                                Investigado = GetNullableString(dr, "Investigado"),
                                Analista = new AsignacionPersona
                                {
                                    IdAsignacion = GetNullableInt(dr, "AnalistaIdAsignacion"),
                                    Nombre = GetNullableString(dr, "AnalistaNombre")
                                },
                                Traductor = new AsignacionPersona
                                {
                                    IdAsignacion = GetNullableInt(dr, "TraductorIdAsignacion"),
                                    Nombre = GetNullableString(dr, "TraductorNombre")
                                },
                                IdEstado = GetNullableInt(dr, "IdEstado"),
                                DescripcionEstado = GetNullableString(dr, "DescripcionEstado"),
                                Vigencia = GetNullableString(dr, "Vigencia")
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

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new AsignacionListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioActual, int idAsignacion)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Asignacion_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@p_intIdAsignacion", MySqlDbType.Int32).Value = idAsignacion;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<AsignacionConsulta>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new AsignacionConsulta
                        {
                            IdAsignacion = Convert.ToInt32(dr["IdAsignacion"]),
                            IdPedido = Convert.ToInt32(dr["IdPedido"]),
                            CodigoPedido = dr["CodigoPedido"]?.ToString() ?? string.Empty,
                            Investigado = GetNullableString(dr, "Investigado"),
                            IdUsuarioAsignado = Convert.ToInt32(dr["IdUsuarioAsignado"]),
                            NombreUsuarioAsignado = dr["NombreUsuarioAsignado"]?.ToString() ?? string.Empty,
                            Iniciales = dr["Iniciales"]?.ToString() ?? string.Empty,
                            IdRolAsignado = Convert.ToInt32(dr["IdRolAsignado"]),
                            DescripcionRolAsignado = GetNullableString(dr, "DescripcionRolAsignado"),
                            IdEstado = Convert.ToInt32(dr["IdEstado"]),
                            DescripcionEstado = GetNullableString(dr, "DescripcionEstado"),
                            FechaAsignacion = Convert.ToDateTime(dr["FechaAsignacion"])
                        });
                    }
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
                    Result = new List<AsignacionConsulta>()
                };
            }
        }

        public async Task<Respuesta> BandejaAsync(UsuarioGeneral usuarioActual, FiltroAsignacionBandeja filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Asignacion_Bandeja", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = (object?)filtro.Busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_intNumPag", MySqlDbType.Int32).Value = filtro.NumPag;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new AsignacionBandejaResult();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                        resultado.Resumen = new AsignacionBandejaResumen
                        {
                            Total = Convert.ToInt32(dr["Total"]),
                            EnProceso = Convert.ToInt32(dr["EnProceso"]),
                            Aprobadas = Convert.ToInt32(dr["Aprobadas"]),
                            Rechazadas = Convert.ToInt32(dr["Rechazadas"])
                        };
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstAsignaciones.Add(new AsignacionBandejaItem
                            {
                                IdPedido = GetNullableInt(dr, "IdPedido"),
                                CodigoPedido = GetNullableString(dr, "CodigoPedido"),
                                IdInforme = GetNullableInt(dr, "IdInforme"),
                                IdInformeOriginal = GetNullableInt(dr, "IdInformeOriginal"),
                                Investigado = GetNullableString(dr, "Investigado"),
                                Pais = GetNullableString(dr, "Pais"),
                                Fecha = GetNullableString(dr, "Fecha"),
                                TipoTramite = GetNullableString(dr, "TipoTramite"),
                                IdEstado = GetNullableInt(dr, "IdEstado"),
                                Estado = GetNullableString(dr, "Estado"),
                                ColorLetra = GetNullableString(dr, "ColorLetra"),
                                ColorFondo = GetNullableString(dr, "ColorFondo")
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

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new AsignacionBandejaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioActual, EliminarAsignacion request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Asignacion_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@p_intIdAsignacion", MySqlDbType.Int32).Value = request.IdAsignacion;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var lista = new List<EliminarAsignacionResult>();
                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                    lista = await LeerIdAsignacionesAsync(dr, id => new EliminarAsignacionResult { IdAsignacion = id });

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
                    Result = new List<EliminarAsignacionResult>()
                };
            }
        }
    }
}
