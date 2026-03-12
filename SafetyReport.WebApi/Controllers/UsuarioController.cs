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
        public async Task<IActionResult> Crear([FromBody] Usuario request)
        {
            var respuesta = await _usuarioHandler.CrearUsuarioAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] EditarUsuario request)
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

        [HttpPost("listar")]
        public async Task<IActionResult> Listar([FromBody] FiltroUsuario request)
        {
            var respuesta = await _usuarioHandler.ListarUsuariosAsync(
                UsuarioLogueado,
                request?.Filtro
            );

            return Ok(respuesta);
        }

        // 🔹 obtener usuario logueado
        [HttpPost("obtener")]
        public async Task<IActionResult> Obtener()
        {
            var respuesta = await _usuarioHandler.ObtenerUsuarioAsync(
                UsuarioLogueado,
                UsuarioLogueado.IdUsuario
            );

            return Ok(respuesta);
        }
    }
}