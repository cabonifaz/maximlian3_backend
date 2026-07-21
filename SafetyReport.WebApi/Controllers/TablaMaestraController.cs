using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TablaMaestraController : BaseController
    {
        private readonly TablaMaestraHandler _tablaMaestraHandler;

        public TablaMaestraController(TablaMaestraHandler tablaMaestraHandler)
        {
            _tablaMaestraHandler = tablaMaestraHandler;
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroTablaMaestraRequest request)
        {
            var respuesta = await _tablaMaestraHandler.ListarAsync(UsuarioLogueado, request?.idsMaestro, request?.busqueda, request?.numPag);
            return Ok(respuesta);
        }

        [HttpGet("listar-inventario")]
        public async Task<IActionResult> ListarInventario([FromQuery] int? idMaestro)
        {
            var respuesta = await _tablaMaestraHandler.ListarInventarioAsync(UsuarioLogueado, idMaestro);
            return Ok(respuesta);
        }

        [HttpGet("lista-corta")]
        public async Task<IActionResult> ListaCorta([FromQuery] int idMaestro)
        {
            var respuesta = await _tablaMaestraHandler.ListaCortaAsync(UsuarioLogueado, idMaestro);
            return Ok(respuesta);
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] TablaMaestraRequest request)
        {
            var respuesta = await _tablaMaestraHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] EditarTablaMaestraRequest request)
        {
            var respuesta = await _tablaMaestraHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] ObtenerTablaMaestraRequest request)
        {
            var respuesta = await _tablaMaestraHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarTablaMaestraRequest request)
        {
            var respuesta = await _tablaMaestraHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
