using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Cliente;
using SafetyReport.Models;

namespace SafetyReport.Handlers.CasosDeUso
{
    public class ClienteHandler
    {
        private readonly IClienteRepository _repository;
        private readonly ILogger<ClienteHandler> _logger;

        public ClienteHandler(IClienteRepository repository, ILogger<ClienteHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Respuesta> CrearClienteAsync(UsuarioGeneral usuarioLogueado, Cliente request)
        {
            try
            {
                return await _repository.CrearClienteAsync(usuarioLogueado, request);
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
                return await _repository.EditarClienteAsync(usuarioLogueado, request);
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
                return await _repository.ObtenerClienteAsync(usuarioLogueado, idCliente);
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

        // Mismo resultado que ObtenerClienteAsync, pero resuelto por IdDocumentoElectronico (la factura) en
        // vez de un IdCliente directo — ver SP_Cliente_ObtenerPorDocumentoElectronico.
        public async Task<Respuesta> ObtenerClientePorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                return await _repository.ObtenerClientePorDocumentoElectronicoAsync(usuarioLogueado, idDocumentoElectronico);
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

        // Payload reducido de ObtenerClientePorDocumentoElectronicoAsync + las líneas vivas del documento —
        // ver SP_Cliente_ObtenerConLineasPorDocumentoElectronico.
        public async Task<Respuesta> ObtenerClienteConLineasPorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico)
        {
            try
            {
                return await _repository.ObtenerConLineasPorDocumentoElectronicoAsync(usuarioLogueado, idDocumentoElectronico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClienteConLineasConsulta()
                };
            }
        }

        public async Task<Respuesta> ListarClientesAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? idPais, int? idEstado)
        {
            try
            {
                return await _repository.ListarClientesAsync(usuarioLogueado, busqueda, numPag, idPais, idEstado);
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
                return await _repository.EliminarClienteAsync(usuarioLogueado, request.idCliente);
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

        public async Task<Respuesta> ActivarDesactivarClienteAsync(UsuarioGeneral usuarioLogueado, ClienteEstadoRequest request)
        {
            try
            {
                return await _repository.ActivarDesactivarClienteAsync(usuarioLogueado, request.idCliente, request.idEstado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<ClienteEstadoActualizado>()
                };
            }
        }

        public async Task<Respuesta> ListarClienteShortAsync(UsuarioGeneral usuarioLogueado, string? correoBusqueda)
        {
            try
            {
                return await _repository.ListarClienteShortAsync(usuarioLogueado, correoBusqueda);
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

        public async Task<Respuesta> ListarClientesFacturacionAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? emitirPrefactura, int? idIdiomaFacturacion)
        {
            try
            {
                return await _repository.ListarClientesFacturacionAsync(usuarioLogueado, busqueda, numPag, emitirPrefactura, idIdiomaFacturacion);
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

        public async Task<Respuesta> ListarPedidosFacturacionClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente, string? busqueda, int? numPag)
        {
            try
            {
                return await _repository.ListarPedidosFacturacionClienteAsync(usuarioLogueado, idCliente, busqueda, numPag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new ClientePedidosFacturacionResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerResumenClientesAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                return await _repository.ObtenerResumenClientesAsync(usuarioLogueado);
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
