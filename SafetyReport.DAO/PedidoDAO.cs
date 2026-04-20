using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class PedidoDAO
    {
        private readonly DbConfig _dbConfig;

        public PedidoDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
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
                    table.Rows.Add(i++, valor);
            }

            return table;
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            var respuesta = new Respuesta();

            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 0;

                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                var json = dr["Result"]?.ToString();

                respuesta.Result = !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<T>()
                    : new List<T>();
            }
            else
            {
                respuesta.IdTipoMensaje = 1;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                respuesta.Result = new List<T>();
            }

            return respuesta;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Pedido request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Pedido_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 50).Value = request.Codigo;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCliente", SqlDbType.VarChar, 255).Value = (object?)request.NombreCliente ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = request.IdCompania;
                cmd.Parameters.Add("@vchNumeroDocumentoInvestigado", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@vchInvestigarRazonSocialNombres", SqlDbType.VarChar).Value = request.InvestigarRazonSocialNombres;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.IdTarifario;
                cmd.Parameters.Add("@intIdPlantilla", SqlDbType.Int).Value = request.IdPlantilla;
                cmd.Parameters.Add("@intIdIdioma", SqlDbType.Int).Value = request.IdIdioma;
                cmd.Parameters.Add("@intIdClaseInforme", SqlDbType.Int).Value = request.IdClaseInforme;
                cmd.Parameters.Add("@vchNumReferencia", SqlDbType.VarChar, 32).Value = (object?)request.NumReferencia ?? DBNull.Value;

                cmd.Parameters.Add("@decMontoCredito", SqlDbType.Decimal).Value = (object?)request.MontoCredito ?? DBNull.Value;
                cmd.Parameters["@decMontoCredito"].Precision = 18;
                cmd.Parameters["@decMontoCredito"].Scale = 2;

                cmd.Parameters.Add("@intPlazoCredito", SqlDbType.Int).Value = (object?)request.PlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPlazoCredito", SqlDbType.Int).Value = (object?)request.IdTipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@vchTipoPlazoCredito", SqlDbType.VarChar).Value = (object?)request.TipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchDesde", SqlDbType.DateTime).Value = (object?)request.FchDesde ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchHasta", SqlDbType.DateTime).Value = (object?)request.FchHasta ?? DBNull.Value;
                cmd.Parameters.Add("@vchComentario", SqlDbType.VarChar).Value = (object?)request.Comentario ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;
                cmd.Parameters.Add("@bitImprimeLogoSafety", SqlDbType.Bit).Value = request.ImprimeLogoSafety;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoCreado>()
                };
            }
        }
        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarPedido request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Pedido_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = request.IdPedido;
                cmd.Parameters.Add("@vchCodigo", SqlDbType.VarChar, 50).Value = request.Codigo;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = request.IdCliente;
                cmd.Parameters.Add("@vchNumeroDocumento", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumento ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreCliente", SqlDbType.VarChar, 255).Value = (object?)request.NombreCliente ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPersona", SqlDbType.Int).Value = request.IdTipoPersona;
                cmd.Parameters.Add("@intIdCompania", SqlDbType.Int).Value = request.IdCompania;
                cmd.Parameters.Add("@vchNumeroDocumentoInvestigado", SqlDbType.VarChar, 50).Value = (object?)request.NumeroDocumentoInvestigado ?? DBNull.Value;
                cmd.Parameters.Add("@vchInvestigarRazonSocialNombres", SqlDbType.VarChar).Value = request.InvestigarRazonSocialNombres;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = request.IdTarifario;
                cmd.Parameters.Add("@intIdPlantilla", SqlDbType.Int).Value = request.IdPlantilla;
                cmd.Parameters.Add("@intIdIdioma", SqlDbType.Int).Value = request.IdIdioma;
                cmd.Parameters.Add("@intIdClaseInforme", SqlDbType.Int).Value = request.IdClaseInforme;
                cmd.Parameters.Add("@vchNumReferencia", SqlDbType.VarChar, 32).Value = (object?)request.NumReferencia ?? DBNull.Value;

                cmd.Parameters.Add("@decMontoCredito", SqlDbType.Decimal).Value = (object?)request.MontoCredito ?? DBNull.Value;
                cmd.Parameters["@decMontoCredito"].Precision = 18;
                cmd.Parameters["@decMontoCredito"].Scale = 2;

                cmd.Parameters.Add("@intPlazoCredito", SqlDbType.Int).Value = (object?)request.PlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTipoPlazoCredito", SqlDbType.Int).Value = (object?)request.IdTipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@vchTipoPlazoCredito", SqlDbType.VarChar).Value = (object?)request.TipoPlazoCredito ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchDesde", SqlDbType.DateTime).Value = (object?)request.FchDesde ?? DBNull.Value;
                cmd.Parameters.Add("@dtFchHasta", SqlDbType.DateTime).Value = (object?)request.FchHasta ?? DBNull.Value;
                cmd.Parameters.Add("@vchComentario", SqlDbType.VarChar).Value = (object?)request.Comentario ?? DBNull.Value;
                cmd.Parameters.Add("@intIdEstado", SqlDbType.Int).Value = request.IdEstado;
                cmd.Parameters.Add("@bitImprimeLogoSafety", SqlDbType.Bit).Value = request.ImprimeLogoSafety;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoCreado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoObtener request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Pedido_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = (object?)request.idPedido ?? DBNull.Value;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = (object?)request.idCliente ?? DBNull.Value;
                cmd.Parameters.Add("@intIdTarifario", SqlDbType.Int).Value = (object?)request.idTarifario ?? DBNull.Value;
                cmd.Parameters.Add("@vchNombreInvestigado", SqlDbType.VarChar, 255).Value = (object?)request.nombreInvestigado ?? DBNull.Value;

                var tableIdEstado = ConstruirTablaListaGeneralNum(request.idEstado);
                var tvpIdEstado = cmd.Parameters.AddWithValue("@lstIdEstado", tableIdEstado);
                tvpIdEstado.SqlDbType = SqlDbType.Structured;
                tvpIdEstado.TypeName = "LISTA_GENERAL_NUM";

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoConsulta>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedido request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Pedido_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = (object?)request.idCliente ?? DBNull.Value;
                cmd.Parameters.Add("@vchIdEstado", SqlDbType.VarChar).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 0;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<PedidoListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new PedidoListaResult()
                        : new PedidoListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new PedidoListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoListaResult()
                };
            }
        }

        public async Task<Respuesta> ListarAsignacionAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoAsignacion request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("PedidoAsignacion_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchBusqueda", SqlDbType.VarChar, 255).Value = (object?)request.busqueda ?? DBNull.Value;
                cmd.Parameters.Add("@intIdCliente", SqlDbType.Int).Value = (object?)request.idCliente ?? DBNull.Value;
                cmd.Parameters.Add("@vchIdEstado", SqlDbType.VarChar).Value = (object?)request.idEstado ?? DBNull.Value;
                cmd.Parameters.Add("@numPag", SqlDbType.Int).Value = (object?)request.numPag ?? DBNull.Value;

                await cn.OpenAsync();

                var respuesta = new Respuesta();
                using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 0;
                    respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;

                    var json = dr["Result"]?.ToString();
                    respuesta.Result = !string.IsNullOrWhiteSpace(json)
                        ? JsonSerializer.Deserialize<PedidoAsignacionListaResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new PedidoAsignacionListaResult()
                        : new PedidoAsignacionListaResult();
                }
                else
                {
                    respuesta.IdTipoMensaje = 1;
                    respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
                    respuesta.Result = new PedidoAsignacionListaResult();
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new PedidoAsignacionListaResult()
                };
            }
        }

        public async Task<Respuesta> CancelarAsync(UsuarioGeneral usuarioLogueado, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Pedido_Cancelar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idPedido)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("Pedido_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdPedido", SqlDbType.Int).Value = idPedido;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<PedidoEliminado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<PedidoEliminado>()
                };
            }
        }

    }
}