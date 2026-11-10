using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Application.CasosDeUso;
using SafetyReport.Application.Puertos.FlujoInforme;

namespace SafetyReport.WebApi.Controllers
{
    // SRC-F1-065: complementos — informes sin pedido del cliente, creados por el traductor desde la
    // última versión aprobada de la compañía. No se facturan.
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InformeComplementoController(FlujoInformeHandler flujoInformeHandler) : BaseController
    {
        private readonly FlujoInformeHandler _flujoInformeHandler = flujoInformeHandler;

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroComplementos request)
        {
            var respuesta = await _flujoInformeHandler.ListarComplementosAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearComplementoRequest request)
        {
            var respuesta = await _flujoInformeHandler.CrearComplementoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("enviarAprobacion")]
        public async Task<IActionResult> EnviarAprobacion([FromBody] EnviarAprobacionComplementoRequest request)
        {
            var respuesta = await _flujoInformeHandler.EnviarAprobacionComplementoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listarClientesDestino")]
        public async Task<IActionResult> ListarClientesDestino()
        {
            var respuesta = await _flujoInformeHandler.ListarClientesDestinoComplementoAsync(UsuarioLogueado);
            return Ok(respuesta);
        }
    }
}
