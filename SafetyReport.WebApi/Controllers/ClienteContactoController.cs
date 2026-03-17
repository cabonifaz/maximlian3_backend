using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteContactoController : BaseController
    {
        private readonly ClienteContactoHandler _clienteContactoHandler;

        public ClienteContactoController(ClienteContactoHandler clienteContactoHandler)
        {
            _clienteContactoHandler = clienteContactoHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] ClienteContactoCrear request)
        {
            var respuesta = await _clienteContactoHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] ClienteContactoFiltro request)
        {
            var respuesta = await _clienteContactoHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("obtener")]
        public async Task<IActionResult> Obtener([FromBody] ClienteContactoIdRequest request)
        {
            var respuesta = await _clienteContactoHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] ClienteContactoEditar request)
        {
            var respuesta = await _clienteContactoHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] ClienteContactoIdRequest request)
        {
            var respuesta = await _clienteContactoHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}