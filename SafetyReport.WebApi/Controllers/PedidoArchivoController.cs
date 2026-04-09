using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;
using SafetyReport.WebApi.Helpers;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [N8nHeader]
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoArchivoController : BaseController
    {
        private readonly PedidoArchivoHandler _pedidoArchivoHandler;

        public PedidoArchivoController(PedidoArchivoHandler pedidoArchivoHandler)
        {
            _pedidoArchivoHandler = pedidoArchivoHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] PedidoArchivoCrearBatch request)
        {
            var respuesta = await _pedidoArchivoHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] PedidoArchivoEditar request)
        {
            var respuesta = await _pedidoArchivoHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] PedidoArchivoIdRequest request)
        {
            var respuesta = await _pedidoArchivoHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroPedidoArchivo request)
        {
            var respuesta = await _pedidoArchivoHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] PedidoArchivoIdRequest request)
        {
            var respuesta = await _pedidoArchivoHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

    }
}