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

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroInforme request)
        {
            var respuesta = await _informeHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("actualizarEstadoCargaImagenes")]
        public async Task<IActionResult> ActualizarEstadoCargaImagenes([FromBody] InformeLocalImagenEstadoCargaRequest request)
        {
            var respuesta = await _informeHandler.ActualizarEstadoCargaAsync(UsuarioLogueado, request);
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
