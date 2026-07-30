using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly DashboardHandler _dashboardHandler;

        public DashboardController(DashboardHandler dashboardHandler)
        {
            _dashboardHandler = dashboardHandler;
        }

        [HttpGet("resumenClientes")]
        public async Task<IActionResult> ResumenClientes()
        {
            var respuesta = await _dashboardHandler.ObtenerResumenClientesAsync(UsuarioLogueado);
            return Ok(respuesta);
        }
    }
}
