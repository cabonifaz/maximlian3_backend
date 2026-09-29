using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.InformeAprobacion;

namespace SafetyReport.Application.CasosDeUso
{
    public class InformeAprobacionHandler
    {
        private readonly IInformeAprobacionRepository _informeAprobacionRepository;
        private readonly ILogger<InformeAprobacionHandler> _logger;

        public InformeAprobacionHandler(
            IInformeAprobacionRepository informeAprobacionRepository,
            ILogger<InformeAprobacionHandler> logger)
        {
            _informeAprobacionRepository = informeAprobacionRepository;
            _logger = logger;
        }

        public async Task<Respuesta> ListarPendientesAsync(UsuarioGeneral usuarioLogueado, FiltroInformeAprobacionPendientes request)
        {
            try
            {
                return await _informeAprobacionRepository.ListarPendientesAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new InformeAprobacionPendientesListaResult() };
            }
        }

        public async Task<Respuesta> AprobarAsync(UsuarioGeneral usuarioLogueado, InformeAprobacionAprobarRequest request)
        {
            try
            {
                return await _informeAprobacionRepository.AprobarAsync(usuarioLogueado, request.IdInforme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}
