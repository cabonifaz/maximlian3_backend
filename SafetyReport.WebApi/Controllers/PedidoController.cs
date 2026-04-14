using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : BaseController
    {
        private readonly PedidoHandler _pedidoHandler;

        public PedidoController(PedidoHandler pedidoHandler)
        {
            _pedidoHandler = pedidoHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] Pedido request)
        {
            var respuesta = await _pedidoHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] EditarPedido request)
        {
            var respuesta = await _pedidoHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] FiltroPedidoObtener request)
        {
            var respuesta = await _pedidoHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroPedido request)
        {
            var respuesta = await _pedidoHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listarAsignacion")]
        public async Task<IActionResult> ListarAsignacion([FromQuery] FiltroPedidoAsignacion request)
        {
            var respuesta = await _pedidoHandler.ListarAsignacionAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] PedidoIdRequest request)
        {
            var respuesta = await _pedidoHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}