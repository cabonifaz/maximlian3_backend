using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioHandler _usuarioHandler;

        public UsuarioController(UsuarioHandler usuarioHandler)
        {
            _usuarioHandler = usuarioHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearUsuario request)
        {
            var respuesta = await _usuarioHandler.CrearUsuarioAsync(request);
            return Ok(respuesta);
        }
    }
}