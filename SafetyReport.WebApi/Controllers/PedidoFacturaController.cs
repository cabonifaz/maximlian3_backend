using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoFacturaController : BaseController
    {
        private readonly PedidoFacturaHandler _pedidoFacturaHandler;

        public PedidoFacturaController(PedidoFacturaHandler pedidoFacturaHandler)
        {
            _pedidoFacturaHandler = pedidoFacturaHandler;
        }

        [HttpPost("guardarBorrador")]
        public async Task<IActionResult> GuardarBorrador([FromBody] GuardarBorradorFacturaRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.GuardarBorradorFacturaAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("notaCreditoDebito")]
        public async Task<IActionResult> GenerarNotaCreditoDebito([FromBody] GenerarNotaCreditoDebitoRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.GenerarNotaCreditoDebitoAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPut("notaCreditoDebito/{idDocumentoElectronico:int}")]
        public async Task<IActionResult> EditarNotaCreditoDebito(int idDocumentoElectronico, [FromBody] EditarNotaCreditoDebitoRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.EditarNotaCreditoDebitoAsync(UsuarioLogueado, idDocumentoElectronico, request);
            return Ok(respuesta);
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen([FromQuery] DateOnly? fechaDesde, [FromQuery] DateOnly? fechaHasta)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerResumenDashboardAsync(UsuarioLogueado, fechaDesde, fechaHasta);
            return Ok(respuesta);
        }

        [HttpGet("listarPedidos")]
        public async Task<IActionResult> ListarPedidos([FromQuery] ListarPedidosFacturacionRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.ListarPedidosParaFacturacionAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listarFacturas")]
        public async Task<IActionResult> ListarFacturas([FromQuery] ListarFacturasRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.ListarFacturasAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("facturaPorId/{idDocumentoElectronico:int}")]
        public async Task<IActionResult> ObtenerFacturaPorId(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerFacturaPorIdAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpGet("facturaPorId/{idDocumentoElectronico:int}/paraNota")]
        public async Task<IActionResult> ObtenerParaNota(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerParaNotaAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpGet("facturaPorId/{idDocumentoElectronico:int}/urlDescarga")]
        public async Task<IActionResult> ObtenerUrlDescarga(int idDocumentoElectronico, [FromQuery] string tipoArchivo)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerUrlDescargaAsync(UsuarioLogueado, idDocumentoElectronico, tipoArchivo);
            return Ok(respuesta);
        }

        [HttpGet("facturaPorId/{idDocumentoElectronico:int}/urlVerificacion")]
        public async Task<IActionResult> ObtenerUrlVerificacion(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerUrlVerificacionAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpGet("facturaPorId/{idDocumentoElectronico:int}/erroresUltimoEnvio")]
        public async Task<IActionResult> ObtenerErroresUltimoEnvio(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerErroresUltimoEnvioAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpGet("sireRvie/txt")]
        public async Task<IActionResult> GenerarTxtSireRvie([FromQuery] DateOnly periodo)
        {
            var respuesta = await _pedidoFacturaHandler.GenerarTxtSireRvieAsync(UsuarioLogueado, periodo);

            if (respuesta.IdTipoMensaje != 2 || respuesta.Result is not SireRvieExportacion exportacion)
                return Ok(respuesta);

            return File(exportacion.Archivo, exportacion.ContentType, exportacion.NombreArchivo);
        }

        [HttpPost("facturaPorId/{idDocumentoElectronico:int}/camposExtra")]
        public async Task<IActionResult> InsertarCampoExtra(int idDocumentoElectronico, [FromBody] CampoExtraRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.InsertarCampoExtraAsync(UsuarioLogueado, idDocumentoElectronico, request.Texto);
            return Ok(respuesta);
        }

        [HttpPost("facturaPorId/{idDocumentoElectronico:int}/camposExtra/lote")]
        public async Task<IActionResult> InsertarLoteCamposExtra(int idDocumentoElectronico, [FromBody] List<CampoExtraRequest> camposExtra)
        {
            var entradas = camposExtra.Select(c => new FacturacionCampoExtraEntrada { Texto = c.Texto }).ToList();
            var respuesta = await _pedidoFacturaHandler.InsertarLoteCamposExtraAsync(UsuarioLogueado, idDocumentoElectronico, entradas);
            return Ok(respuesta);
        }

        [HttpGet("facturaPorId/{idDocumentoElectronico:int}/camposExtra")]
        public async Task<IActionResult> ListarCamposExtra(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.ListarCamposExtraAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpPut("camposExtra/{idCampoExtraDocumentoElectronico:int}")]
        public async Task<IActionResult> ActualizarCampoExtra(int idCampoExtraDocumentoElectronico, [FromBody] CampoExtraRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.ActualizarCampoExtraAsync(UsuarioLogueado, idCampoExtraDocumentoElectronico, request.Texto);
            return Ok(respuesta);
        }

        [HttpDelete("camposExtra/{idCampoExtraDocumentoElectronico:int}")]
        public async Task<IActionResult> EliminarCampoExtra(int idCampoExtraDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.EliminarCampoExtraAsync(UsuarioLogueado, idCampoExtraDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpPut("facturaPorId/{idDocumentoElectronico:int}/cuotas/{idCuotaDocumentoElectronico:int}/estado")]
        public async Task<IActionResult> ActualizarEstadoCuota(
            int idDocumentoElectronico, int idCuotaDocumentoElectronico, [FromBody] ActualizarEstadoCuotaRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.ActualizarEstadoCuotaAsync(
                UsuarioLogueado, idDocumentoElectronico, idCuotaDocumentoElectronico, request.idEstadoCuotaMaestro, request.fechaPago);
            return Ok(respuesta);
        }

        [HttpPut("facturaPorId/{idDocumentoElectronico:int}/anularManualmente")]
        public async Task<IActionResult> AnularManualmente(int idDocumentoElectronico, [FromBody] AnularManualmenteRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.AnularManualmenteAsync(
                UsuarioLogueado, idDocumentoElectronico, request.motivo, request.fechaAnulacion);
            return Ok(respuesta);
        }

        [HttpGet("facturaPorId/{idDocumentoElectronico:int}/anularManualmente/preview")]
        public async Task<IActionResult> PrevisualizarAnulacionManual(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.PrevisualizarAnulacionManualAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpGet("factura/{idPedido:int}")]
        public async Task<IActionResult> ObtenerFacturaPorPedido(int idPedido)
        {
            var respuesta = await _pedidoFacturaHandler.ObtenerFacturaPorPedidoAsync(UsuarioLogueado, idPedido);
            return Ok(respuesta);
        }

        [HttpPut("guardarCambios/{idDocumentoElectronico:int}")]
        public async Task<IActionResult> GuardarCambios(int idDocumentoElectronico, [FromBody] GuardarCambiosFacturaRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.GuardarCambiosFacturaAsync(UsuarioLogueado, idDocumentoElectronico, request);
            return Ok(respuesta);
        }

        [HttpPost("emitir/{idDocumentoElectronico:int}")]
        public async Task<IActionResult> Emitir(int idDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.EmitirFacturaAsync(UsuarioLogueado, idDocumentoElectronico);
            return Ok(respuesta);
        }

        [HttpPut("estado/{idPedido:int}")]
        public async Task<IActionResult> ActualizarEstadoFacturacion(int idPedido, [FromQuery] int idEstadoFacturacion)
        {
            var respuesta = await _pedidoFacturaHandler.ActualizarEstadoFacturacionAsync(UsuarioLogueado, idPedido, idEstadoFacturacion);
            return Ok(respuesta);
        }

        [HttpPost("anular")]
        public async Task<IActionResult> Anular([FromBody] AnularFacturasRequest request)
        {
            var respuesta = await _pedidoFacturaHandler.AnularFacturasAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("anular/preview")]
        public async Task<IActionResult> PrevisualizarAnular([FromQuery] DateOnly fechaReferencia, [FromQuery] List<int> idsDocumentoElectronico)
        {
            var respuesta = await _pedidoFacturaHandler.PrevisualizarBajaAsync(UsuarioLogueado, fechaReferencia, idsDocumentoElectronico);
            return Ok(respuesta);
        }
    }
}
