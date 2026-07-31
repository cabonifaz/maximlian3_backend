using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : BaseController
    {
        private readonly ClienteHandler _clienteHandler;

        public ClienteController(ClienteHandler clienteHandler)
        {
            _clienteHandler = clienteHandler;
        }

        [HttpGet("/resumen")]
        public async Task<IActionResult> Resumen()
        {
            var respuesta = await _clienteHandler.ObtenerResumenClientesAsync(UsuarioLogueado);
            return Ok(respuesta);
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] Cliente request)
        {
            var respuesta = await _clienteHandler.CrearClienteAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpPost("editar")]
        public async Task<IActionResult> Editar([FromBody] EditarCliente request)
        {
            var respuesta = await _clienteHandler.EditarClienteAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> Obtener([FromQuery] int idCliente)
        {
            var respuesta = await _clienteHandler.ObtenerClienteAsync(UsuarioLogueado, idCliente);
            return Ok(respuesta);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar([FromQuery] FiltroCliente request)
        {
            var respuesta = await _clienteHandler.ListarClientesAsync(UsuarioLogueado, request?.busqueda, request?.numPag, request?.idPais, request?.idEstado);
            return Ok(respuesta);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] ClienteIdRequest request)
        {
            var respuesta = await _clienteHandler.EliminarClienteAsync(UsuarioLogueado, request);
            return Ok(respuesta);
        }

        [HttpGet("listaCorta")]
        public async Task<IActionResult> ListaCorta([FromQuery] string? correoBusqueda)
        {
            var respuesta = await _clienteHandler.ListarClienteShortAsync(UsuarioLogueado, correoBusqueda);
            return Ok(respuesta);
        }

        [HttpGet("listarFacturacion")]
        public async Task<IActionResult> ListarFacturacion([FromQuery] FiltroClienteFacturacion request)
        {
            var respuesta = await _clienteHandler.ListarClientesFacturacionAsync(UsuarioLogueado, request?.busqueda, request?.numPag, request?.emitirPrefactura, request?.idIdiomaFacturacion, request?.estadoFacturacion);
            return Ok(respuesta);
        }
    }
}