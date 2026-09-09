using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Pedido;

public interface IPedidoRepository
{
    Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Models.Pedido request);
    Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarPedido request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoObtener request);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedido request);
    Task<Respuesta> ListarAsignacionAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoAsignacion request);
    Task<Respuesta> CancelarAsync(UsuarioGeneral usuarioLogueado, int idPedido);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idPedido);
    Task<Respuesta> ObtenerResumenAsync(UsuarioGeneral usuarioLogueado);
    Task<Respuesta> ListarParaFacturacionAsync(UsuarioGeneral usuarioLogueado, ListarPedidosFacturacionRequest request);
    Task<Respuesta> ListarParaFacturacionConGruposAsync(UsuarioGeneral usuarioLogueado, ListarPedidosFacturacionConGruposRequest request);
    Task<Respuesta> ListarPorDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico);
    Task<Respuesta> ListarPorDocumentoElectronicoPublicoAsync(int idEmpresa, int idDocumentoElectronico);
    Task<Respuesta> ListarParaPrefacturaAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoPrefactura request);
}
