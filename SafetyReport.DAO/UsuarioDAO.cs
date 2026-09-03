using Microsoft.Extensions.Logging;
using MySqlConnector;
using SafetyReport.Models;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class UsuarioDAO
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<UsuarioDAO> _logger;

        public UsuarioDAO(DbConfig dbConfig, ILogger<UsuarioDAO> logger)
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

        private static string? GetNullableString(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static decimal? GetNullableDecimal(DbDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToDecimal(dr[columna]);

        public async Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchNombres", request.Nombres);
                cmd.Parameters.AddWithValue("@vchApellidoPaterno", request.ApellidoPaterno);
                cmd.Parameters.AddWithValue("@vchApellidoMaterno", (object?)request.ApellidoMaterno ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchCorreo", request.Correo);
                cmd.Parameters.AddWithValue("@vchUsuarioCreado", request.usuarioCreacion);
                cmd.Parameters.AddWithValue("@lstRoles", JsonSerializer.Serialize((request.Roles ?? new List<int>()).Select(id => new { NUM1 = id })));
                cmd.Parameters.AddWithValue("@lstIdiomas", JsonSerializer.Serialize((request.Idiomas ?? new List<int>()).Select(id => new { NUM1 = id })));

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    respuesta.Result = new UsuarioCreado
                    {
                        IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                        Usuario = dr["Usuario"]?.ToString() ?? string.Empty
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> EditarUsuarioAsync(UsuarioGeneral usuarioLogueado, InfoUsuarioEditar request)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuarioMOD", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@intIdUsuarioEditar", request.IdUsuario);
                cmd.Parameters.AddWithValue("@vchNombres", request.Nombres);
                cmd.Parameters.AddWithValue("@vchApellidoPaterno", request.ApellidoPaterno);
                cmd.Parameters.AddWithValue("@vchApellidoMaterno", (object?)request.ApellidoMaterno ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEstado", request.IdEstado);
                cmd.Parameters.AddWithValue("@lstRoles", JsonSerializer.Serialize((request.Roles ?? new List<int>()).Select(id => new { NUM1 = id })));
                cmd.Parameters.AddWithValue("@lstIdiomas", JsonSerializer.Serialize((request.Idiomas ?? new List<int>()).Select(id => new { NUM1 = id })));

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    respuesta.Result = new UsuarioCreado
                    {
                        IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                        Usuario = dr["Usuario"]?.ToString() ?? string.Empty
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> EliminarUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioEliminar)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioActual.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioActual.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioActual.IdRol);
                cmd.Parameters.AddWithValue("@intIdUsuarioEliminar", idUsuarioEliminar);

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    respuesta.Result = new EliminarUsuarioResult
                    {
                        IdUsuarioEliminar = Convert.ToInt32(dr["IdUsuarioEliminar"]),
                        Usuario = dr["Usuario"]?.ToString() ?? string.Empty
                    };
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> ListarUsuariosAsync(UsuarioGeneral usuarioActual, string? filtro, int? idEstado, int? numPag)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioActual.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioActual.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioActual.IdRol);
                cmd.Parameters.AddWithValue("@vchFiltro", (object?)filtro ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdEstado", (object?)idEstado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)numPag ?? DBNull.Value);

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2)
                {
                    var resultado = new UsuarioListaResult();

                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstUsuarios.Add(new UsuarioListaConsulta
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]),
                                Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                                ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                                ApellidoMaterno = GetNullableString(dr, "ApellidoMaterno"),
                                Correo = dr["Correo"]?.ToString() ?? string.Empty,
                                Usuario = dr["Usuario"]?.ToString() ?? string.Empty,
                                Roles = GetNullableString(dr, "Roles"),
                                Estado = GetNullableString(dr, "Estado")
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> ObtenerUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioConsulta)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioActual.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioActual.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioActual.IdRol);
                cmd.Parameters.AddWithValue("@intIdUsuarioConsulta", idUsuarioConsulta);

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2)
                {
                    var consulta = new UsuarioConsulta();

                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                    {
                        consulta.IdUsuario = Convert.ToInt32(dr["IdUsuario"]);
                        consulta.IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]);
                        consulta.Nombres = dr["Nombres"]?.ToString() ?? string.Empty;
                        consulta.ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty;
                        consulta.ApellidoMaterno = GetNullableString(dr, "ApellidoMaterno");
                        consulta.Correo = dr["Correo"]?.ToString() ?? string.Empty;
                        consulta.Usuario = dr["Usuario"]?.ToString() ?? string.Empty;
                        consulta.IdEstado = Convert.ToInt32(dr["IdEstado"]);
                        consulta.Estado = GetNullableString(dr, "Estado");
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            consulta.Roles.Add(Convert.ToInt32(dr["IdRol"]));
                        }
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            consulta.Idiomas.Add(Convert.ToInt32(dr["IdIdioma"]));
                        }
                    }

                    respuesta.Result = consulta;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> ListarCortaAsync(UsuarioGeneral usuarioActual, int idRolFiltro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Listar_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioActual.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioActual.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioActual.IdRol);
                cmd.Parameters.AddWithValue("@intIdRolFiltro", idRolFiltro);

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2)
                {
                    var lista = new List<UsuarioListaCortaItem>();

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new UsuarioListaCortaItem
                            {
                                Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                                ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                                ApellidoMaterno = GetNullableString(dr, "ApellidoMaterno"),
                                Correo = dr["Correo"]?.ToString() ?? string.Empty
                            });
                        }
                    }

                    respuesta.Result = lista;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        // Separado de ListarCortaAsync (SP_Usuario_Listar_Corta, sigue igual) — este llama a
        // SP_Usuario_ListaCortaDashboard, que acepta varios roles a la vez.
        public async Task<Respuesta> ListarCortaDashboardAsync(UsuarioGeneral usuarioActual, List<int>? idsRolFiltro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_ListaCortaDashboard", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioActual.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioActual.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioActual.IdRol);
                cmd.Parameters.AddWithValue("@vchIdRolFiltro",
                    idsRolFiltro is { Count: > 0 } ? (object)string.Join(",", idsRolFiltro) : DBNull.Value);

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2)
                {
                    var lista = new List<UsuarioListaCortaDashboardItem>();

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new UsuarioListaCortaDashboardItem
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                                ApellidoPaterno = dr["ApellidoPaterno"]?.ToString() ?? string.Empty,
                                ApellidoMaterno = GetNullableString(dr, "ApellidoMaterno")
                            });
                        }
                    }

                    respuesta.Result = lista;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> ListarCortaAsignacionAsync(UsuarioGeneral usuarioActual, int idRolFiltro, string? filtro, bool esTraductor, List<int>? idiomasPedido)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_UsuarioAsignacion_ListaCorta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioActual.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioActual.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioActual.IdRol);
                cmd.Parameters.AddWithValue("@intIdRolFiltro", idRolFiltro);
                cmd.Parameters.AddWithValue("@vchFiltro", (object?)filtro ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bitEsTraductor", esTraductor);
                cmd.Parameters.AddWithValue("@lstIdiomasPedido", JsonSerializer.Serialize(idiomasPedido ?? new List<int>()));

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2)
                {
                    var lista = new List<UsuarioAsignacionListaCortaItem>();

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new UsuarioAsignacionListaCortaItem
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                Iniciales = dr["Iniciales"]?.ToString() ?? string.Empty,
                                NombreCompleto = dr["NombreCompleto"]?.ToString() ?? string.Empty,
                                CantidadIdiomas = GetNullableInt(dr, "CantidadIdiomas"),
                                CantidadIdiomasCoincidentes = GetNullableInt(dr, "CantidadIdiomasCoincidentes"),
                                CantidadAsignaciones = Convert.ToInt32(dr["CantidadAsignaciones"])
                            });
                        }
                    }

                    respuesta.Result = lista;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> ActualizarSubAsync(UsuarioGeneral usuarioActual, int idUsuarioActualizar, string sub)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Actualizar_Cognito", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioActual.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioActual.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioActual.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioActual.IdRol);
                cmd.Parameters.AddWithValue("@intIdUsuarioActualizar", idUsuarioActualizar);
                cmd.Parameters.AddWithValue("@vchSub", sub);

                await cn.OpenAsync();
                using MySqlDataReader dr = await cmd.ExecuteReaderAsync();

                return await LeerCabeceraAsync(dr, cmd.CommandText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<Respuesta> ObtenerResumenAsync(UsuarioGeneral usuarioLogueado, FiltroUsuarioResumen filtro)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new("SP_Usuario_Resumen", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intIdUsuario", usuarioLogueado.IdUsuario);
                cmd.Parameters.AddWithValue("@vchUsuario", usuarioLogueado.Usuario);
                cmd.Parameters.AddWithValue("@intIdEmpresa", usuarioLogueado.IdEmpresa);
                cmd.Parameters.AddWithValue("@intIdRol", usuarioLogueado.IdRol);
                cmd.Parameters.AddWithValue("@vchBusqueda", (object?)filtro.busqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@intIdRolAsignado", (object?)filtro.idRolAsignado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFchDesde", (object?)filtro.fchDesde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dtFchHasta", (object?)filtro.fchHasta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vchIdEficiencia", (object?)filtro.idEficiencia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numPag", (object?)filtro.numPag ?? DBNull.Value);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                var resultado = new UsuarioCumplimientoResult();
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
                            resultado.lstUsuarios.Add(new UsuarioCumplimientoItem
                            {
                                IdColaborador = Convert.ToInt32(dr["IdColaborador"]),
                                NombreCompleto = dr["NombreCompleto"]?.ToString() ?? string.Empty,
                                Iniciales = GetNullableString(dr, "Iniciales"),
                                IdRol = Convert.ToInt32(dr["IdRol"]),
                                DescripcionRol = GetNullableString(dr, "DescripcionRol"),
                                CantidadOrdenes = Convert.ToInt32(dr["CantidadOrdenes"]),
                                CantidadInformes = Convert.ToInt32(dr["CantidadInformes"]),
                                CantidadTardios = GetNullableInt(dr, "CantidadTardios"),
                                CantidadObservados = Convert.ToInt32(dr["CantidadObservados"]),
                                CantidadConInformacionFinanciera = Convert.ToInt32(dr["CantidadConInformacionFinanciera"]),
                                PorcentajeCumplimiento = GetNullableDecimal(dr, "PorcentajeCumplimiento") ?? 0
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
                    Result = new UsuarioCumplimientoResult()
                };
            }
        }
    }
}
