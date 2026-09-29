using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeAprobacionHandler
    {
        private readonly InformeAprobacionDAO _dao;
        private readonly ILogger<InformeAprobacionHandler> _logger;

        public InformeAprobacionHandler(InformeAprobacionDAO dao, ILogger<InformeAprobacionHandler> logger)
        {
            _dao = dao;
            _logger = logger;
        }

        public async Task<Respuesta> ListarPendientesAsync(UsuarioGeneral usuarioLogueado, FiltroInformeAprobacionPendientes request)
        {
            try
            {
                return await _dao.ListarPendientesAsync(usuarioLogueado, request);
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
                return await _dao.AprobarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message };
            }
        }
    }
}
