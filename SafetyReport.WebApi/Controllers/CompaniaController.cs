using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniaController : BaseController
    {
        private readonly CompaniaHandler _companiaHandler;

        public CompaniaController(CompaniaHandler companiaHandler)
        {
            _companiaHandler = companiaHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] List<CompaniaCrear> request)
        {
            var respuesta = await _companiaHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] CompaniaEditar request)
        {
            var respuesta = await _companiaHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] CompaniaObtenerRequest request)
        {
            var respuesta = await _companiaHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroCompania filtro)
        {
            var respuesta = await _companiaHandler.ListarAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] CompaniaIdRequest request)
        {
            var respuesta = await _companiaHandler.EliminarAsync(UsuarioLogueado, request.IdCompania);
            return Ok(respuesta);
        }
    }
}
