using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/informeObservacion")]
    [ApiController]
    public class InformeObservacionController(InformeObservacionHandler informeObservacionHandler) : BaseController
    {
        private readonly InformeObservacionHandler _informeObservacionHandler = informeObservacionHandler;

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] InformeObservacionListarRequest request)
        {
            var respuesta = await _informeObservacionHandler.ListarObservacionesAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("insertarLote")]
        public async Task<IActionResult> InsertarLote([FromBody] InformeObservacionInsertarRequest request)
        {
            var respuesta = await _informeObservacionHandler.InsertarObservacionesLoteAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] InformeObservacionEditarRequest request)
        {
            var respuesta = await _informeObservacionHandler.EditarObservacionAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] InformeObservacionIdRequest request)
        {
            var respuesta = await _informeObservacionHandler.EliminarObservacionAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
