using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DirectorioEjecutivoController : BaseController
    {
        private readonly DirectorioEjecutivoHandler _directorioEjecutivoHandler;

        public DirectorioEjecutivoController(DirectorioEjecutivoHandler directorioEjecutivoHandler)
        {
            _directorioEjecutivoHandler = directorioEjecutivoHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] List<DirectorioEjecutivoCrear> request)
        {
            var respuesta = await _directorioEjecutivoHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] DirectorioEjecutivoEditar request)
        {
            var respuesta = await _directorioEjecutivoHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] DirectorioEjecutivoObtenerRequest request)
        {
            var respuesta = await _directorioEjecutivoHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroDirectorioEjecutivo filtro)
        {
            var respuesta = await _directorioEjecutivoHandler.ListarAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] DirectorioEjecutivoIdRequest request)
        {
            var respuesta = await _directorioEjecutivoHandler.EliminarAsync(UsuarioLogueado, request.IdDirectorioEjecutivo);
            return Ok(respuesta);
        }
    }
}
