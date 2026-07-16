using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Data;

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

        private static DataTable ConstruirTablaListaGeneralNum(List<int>? valores)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("NUM1", typeof(int));

            int i = 1;
            if (valores != null)
            {
                foreach (var valor in valores)
                {
                    table.Rows.Add(i++, valor);
                }
            }

            return table;
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

        private static string? GetNullableString(SqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        public async Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Usuario_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 50).Value = request.Nombres;
                cmd.Parameters.Add("@vchApellidoPaterno", SqlDbType.VarChar, 50).Value = request.ApellidoPaterno;
                cmd.Parameters.Add("@vchApellidoMaterno", SqlDbType.VarChar, 50).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
                cmd.Parameters.Add("@vchCorreo", SqlDbType.VarChar, 100).Value = request.Correo;
                cmd.Parameters.Add("@vchUsuarioCreado", SqlDbType.VarChar, 32).Value = request.usuarioCreacion;

                var tableRoles = ConstruirTablaListaGeneralNum(request.Roles);
                var tvpRoles = cmd.Parameters.AddWithValue("@lstRoles", tableRoles);
                tvpRoles.SqlDbType = SqlDbType.Structured;
                tvpRoles.TypeName = "LISTA_GENERAL_NUM";

                var tableIdiomas = ConstruirTablaListaGeneralNum(request.Idiomas);
                var tvpIdiomas = cmd.Parameters.AddWithValue("@lstIdiomas", tableIdiomas);
                tvpIdiomas.SqlDbType = SqlDbType.Structured;
                tvpIdiomas.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Usuario_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuarioMOD", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdUsuarioEditar", SqlDbType.Int).Value = request.IdUsuario;
                cmd.Parameters.Add("@vchNombres", SqlDbType.VarChar, 50).Value = request.Nombres;
                cmd.Parameters.Add("@vchApellidoPaterno", SqlDbType.VarChar, 50).Value = request.ApellidoPaterno;
                cmd.Parameters.Add("@vchApellidoMaterno", SqlDbType.VarChar, 50).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;

                var tableRoles = ConstruirTablaListaGeneralNum(request.Roles);
                var tvpRoles = cmd.Parameters.AddWithValue("@lstRoles", tableRoles);
                tvpRoles.SqlDbType = SqlDbType.Structured;
                tvpRoles.TypeName = "LISTA_GENERAL_NUM";

                var tableIdiomas = ConstruirTablaListaGeneralNum(request.Idiomas);
                var tvpIdiomas = cmd.Parameters.AddWithValue("@lstIdiomas", tableIdiomas);
                tvpIdiomas.SqlDbType = SqlDbType.Structured;
                tvpIdiomas.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Usuario_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdUsuarioEliminar", SqlDbType.Int).Value = idUsuarioEliminar;

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Usuario_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@vchFiltro", SqlDbType.VarChar, 255).Value = (object?)filtro ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = (object?)idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)numPag ?? DBNull.Value;

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Usuario_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdUsuarioConsulta", SqlDbType.Int).Value = idUsuarioConsulta;

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Usuario_Listar_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdRolFiltro", SqlDbType.Int).Value = idRolFiltro;

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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

        public async Task<Respuesta> ListarCortaAsignacionAsync(UsuarioGeneral usuarioActual, int idRolFiltro, string? filtro, bool esTraductor, List<int>? idiomasPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_UsuarioAsignacion_Listar_Corta", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdRolFiltro", SqlDbType.Int).Value = idRolFiltro;
                cmd.Parameters.Add("@vchFiltro", SqlDbType.VarChar, 255).Value = (object?)filtro ?? DBNull.Value;
                cmd.Parameters.Add("@bitEsTraductor", SqlDbType.Bit).Value = esTraductor;

                var tableIdiomas = ConstruirTablaListaGeneralNum(idiomasPedido);
                var tvpIdiomas = cmd.Parameters.AddWithValue("@lstIdiomasPedido", tableIdiomas);
                tvpIdiomas.SqlDbType = SqlDbType.Structured;
                tvpIdiomas.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("SP_Usuario_Actualizar_Cognito", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioActual.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioActual.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioActual.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioActual.IdRol;
                cmd.Parameters.Add("@intIdUsuarioActualizar", SqlDbType.Int).Value = idUsuarioActualizar;
                cmd.Parameters.Add("@vchSub", SqlDbType.VarChar, 255).Value = sub;

                await cn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

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
    }
}
