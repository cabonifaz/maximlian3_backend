using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BancoController : BaseController
    {
        private readonly BancoHandler _bancoHandler;

        public BancoController(BancoHandler bancoHandler)
        {
            _bancoHandler = bancoHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] List<BancoCrear> request)
        {
            var respuesta = await _bancoHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] BancoEditar request)
        {
            var respuesta = await _bancoHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] BancoObtenerRequest request)
        {
            var respuesta = await _bancoHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroBanco filtro)
        {
            var respuesta = await _bancoHandler.ListarAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpPost("obtenerCoincidencias")]
        public async Task<IActionResult> ObtenerCoincidencias([FromBody] List<BancoMatchItem> request)
        {
            var respuesta = await _bancoHandler.ListarMatchAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] BancoIdRequest request)
        {
            var respuesta = await _bancoHandler.EliminarAsync(UsuarioLogueado, request.IdBanco);
            return Ok(respuesta);
        }
    }
}
