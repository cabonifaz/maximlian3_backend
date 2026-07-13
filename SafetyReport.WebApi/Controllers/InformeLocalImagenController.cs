using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/informeLocalImagen")]
    [ApiController]
    public class InformeLocalImagenController(InformeLocalImagenHandler informeLocalImagenHandler) : BaseController
    {
        private readonly InformeLocalImagenHandler _informeLocalImagenHandler = informeLocalImagenHandler;

        [HttpPost("obtenerUrls")]
        public async Task<IActionResult> ObtenerUrls([FromBody] InformeLocalImagenEstadoCargaRequest request)
        {
            var respuesta = await _informeLocalImagenHandler.ObtenerUrlsImagenesAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("actualizarEstadoCarga")]
        public async Task<IActionResult> ActualizarEstadoCarga([FromBody] InformeLocalImagenEstadoCargaRequest request)
        {
            var respuesta = await _informeLocalImagenHandler.ActualizarEstadoCargaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
