using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UsuarioController : BaseController
    {
        private readonly UsuarioHandler _usuarioHandler;

        public UsuarioController(UsuarioHandler usuarioHandler)
        {
            _usuarioHandler = usuarioHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] UsuarioCrear request)
        {
            var respuesta = await _usuarioHandler.CrearUsuarioAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] InfoUsuarioEditar request)
        {
            var respuesta = await _usuarioHandler.EditarUsuarioAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarUsuario request)
        {
            var respuesta = await _usuarioHandler.EliminarUsuarioAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroUsuario request)
        {
            var respuesta = await _usuarioHandler.ListarUsuariosAsync(
                UsuarioLogueado,
                request?.Filtro,
                request?.numPag
            );

            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] int IdUsuario)
        {
            var respuesta = await _usuarioHandler.ObtenerUsuarioAsync(
                UsuarioLogueado,
                IdUsuario
            );

            return Ok(respuesta);
        }

        [HttpGet("listaCorta")]
        public async Task<IActionResult> ListarCorta([FromQuery] int IdRolFiltro)
        {
            var respuesta = await _usuarioHandler.ListarCortaAsync(
                UsuarioLogueado,
                IdRolFiltro
            );

            return Ok(respuesta);
        }
    }
}