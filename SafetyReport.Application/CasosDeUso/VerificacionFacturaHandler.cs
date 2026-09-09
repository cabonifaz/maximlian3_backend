using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Pedido;
using SafetyReport.Application.Puertos.PedidoFactura;
using SafetyReport.Models;

namespace SafetyReport.Application.CasosDeUso
{
    // Verificación pública de facturas: cualquiera con el link (token) puede consultar el documento,
    // descargar su Xml/Pdf, o listar sus pedidos, sin login. Separado de PedidoFacturaHandler a
    // propósito — ese flujo asume un UsuarioGeneral autenticado, este no recibe ni necesita uno. El
    // controller que lo expone no lleva [Authorize].
    public class VerificacionFacturaHandler
    {
        private readonly IFacturacionElectronicaGateway _facturacionGateway;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ILogger<VerificacionFacturaHandler> _logger;

        public VerificacionFacturaHandler(
            IFacturacionElectronicaGateway facturacionGateway,
            IPedidoRepository pedidoRepository,
            ILogger<VerificacionFacturaHandler> logger)
        {
            _facturacionGateway = facturacionGateway;
            _pedidoRepository = pedidoRepository;
            _logger = logger;
        }

        public async Task<Respuesta> ObtenerPorTokenAsync(string token)
        {
            try
            {
                var resultado = await _facturacionGateway.ObtenerDocumentoPorTokenAsync(token, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo obtener el documento." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // Pedidos individuales detrás del documento (montos ya concatenados con Moneda, ver
        // SP_Pedido_ListarPorDocumentoElectronicoPublico) — resuelve el token contra ms-facturación
        // (Id/IdInquilino, este equivale a IdEmpresa en maximlian3) y con eso consulta maximlian3.
        public async Task<Respuesta> ListarPedidosPorTokenAsync(string token)
        {
            try
            {
                var identificado = await _facturacionGateway.ObtenerIdDocumentoPorTokenAsync(token, CancellationToken.None);

                if (identificado is null || identificado.IdTipoMensaje != 2 || identificado.Datos is null)
                {
                    return new Respuesta { IdTipoMensaje = identificado?.IdTipoMensaje ?? 3, Mensaje = identificado?.Mensaje ?? "Token de verificación inválido." };
                }

                return await _pedidoRepository.ListarPorDocumentoElectronicoPublicoAsync(
                    identificado.Datos.IdInquilino, identificado.Datos.IdDocumentoElectronico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }

        // tipoArchivo: "Xml" o "Pdf". Solo hace de proxy — la URL presignada la arma ms-facturación.
        public async Task<Respuesta> ObtenerUrlDescargaPorTokenAsync(string token, string tipoArchivo)
        {
            try
            {
                var resultado = await _facturacionGateway.ObtenerUrlDescargaPorTokenAsync(token, tipoArchivo, CancellationToken.None);

                if (resultado is null || resultado.IdTipoMensaje != 2)
                {
                    return new Respuesta { IdTipoMensaje = resultado?.IdTipoMensaje ?? 3, Mensaje = resultado?.Mensaje ?? "No se pudo obtener la URL de descarga." };
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "Consulta exitosa.", Result = resultado.Datos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}
