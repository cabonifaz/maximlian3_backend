using SafetyReport.Models;

namespace SafetyReport.Application.Ports.PedidoFacturaLinea
{
    public interface IPedidoFacturaLineaRepository
    {
        Task<Respuesta> CrearAsync(
            UsuarioGeneral usuarioLogueado, int idCliente, List<int> idPedidos, string? codigo, string descripcion);

        Task<Respuesta> CrearLoteAsync(UsuarioGeneral usuarioLogueado, int idCliente, List<GrupoLineaLoteRequest> grupos);

        Task<Respuesta> ActualizarDatosAsync(
            UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea, string? codigo, string descripcion,
            decimal valorUnitario, decimal descuento);

        Task<Respuesta> ActualizarPedidosAsync(
            UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea, int idCliente, List<int> idPedidos);

        Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, ListarLineasFacturacionRequest request);

        Task<Respuesta> DesvincularAsync(UsuarioGeneral usuarioLogueado, int idPedidoFacturaLinea);

        Task<Respuesta> ObtenerParaBorradorAsync(
            UsuarioGeneral usuarioLogueado, int idCliente, int idMoneda, List<int> idsLinea,
            int? idDocumentoElectronico = null);
    }
}
