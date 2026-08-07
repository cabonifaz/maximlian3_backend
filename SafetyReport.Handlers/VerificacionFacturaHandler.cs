using Microsoft.Extensions.Logging;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    // Verificación pública de facturas: cualquiera con el link (token) puede consultar el documento y
    // descargar su Xml/Pdf, sin login. Separado de PedidoFacturaHandler a propósito — ese flujo asume un
    // UsuarioGeneral autenticado, este no recibe ni necesita uno. El controller que lo expone no lleva
    // [Authorize].
    public class VerificacionFacturaHandler
    {
        private readonly FacturacionElectronicaService _facturacionService;
        private readonly ILogger<VerificacionFacturaHandler> _logger;

        public VerificacionFacturaHandler(FacturacionElectronicaService facturacionService, ILogger<VerificacionFacturaHandler> logger)
        {
            _facturacionService = facturacionService;
            _logger = logger;
        }

        public async Task<Respuesta> ObtenerPorTokenAsync(string token)
        {
            try
            {
                var resultado = await _facturacionService.ObtenerDocumentoPorTokenAsync(token, CancellationToken.None);

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

        // tipoArchivo: "Xml" o "Pdf". Solo hace de proxy — la URL presignada la arma ms-facturación.
        public async Task<Respuesta> ObtenerUrlDescargaPorTokenAsync(string token, string tipoArchivo)
        {
            try
            {
                var resultado = await _facturacionService.ObtenerUrlDescargaPorTokenAsync(token, tipoArchivo, CancellationToken.None);

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
