using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.Cliente;

public interface IClienteRepository
{
    Task<Respuesta> CrearClienteAsync(UsuarioGeneral usuarioLogueado, Models.Cliente request);
    Task<Respuesta> EditarClienteAsync(UsuarioGeneral usuarioLogueado, EditarCliente request);
    Task<Respuesta> ObtenerClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente);
    Task<Respuesta> ObtenerClientePorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico);
    Task<Respuesta> ObtenerConLineasPorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico);
    Task<Respuesta> ListarClientesAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? idPais, int? idEstado);
    Task<Respuesta> EliminarClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente);
    Task<Respuesta> ActivarDesactivarClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente, int idEstado);
    Task<Respuesta> ListarClienteShortAsync(UsuarioGeneral usuarioLogueado, string? correoBusqueda);
    Task<Respuesta> ListarClientesFacturacionAsync(UsuarioGeneral usuarioLogueado, string? busqueda, int? numPag, int? emitirPrefactura, int? idIdiomaFacturacion);
    Task<Respuesta> ListarPedidosFacturacionClienteAsync(UsuarioGeneral usuarioLogueado, int idCliente, string? busqueda, int? numPag);
    Task<Respuesta> ObtenerResumenClientesAsync(UsuarioGeneral usuarioLogueado);
}
