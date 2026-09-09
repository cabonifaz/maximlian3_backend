using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.PedidoFactura;
using SafetyReport.Application.Puertos.PedidoFacturaLinea;
using SafetyReport.Models;

namespace SafetyReport.Application.CasosDeUso
{
    // CRUD del sub-recurso PEDIDO_FACTURA_LINEA — ValidarAccesoFacturacionAsync se mantiene en
    // PedidoFacturaDAO porque es el chequeo de acceso compartido por todo el módulo de
    // facturación, no algo propio de líneas — ver PLAN_Lineas_Facturacion.md.
    public class PedidoFacturaLineaHandler
    {
        private readonly IPedidoFacturaLineaRepository _pedidoFacturaLineaRepository;
        private readonly IFacturacionAccessValidator _facturacionAccessValidator;
        private readonly ILogger<PedidoFacturaLineaHandler> _logger;

        public PedidoFacturaLineaHandler(
            IPedidoFacturaLineaRepository pedidoFacturaLineaRepository,
            IFacturacionAccessValidator facturacionAccessValidator,
            ILogger<PedidoFacturaLineaHandler> logger)
        {
            _pedidoFacturaLineaRepository = pedidoFacturaLineaRepository;
            _facturacionAccessValidator = facturacionAccessValidator;
            _logger = logger;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, CrearLineaFacturacionRequest request)
        {
            try
            {
                var acceso = await _facturacionAccessValidator.ValidarAccesoFacturacionAsync(usuarioLogueado, "crear una línea de facturación");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                return await _pedidoFacturaLineaRepository.CrearAsync(
                    usuarioLogueado, request.idCliente, request.idsPedido, request.codigo, request.descripcion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> CrearLoteAsync(UsuarioGeneral usuarioLogueado, CrearLineaFacturacionLoteRequest request)
        {
            try
            {
                var acceso = await _facturacionAccessValidator.ValidarAccesoFacturacionAsync(usuarioLogueado, "crear líneas de facturación en lote");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                return await _pedidoFacturaLineaRepository.CrearLoteAsync(usuarioLogueado, request.idCliente, request.grupos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // A diferencia del resto de las SPs de este módulo, SP_PedidoFactura_DesvincularLinea no valida
        // rol/permiso adentro (también corre desde cascades internos sin usuario logueado real) — acá,
        // en cambio, ValidarAccesoFacturacionAsync es el único chequeo de acceso real para el camino manual.
        public async Task<Respuesta> DesvincularAsync(UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea)
        {
            try
            {
                var acceso = await _facturacionAccessValidator.ValidarAccesoFacturacionAsync(usuarioLogueado, "eliminar una línea de facturación");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                return await _pedidoFacturaLineaRepository.DesvincularAsync(usuarioLogueado, idPedidoFacturaLinea);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ActualizarDatosAsync(
            UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea, ActualizarLineaFacturacionRequest request)
        {
            try
            {
                var acceso = await _facturacionAccessValidator.ValidarAccesoFacturacionAsync(usuarioLogueado, "editar una línea de facturación");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                return await _pedidoFacturaLineaRepository.ActualizarDatosAsync(
                    usuarioLogueado, idPedidoFacturaLinea, request.codigo, request.descripcion,
                    request.valorUnitario, request.descuento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ActualizarPedidosAsync(
            UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea, ActualizarPedidosLineaFacturacionRequest request)
        {
            try
            {
                var acceso = await _facturacionAccessValidator.ValidarAccesoFacturacionAsync(usuarioLogueado, "editar los pedidos de una línea de facturación");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                return await _pedidoFacturaLineaRepository.ActualizarPedidosAsync(
                    usuarioLogueado, idPedidoFacturaLinea, request.idCliente, request.idsPedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, ListarLineasFacturacionRequest request)
        {
            try
            {
                var acceso = await _facturacionAccessValidator.ValidarAccesoFacturacionAsync(usuarioLogueado, "listar las líneas de facturación");
                if (acceso.IdTipoMensaje != 2)
                {
                    return acceso;
                }

                return await _pedidoFacturaLineaRepository.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}
