using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MasterTableController : BaseController
    {
        private readonly MasterTableHandler _masterTableHandler;

        public MasterTableController(MasterTableHandler masterTableHandler)
        {
            _masterTableHandler = masterTableHandler;
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroMasterTableRequest request)
        {
            var respuesta = await _masterTableHandler.ListarAsync(UsuarioLogueado, request?.IdMaster);
            return Ok(respuesta);
        }

        [HttpGet("listar-inventario")]
        public async Task<IActionResult> ListarInventario([FromQuery] object? request)
        {
            var respuesta = await _masterTableHandler.ListarInventarioAsync(UsuarioLogueado);
            return Ok(respuesta);
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] MasterTableRequest request)
        {
            var respuesta = await _masterTableHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] EditarMasterTableRequest request)
        {
            var respuesta = await _masterTableHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarMasterTableRequest request)
        {
            var respuesta = await _masterTableHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}