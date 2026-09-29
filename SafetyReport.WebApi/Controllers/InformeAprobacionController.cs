using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Application.CasosDeUso;
using SafetyReport.Application.Puertos.InformeAprobacion;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/informeAprobacion")]
    [ApiController]
    public class InformeAprobacionController(InformeAprobacionHandler informeAprobacionHandler) : BaseController
    {
        private readonly InformeAprobacionHandler _informeAprobacionHandler = informeAprobacionHandler;

        [HttpGet("listarPendientes")]
        public async Task<IActionResult> ListarPendientes([FromQuery] FiltroInformeAprobacionPendientes request)
        {
            var respuesta = await _informeAprobacionHandler.ListarPendientesAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("aprobar")]
        public async Task<IActionResult> Aprobar([FromBody] InformeAprobacionAprobarRequest request)
        {
            var respuesta = await _informeAprobacionHandler.AprobarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
