using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class ClienteHandler
    {
        private readonly ClienteDAO _dao;
        private readonly ILogger<ClienteHandler> _logger;

        public ClienteHandler(ClienteDAO dao, ILogger<ClienteHandler> logger)
        {
            _dao = dao;
            _logger = logger;
        }

        public async Task<Respuesta> CrearClienteAsync(UsuarioGeneral usuarioLogueado, Cliente request)
        {
            try
            {
                return await _dao.CrearClienteAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteCreado>()
                };
            }
        }

        public async Task<Respuesta> EditarClienteAsync(UsuarioGeneral usuarioLogueado, EditarCliente request)
        {
            try
            {
                return await _dao.EditarClienteAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente)
        {
            try
            {
                return await _dao.ObtenerClienteAsync(usuarioLogueado, idCliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarClientesAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? idPais, int? idEstado)
        {
            try
            {
                return await _dao.ListarClientesAsync(usuarioLogueado, busqueda, numPag, idPais, idEstado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarClienteAsync(UsuarioGeneral usuarioLogueado, ClienteIdRequest request)
        {
            try
            {
                return await _dao.EliminarClienteAsync(usuarioLogueado, request.idCliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteEliminado>()
                };
            }
        }

        public async Task<Respuesta> ListarClienteShortAsync(UsuarioGeneral usuarioLogueado, string? correoBusqueda)
        {
            try
            {
                return await _dao.ListarClienteShortAsync(usuarioLogueado, correoBusqueda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteListaCorta>()
                };
            }
        }

        public async Task<Respuesta> ListarClientesFacturacionAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? emitirPrefactura, int? idIdiomaFacturacion, int? estadoFacturacion)
        {
            try
            {
                return await _dao.ListarClientesFacturacionAsync(usuarioLogueado, busqueda, numPag, emitirPrefactura, idIdiomaFacturacion, estadoFacturacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteListaFacturacionResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerResumenClientesAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                return await _dao.ObtenerResumenClientesAsync(usuarioLogueado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteResumen()
                };
            }
        }
    }
}