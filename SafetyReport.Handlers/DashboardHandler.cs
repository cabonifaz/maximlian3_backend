using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class DashboardHandler
    {
        private readonly DashboardDAO _dao;
        private readonly ILogger<DashboardHandler> _logger;

        public DashboardHandler(DashboardDAO dao, ILogger<DashboardHandler> logger)
        {
            _dao = dao;
            _logger = logger;
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
                    Result = new ResumenClientesDashboard()
                };
            }
        }
    }
}
