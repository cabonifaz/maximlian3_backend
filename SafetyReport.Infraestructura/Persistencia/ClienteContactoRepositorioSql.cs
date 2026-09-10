using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.ClienteContacto;
using System.Data;

namespace SafetyReport.Infrastructure.Persistencia
{
    public class ClienteContactoRepositorioSql : IClienteContactoRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<ClienteContactoRepositorioSql> _logger;

        public ClienteContactoRepositorioSql(DbConfig dbConfig, ILogger<ClienteContactoRepositorioSql> logger)
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

        private static async Task<List<T>> LeerIdsAsync<T>(MySqlDataReader dr, string columnName, Func<int?, T> factory)
        {
            var lista = new List<T>();

            while (await dr.ReadAsync())
            {
                lista.Add(factory(GetNullableInt(dr, columnName)));
            }

            return lista;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, ClienteContactoCrear request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_ClienteContacto_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_vchCodigo", MySqlDbType.VarChar, 32).Value = (object?)request.Codigo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombres", MySqlDbType.VarChar, 255).Value = request.Nombres;
                cmd.Parameters.Add("@p_intIdTipoPersonaContacto", MySqlDbType.Int32).Value = request.IdTipoPersonaContacto;
                cmd.Parameters.Add("@p_intIdTipoContacto", MySqlDbType.Int32).Value = request.IdTipoContacto;
                cmd.Parameters.Add("@p_vchTipoContacto", MySqlDbType.VarChar, 255).Value = (object?)request.TipoContacto ?? DBNull.Value;
                cmd.Parameters.Add("@p_intIdAreaTrabajo", MySqlDbType.Int32).Value = request.IdAreaTrabajo;
                cmd.Parameters.Add("@p_vchTelefono", MySqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCorreo", MySqlDbType.VarChar, 100).Value = (object?)request.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@p_bitEnviarCorreo", MySqlDbType.Bool).Value = request.EnviarCorreo;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdClienteContacto", id => new ClienteContactoCreado { IdClienteContacto = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<ClienteContactoCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoCreado>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoFiltro request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_ClienteContacto_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;
                cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@p_numPag", MySqlDbType.Int32).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var resultado = new ClienteContactoListaResult();

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Convert.ToInt32(dr["TotalRegistros"]);
                        resultado.TotalPaginas = Convert.ToInt32(dr["TotalPaginas"]);
                    }

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            resultado.lstClienteContactos.Add(new ClienteContactoListaDetalleResult
                            {
                                IdClienteContacto = Convert.ToInt32(dr["IdClienteContacto"]),
                                Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                                TipoContacto = dr["TipoContacto"]?.ToString() ?? string.Empty,
                                AreaTrabajo = dr["AreaTrabajo"]?.ToString() ?? string.Empty,
                                Telefono = GetNullableString(dr, "Telefono"),
                                Correo = GetNullableString(dr, "Correo"),
                                EnviarCorreo = Convert.ToBoolean(dr["EnviarCorreo"])
                            });
                        }
                    }

                    respuesta.Result = resultado;
                }
                else
                {
                    respuesta.Result = new ClienteContactoListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteContactoListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ClienteContactoIdRequest request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_ClienteContacto_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdClienteContacto", MySqlDbType.Int32).Value = request.idClienteContacto;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    var lista = new List<ClienteContactoSeleccionado>();

                    while (await dr.ReadAsync())
                    {
                        lista.Add(new ClienteContactoSeleccionado
                        {
                            IdClienteContacto = Convert.ToInt32(dr["IdClienteContacto"]),
                            IdCliente = Convert.ToInt32(dr["IdCliente"]),
                            Codigo = GetNullableString(dr, "Codigo"),
                            Nombres = dr["Nombres"]?.ToString() ?? string.Empty,
                            IdTipoPersonaContacto = Convert.ToInt32(dr["IdTipoPersonaContacto"]),
                            IdTipoContacto = Convert.ToInt32(dr["IdTipoContacto"]),
                            IdAreaTrabajo = Convert.ToInt32(dr["IdAreaTrabajo"]),
                            Telefono = GetNullableString(dr, "Telefono"),
                            Correo = GetNullableString(dr, "Correo"),
                            EnviarCorreo = Convert.ToBoolean(dr["EnviarCorreo"])
                        });
                    }

                    respuesta.Result = lista;
                }
                else
                {
                    respuesta.Result = new List<ClienteContactoSeleccionado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoSeleccionado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoEditar request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_ClienteContacto_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdClienteContacto", MySqlDbType.Int32).Value = request.IdClienteContacto;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.IdCliente;
                cmd.Parameters.Add("@p_vchCodigo", MySqlDbType.VarChar, 32).Value = (object?)request.Codigo ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchNombres", MySqlDbType.VarChar, 255).Value = request.Nombres;
                cmd.Parameters.Add("@p_intIdTipoPersonaContacto", MySqlDbType.Int32).Value = request.IdTipoPersonaContacto;
                cmd.Parameters.Add("@p_intIdTipoContacto", MySqlDbType.Int32).Value = request.IdTipoContacto;
                cmd.Parameters.Add("@p_intIdAreaTrabajo", MySqlDbType.Int32).Value = request.IdAreaTrabajo;
                cmd.Parameters.Add("@p_vchTelefono", MySqlDbType.VarChar, 128).Value = (object?)request.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@p_vchCorreo", MySqlDbType.VarChar, 100).Value = (object?)request.Correo ?? DBNull.Value;
                cmd.Parameters.Add("@p_bitEnviarCorreo", MySqlDbType.Bool).Value = request.EnviarCorreo;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdClienteContacto", id => new ClienteContactoCreado { IdClienteContacto = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<ClienteContactoCreado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoIdRequest request)
        {
            try
            {
                using MySqlConnection cn = new MySqlConnection(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new MySqlCommand("SP_ClienteContacto_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@p_intIdClienteContacto", MySqlDbType.Int32).Value = request.idClienteContacto;
                cmd.Parameters.Add("@p_intIdCliente", MySqlDbType.Int32).Value = request.idCliente;

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, cmd.CommandText);

                if (respuesta.IdTipoMensaje == 2 && await dr.NextResultAsync())
                {
                    respuesta.Result = await LeerIdsAsync(dr, "IdClienteContacto", id => new ClienteContactoEliminado { IdClienteContacto = id ?? 0 });
                }
                else
                {
                    respuesta.Result = new List<ClienteContactoEliminado>();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteContactoEliminado>()
                };
            }
        }
    }
}
