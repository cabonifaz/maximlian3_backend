using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InformeController(InformeHandler informeHandler, InformeTranslationHandler translationHandler) : BaseController
    {
        private readonly InformeHandler _informeHandler = informeHandler;
        private readonly InformeTranslationHandler _translationHandler = translationHandler;

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] InformeCrear request)
        {
            var respuesta = await _informeHandler.InsertarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] InformeEditar request)
        {
            var respuesta = await _informeHandler.ActualizarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] FiltroInformeObtener request)
        {
            var respuesta = await _informeHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("previsualizarDocumento")]
        public async Task<IActionResult> PrevisualizarDocumento([FromQuery] FiltroGenerarDocumento request)
        {
            var respuesta = await _informeHandler.PrevisualizarDocumentoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }


        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroInforme request)
        {
            var respuesta = await _informeHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listarIdPorCompania")]
        public async Task<IActionResult> ListarIdPorCompania([FromQuery] FiltroInformeIdPorCompania request)
        {
            var respuesta = await _informeHandler.ListarIdPorCompaniaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }


        [HttpPost("calcularBalanceSeguro")]
        public async Task<IActionResult> CalcularBalanceSeguro([FromBody] InformeBalanceSeguroCalcularRequest request)
        {
            var respuesta = await _informeHandler.CalcularBalanceSeguroAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("calcularBalanceBanco")]
        public async Task<IActionResult> CalcularBalanceBanco([FromBody] InformeBalanceBancoCalcularRequest request)
        {
            var respuesta = await _informeHandler.CalcularBalanceBancoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("calcularBalanceTurquia")]
        public async Task<IActionResult> CalcularBalanceTurquia([FromBody] InformeBalanceTurquiaCalcularRequest request)
        {
            var respuesta = await _informeHandler.CalcularBalanceTurquiaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("calcularBalanceDesagregado")]
        public async Task<IActionResult> CalcularBalanceDesagregado([FromBody] InformeBalanceDesagregadoCalcularRequest request)
        {
            var respuesta = await _informeHandler.CalcularBalanceDesagregadoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("calcularBalanceTotalizado")]
        public async Task<IActionResult> CalcularBalanceTotalizado([FromBody] InformeBalanceTotalizadoCalcularRequest request)
        {
            var respuesta = await _informeHandler.CalcularBalanceTotalizadoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtenerDocumento")]
        public async Task<IActionResult> ObtenerDocumento([FromQuery] FiltroGenerarDocumento request)
        {
            var respuesta = await _informeHandler.ObtenerDocumentoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("actualizarEstado")]
        public async Task<IActionResult> ActualizarEstado([FromBody] InformeActualizarEstadoRequest request)
        {
            var respuesta = await _informeHandler.ActualizarEstadoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpDelete("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] InformeIdRequest request)
        {
            var respuesta = await _informeHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("obtenerUrlPrefirmada")]
        public async Task<IActionResult> ObtenerUrlPrefirmada([FromBody] InformeUrlPrefirmadaRequest request)
        {
            var respuesta = await _informeHandler.ObtenerUrlPrefirmadaAsync(request);
            return Ok(respuesta);
        }

        [HttpPost("autocompletar")]
        [RequestTimeout(300000)]
        public async Task<IActionResult> Autocompletar([FromBody] InformeAutocompletar request)
        {
            var respuesta = await _informeHandler.AutocompletarAsync(request);
            return Ok(respuesta);
        }

        [HttpPost("extraerDocumento")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ExtraerDocumento(
            IFormFile archivo,
            [FromForm] string secciones,
            [FromForm] string? prompt)
        {
            var respuesta = await _informeHandler.ExtraerDocumentoAsync(archivo, secciones, prompt);
            return Ok(respuesta);
        }

        [HttpPost("traducir")]
        public async Task<IActionResult> Traducir([FromBody] InformeTranslationRequest request)
        {
            var respuesta = await _translationHandler.TranslateAsync(request);
            return Ok(respuesta);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerEvolucion([FromQuery] EvolucionInformesRequest request)
        {
            var respuesta = await _informeHandler.ObtenerEvolucionAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

    }
}
