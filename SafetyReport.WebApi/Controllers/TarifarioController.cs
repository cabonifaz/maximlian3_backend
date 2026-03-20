using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TarifarioController : BaseController
    {
        private readonly TarifarioHandler _tarifarioHandler;

        public TarifarioController(TarifarioHandler tarifarioHandler)
        {
            _tarifarioHandler = tarifarioHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] TarifarioCrear request)
        {
            var respuesta = await _tarifarioHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] TarifarioFiltro request)
        {
            var respuesta = await _tarifarioHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] TarifarioIdRequest request)
        {
            var respuesta = await _tarifarioHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] TarifarioEditar request)
        {
            var respuesta = await _tarifarioHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] TarifarioIdRequest request)
        {
            var respuesta = await _tarifarioHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listaCorta")]
        public async Task<IActionResult> ListaCorta([FromQuery] TarifarioListaCortaFiltro request)
        {
            var respuesta = await _tarifarioHandler.ListarCortaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}