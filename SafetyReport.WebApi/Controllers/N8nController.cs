using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;
using SafetyReport.WebApi.Helpers;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [N8nHeader]
    public class N8nController : BaseController
    {
        private readonly N8nHandler _n8nHandler;

        public N8nController(N8nHandler n8nHandler)
        {
            _n8nHandler = n8nHandler;
        }

        [HttpGet("obtenerCliente")]
        public async Task<IActionResult> ObtenerCliente([FromQuery] N8nClienteFiltro request)
        {
            var respuesta = await _n8nHandler.ObtenerClienteAsync(UsuarioLogueado, request?.emailBusqueda);
            return Ok(respuesta);
        }
    }
}
