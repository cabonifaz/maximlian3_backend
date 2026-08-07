using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;

namespace SafetyReport.WebApi.Controllers
{
    // Público a propósito — sin [Authorize]: el link de verificación (token del PDF) debe funcionar para
    // cualquiera que lo abra, no solo usuarios logueados. No hereda de BaseController (asume un
    // UsuarioLogueado autenticado que acá no existe).
    [Route("api/[controller]")]
    [ApiController]
    public class VerificacionFacturaController : ControllerBase
    {
        private readonly VerificacionFacturaHandler _verificacionFacturaHandler;

        public VerificacionFacturaController(VerificacionFacturaHandler verificacionFacturaHandler)
        {
            _verificacionFacturaHandler = verificacionFacturaHandler;
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> ObtenerPorToken(string token)
        {
            var respuesta = await _verificacionFacturaHandler.ObtenerPorTokenAsync(token);
            return Ok(respuesta);
        }

        [HttpGet("{token}/urlDescarga")]
        public async Task<IActionResult> ObtenerUrlDescargaPorToken(string token, [FromQuery] string tipoArchivo)
        {
            var respuesta = await _verificacionFacturaHandler.ObtenerUrlDescargaPorTokenAsync(token, tipoArchivo);
            return Ok(respuesta);
        }
    }
}
