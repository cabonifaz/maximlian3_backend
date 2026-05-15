using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AsignacionController : BaseController
    {
        private readonly AsignacionHandler _asignacionHandler;

        public AsignacionController(AsignacionHandler asignacionHandler)
        {
            _asignacionHandler = asignacionHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] AsignacionCrear request)
        {
            var respuesta = await _asignacionHandler.InsertarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] AsignacionActualizar request)
        {
            var respuesta = await _asignacionHandler.ActualizarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroAsignacion request)
        {
            var respuesta = await _asignacionHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] int idAsignacion)
        {
            var respuesta = await _asignacionHandler.ObtenerAsync(UsuarioLogueado, idAsignacion);
            return Ok(respuesta);
        }

        [HttpGet("bandeja")]
        public async Task<IActionResult> Bandeja([FromQuery] FiltroAsignacionBandeja filtro)
        {
            var respuesta = await _asignacionHandler.BandejaAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarAsignacion request)
        {
            var respuesta = await _asignacionHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
