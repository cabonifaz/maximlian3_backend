using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.InformeObservacion;
using SafetyReport.Models;

namespace SafetyReport.Handlers.CasosDeUso
{
    public class InformeObservacionHandler
    {
        private readonly IInformeObservacionRepository _informeObservacionRepository;
        private readonly ILogger<InformeObservacionHandler> _logger;

        public InformeObservacionHandler(
            IInformeObservacionRepository informeObservacionRepository,
            ILogger<InformeObservacionHandler> logger)
        {
            _informeObservacionRepository = informeObservacionRepository;
            _logger = logger;
        }

        public async Task<Respuesta> ListarObservacionesAsync(UsuarioGeneral usuarioLogueado, InformeObservacionListarRequest request)
        {
            try
            {
                return await _informeObservacionRepository.ListarObservacionesAsync(usuarioLogueado, request.IdPedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeObservacionConsulta>() };
            }
        }

        public async Task<Respuesta> InsertarObservacionesLoteAsync(UsuarioGeneral usuarioLogueado, InformeObservacionInsertarRequest request)
        {
            try
            {
                return await _informeObservacionRepository.InsertarObservacionesLoteAsync(
                    usuarioLogueado, request.IdInforme, request.IdPedido, request.Observaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EditarObservacionAsync(UsuarioGeneral usuarioLogueado, InformeObservacionEditarRequest request)
        {
            try
            {
                return await _informeObservacionRepository.EditarObservacionAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EliminarObservacionAsync(UsuarioGeneral usuarioLogueado, InformeObservacionIdRequest request)
        {
            try
            {
                return await _informeObservacionRepository.EliminarObservacionAsync(usuarioLogueado, request.IdInformeObservacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }
    }
}
