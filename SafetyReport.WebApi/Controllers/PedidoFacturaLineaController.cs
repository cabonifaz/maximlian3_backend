using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoFacturaLineaController : BaseController
    {
        private readonly PedidoFacturaLineaHandler _pedidoFacturaLineaHandler;

        public PedidoFacturaLineaController(PedidoFacturaLineaHandler pedidoFacturaLineaHandler)
        {
            _pedidoFacturaLineaHandler = pedidoFacturaLineaHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarLineasFacturacionRequest request)
        {
            var respuesta = await _pedidoFacturaLineaHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearLineaFacturacionRequest request)
        {
            var respuesta = await _pedidoFacturaLineaHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPut("{idPedidoFacturaLinea:int}")]
        public async Task<IActionResult> ActualizarDatos(int idPedidoFacturaLinea, [FromBody] ActualizarLineaFacturacionRequest request)
        {
            var respuesta = await _pedidoFacturaLineaHandler.ActualizarDatosAsync(UsuarioLogueado, idPedidoFacturaLinea, request);
            return Ok(respuesta);
        }

        [HttpPut("{idPedidoFacturaLinea:int}/pedidos")]
        public async Task<IActionResult> ActualizarPedidos(int idPedidoFacturaLinea, [FromBody] ActualizarPedidosLineaFacturacionRequest request)
        {
            var respuesta = await _pedidoFacturaLineaHandler.ActualizarPedidosAsync(UsuarioLogueado, idPedidoFacturaLinea, request);
            return Ok(respuesta);
        }

        [HttpDelete("{idPedidoFacturaLinea:int}")]
        public async Task<IActionResult> Desvincular(int idPedidoFacturaLinea)
        {
            var respuesta = await _pedidoFacturaLineaHandler.DesvincularAsync(UsuarioLogueado, idPedidoFacturaLinea);
            return Ok(respuesta);
        }
    }
}
