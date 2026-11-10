using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Application.CasosDeUso;
using SafetyReport.Application.Puertos.Asignacion;
using SafetyReport.Application.Puertos.FlujoInforme;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AsignacionController : BaseController
    {
        private readonly AsignacionHandler _asignacionHandler;
        private readonly FlujoInformeHandler _flujoInformeHandler;

        public AsignacionController(AsignacionHandler asignacionHandler, FlujoInformeHandler flujoInformeHandler)
        {
            _asignacionHandler = asignacionHandler;
            _flujoInformeHandler = flujoInformeHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] AsignacionCrear request)
        {
            var respuesta = await _asignacionHandler.InsertarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] AsignacionActualizar request)
        {
            var respuesta = await _asignacionHandler.ActualizarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroAsignacion request)
        {
            var respuesta = await _asignacionHandler.ListarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] int idAsignacion)
        {
            var respuesta = await _asignacionHandler.ObtenerAsync(UsuarioLogueado, idAsignacion);
            return Ok(respuesta);
        }

        [HttpGet("bandeja")]
        public async Task<IActionResult> Bandeja([FromQuery] FiltroAsignacionBandeja filtro)
        {
            var respuesta = await _asignacionHandler.BandejaAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarAsignacion request)
        {
            var respuesta = await _asignacionHandler.EliminarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        // SRC-F1-065: tabs por etapa del flujo analista → traductor (prioridad + vencimiento)
        [HttpGet("listarPorEtapa")]
        public async Task<IActionResult> ListarPorEtapa([FromQuery] FiltroFlujoAsignacion filtro)
        {
            var respuesta = await _flujoInformeHandler.ListarPorEtapaAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        // SRC-F1-065: asigna o reasigna un rol (analista / traductor) de un pedido
        [HttpPost("asignarRol")]
        public async Task<IActionResult> AsignarRol([FromBody] AsignarRolFlujoRequest request)
        {
            var respuesta = await _flujoInformeHandler.AsignarRolAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }
    }
}
