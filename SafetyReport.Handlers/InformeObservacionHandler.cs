using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeObservacionHandler
    {
        private readonly InformeObservacionDAO _dao;
        private readonly ILogger<InformeObservacionHandler> _logger;

        public InformeObservacionHandler(InformeObservacionDAO dao, ILogger<InformeObservacionHandler> logger)
        {
            _dao = dao;
            _logger = logger;
        }

        public async Task<Respuesta> ListarObservacionesAsync(UsuarioGeneral usuarioLogueado, InformeObservacionListarRequest request)
        {
            try
            {
                return await _dao.ListarObservacionesAsync(usuarioLogueado, request.IdPedido);
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
                return await _dao.InsertarObservacionesLoteAsync(usuarioLogueado, request.IdInforme, request.IdPedido, request.Observaciones);
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
                return await _dao.EditarObservacionAsync(usuarioLogueado, request);
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
                return await _dao.EliminarObservacionAsync(usuarioLogueado, request.IdInformeObservacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }
    }
}
