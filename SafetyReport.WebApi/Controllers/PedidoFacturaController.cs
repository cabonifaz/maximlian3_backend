using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoFacturaController : BaseController
    {
        private readonly PedidoFacturaHandler _pedidoFacturaHandler;

        public PedidoFacturaController(PedidoFacturaHandler pedidoFacturaHandler)
        {
            _pedidoFacturaHandler = pedidoFacturaHandler;
        }

        [HttpPost("guardarBorrador")]
        public async Task<IActionResult> GuardarBorrador([FromQuery] int idPedido)
        {
            var respuesta = await _pedidoFacturaHandler.GuardarBorradorFacturaAsync(UsuarioLogueado, idPedido);
            return Ok(respuesta);
        }
    }
}
