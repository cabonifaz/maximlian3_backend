using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniaController : BaseController
    {
        private readonly CompaniaHandler _companiaHandler;

        public CompaniaController(CompaniaHandler companiaHandler)
        {
            _companiaHandler = companiaHandler;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] List<CompaniaCrear> request)
        {
            var respuesta = await _companiaHandler.CrearAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] CompaniaEditar request)
        {
            var respuesta = await _companiaHandler.EditarAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] CompaniaObtenerRequest request)
        {
            var respuesta = await _companiaHandler.ObtenerAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroCompania filtro)
        {
            var respuesta = await _companiaHandler.ListarAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] FiltroCompaniaBusqueda filtro)
        {
            var respuesta = await _companiaHandler.BuscarAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpPost("obtenerCoincidencias")]
        public async Task<IActionResult> ObtenerCoincidencias([FromBody] List<CompaniaMatchItem> request)
        {
            var respuesta = await _companiaHandler.ListarMatchAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] CompaniaIdRequest request)
        {
            var respuesta = await _companiaHandler.EliminarAsync(UsuarioLogueado, request.IdCompania);
            return Ok(respuesta);
        }

        [HttpPost("noticia/crear")]
        [Consumes("application/json")]
        public async Task<IActionResult> CrearNoticia([FromBody] CompaniaNoticiaCrear request)
        {
            var respuesta = await _companiaHandler.CrearNoticiaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }


        [HttpPost("noticia/editar")]
        public async Task<IActionResult> EditarNoticia([FromBody] CompaniaNoticiaEditar request)
        {
            var respuesta = await _companiaHandler.EditarNoticiaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("noticia/obtener")]
        public async Task<IActionResult> ObtenerNoticia([FromQuery] CompaniaNoticiaObtenerRequest request)
        {
            var respuesta = await _companiaHandler.ObtenerNoticiaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("noticia/archivo/obtener")]
        public async Task<IActionResult> ObtenerNoticiaArchivo([FromQuery] CompaniaNoticiaArchivoIdRequest request)
        {
            var respuesta = await _companiaHandler.ObtenerNoticiaArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("noticia/archivo/eliminar")]
        public async Task<IActionResult> EliminarNoticiaArchivo([FromBody] CompaniaNoticiaArchivoIdRequest request)
        {
            var respuesta = await _companiaHandler.EliminarNoticiaArchivoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("noticia/listar")]
        public async Task<IActionResult> ListarNoticias([FromQuery] FiltroCompaniaNoticia filtro)
        {
            var respuesta = await _companiaHandler.ListarNoticiasAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpPost("noticia/eliminar")]
        public async Task<IActionResult> EliminarNoticia([FromBody] CompaniaNoticiaIdRequest request)
        {
            var respuesta = await _companiaHandler.EliminarNoticiaAsync(UsuarioLogueado, request.IdCompaniaNoticia);
            return Ok(respuesta);
        }

        [HttpGet("companianoticiabalance/listar")]
        public async Task<IActionResult> ListarNoticiasBalance([FromQuery] FiltroCompaniaNoticiaBalance filtro)
        {
            var respuesta = await _companiaHandler.ListarNoticiasBalanceAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpGet("companianoticiabalance/obtener")]
        public async Task<IActionResult> ObtenerNoticiaBalance([FromQuery] CompaniaNoticiaBalanceObtenerRequest request)
        {
            var respuesta = await _companiaHandler.ObtenerNoticiaBalanceAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("companianoticiadetalle/listar")]
        public async Task<IActionResult> ListarNoticiasDetalle([FromQuery] FiltroCompaniaNoticiaDetalle filtro)
        {
            var respuesta = await _companiaHandler.ListarNoticiasDetalleAsync(UsuarioLogueado, filtro);
            return Ok(respuesta);
        }

        [HttpGet("companianoticiadetalle/exportar")]
        [HttpGet("companianoticiadetalle/exportarExcel")]
        [HttpGet("companianoticiadetalle/exportar.xlsx")]
        [HttpGet("noticia/detalle/exportar")]
        [HttpGet("noticia/detalle/exportarExcel")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        public async Task<IActionResult> ExportarNoticiasDetalle([FromQuery] FiltroCompaniaNoticiaDetalle filtro)
        {
            var respuesta = await _companiaHandler.ExportarNoticiasDetalleAsync(UsuarioLogueado, filtro);

            if (respuesta.IdTipoMensaje != 2 || respuesta.Result is not CompaniaNoticiaDetalleExportacion exportacion)
                return Ok(respuesta);

            return File(
                exportacion.Archivo,
                exportacion.ContentType,
                exportacion.NombreArchivo);
        }
    }
}
