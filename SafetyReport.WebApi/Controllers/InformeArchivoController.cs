using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/informeArchivo")]
    [ApiController]
    public class InformeArchivoController(InformeArchivoHandler informeArchivoHandler) : BaseController
    {
        private readonly InformeArchivoHandler _informeArchivoHandler = informeArchivoHandler;

        [HttpPost("generarUrls")]
        public async Task<IActionResult> GenerarUrls([FromBody] InformeArchivoUrlRequest request)
        {
            var respuesta = await _informeArchivoHandler.GenerarUrlsArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("obtener")]
        public async Task<IActionResult> Obtener([FromBody] InformeArchivoIdRequest request)
        {
            var respuesta = await _informeArchivoHandler.ObtenerArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpDelete("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] InformeArchivoIdRequest request)
        {
            var respuesta = await _informeArchivoHandler.EliminarArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] InformeArchivoActualizarRequest request)
        {
            var respuesta = await _informeArchivoHandler.ActualizarArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("insertarLote")]
        public async Task<IActionResult> InsertarLote([FromBody] InformeArchivoInsertarRequest request)
        {
            var respuesta = await _informeArchivoHandler.InsertarArchivoLoteAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
