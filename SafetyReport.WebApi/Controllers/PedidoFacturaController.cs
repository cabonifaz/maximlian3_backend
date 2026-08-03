using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

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
        public async Task<IActionResult> GuardarBorrador([FromBody] GuardarBorradorFacturaRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.GuardarBorradorFacturaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listarPedidos")]
        public async Task<IActionResult> ListarPedidos([FromQuery] ListarPedidosFacturacionRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.ListarPedidosParaFacturacionAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
