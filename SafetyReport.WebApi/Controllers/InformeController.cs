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
    public class InformeController(InformeHandler informeHandler) : BaseController
    {
        private readonly InformeHandler _informeHandler = informeHandler;

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

        [HttpGet("generarDocumento")]
        public async Task<IActionResult> GenerarDocumento([FromQuery] FiltroGenerarDocumento request)
        {
            var respuesta = await _informeHandler.GenerarDocumentoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("generarDocumentoDocx")]
        public async Task<IActionResult> GenerarDocumentoDocx([FromQuery] FiltroGenerarDocumento request)
        {
            var respuesta = await _informeHandler.GenerarDocumentoDocxAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("generarDocumentoPdf")]
        public async Task<IActionResult> GenerarDocumentoPdf([FromQuery] FiltroGenerarDocumento request)
        {
            var respuesta = await _informeHandler.GenerarDocumentoPdfAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroInforme request)
        {
            var respuesta = await _informeHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("obtenerUrlsImagenes")]
        public async Task<IActionResult> ObtenerUrlsImagenes([FromBody] InformeLocalImagenEstadoCargaRequest request)
        {
            var respuesta = await _informeHandler.ObtenerUrlsImagenesAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("actualizarEstadoCargaImagenes")]
        public async Task<IActionResult> ActualizarEstadoCargaImagenes([FromBody] InformeLocalImagenEstadoCargaRequest request)
        {
            var respuesta = await _informeHandler.ActualizarEstadoCargaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("generarUrlsArchivo")]
        public async Task<IActionResult> GenerarUrlsArchivo([FromBody] InformeArchivoUrlRequest request)
        {
            var respuesta = await _informeHandler.GenerarUrlsArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("obtenerArchivo")]
        public async Task<IActionResult> ObtenerArchivo([FromBody] InformeArchivoIdRequest request)
        {
            var respuesta = await _informeHandler.ObtenerArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminarArchivo")]
        public async Task<IActionResult> EliminarArchivo([FromBody] InformeArchivoIdRequest request)
        {
            var respuesta = await _informeHandler.EliminarArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("actualizarArchivo")]
        public async Task<IActionResult> ActualizarArchivo([FromBody] InformeArchivoActualizarRequest request)
        {
            var respuesta = await _informeHandler.ActualizarArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("insertarArchivoLote")]
        public async Task<IActionResult> InsertarArchivoLote([FromBody] InformeArchivoInsertarRequest request)
        {
            var respuesta = await _informeHandler.InsertarArchivoLoteAsync(UsuarioLogueado, request);
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

        [HttpPost("eliminar")]
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

    }
}
