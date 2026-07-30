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
        private readonly IHostEnvironment _entorno;

        public PedidoFacturaController(PedidoFacturaHandler pedidoFacturaHandler, IHostEnvironment entorno)
        {
            _pedidoFacturaHandler = pedidoFacturaHandler;
            _entorno = entorno;
        }

        [HttpPost("enviarSunat")]
        public async Task<IActionResult> EnviarSunat([FromQuery] int idPedido)
        {
            // Mismo criterio que ms-facturación (Beta en dev/staging, Producción en cualquier otro entorno).
            var ambienteCodigo = _entorno.IsDevelopment() || _entorno.IsStaging() ? "Beta" : "Produccion";
            var respuesta = await _pedidoFacturaHandler.EnviarPedidoASunatAsync(UsuarioLogueado, idPedido, ambienteCodigo);
            return Ok(respuesta);
        }
    }
}
