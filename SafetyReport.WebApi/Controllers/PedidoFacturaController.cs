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

        [HttpGet("factura/{idPedido:int}")]
        public async Task<IActionResult> ObtenerFacturaPorPedido(int idPedido)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerFacturaPorPedidoAsync(UsuarioLogueado, idPedido);
            return Ok(respuesta);
        }

        [HttpPut("guardarCambios/{idDocumentoElectronico:int}")]
        public async Task<IActionResult> GuardarCambios(int idDocumentoElectronico, [FromBody] GuardarCambiosFacturaRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.GuardarCambiosFacturaAsync(UsuarioLogueado, idDocumentoElectronico, request);
            return Ok(respuesta);
        }

        [HttpPost("emitir/{idDocumentoElectronico:int}")]
        public async Task<IActionResult> Emitir(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.EmitirFacturaAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpPut("estado/{idPedido:int}")]
        public async Task<IActionResult> ActualizarEstadoFacturacion(int idPedido, [FromQuery] int idEstadoFacturacion)
        {
            var respuesta = await _pedidoFacturaHandler.ActualizarEstadoFacturacionAsync(UsuarioLogueado, idPedido, idEstadoFacturacion);
            return Ok(respuesta);
        }
    }
}
