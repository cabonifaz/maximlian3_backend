using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.PedidoFactura
{
    public interface IPedidoFacturaRepository
    {
        Task<Respuesta> ObtenerDatosBorradorAsync(UsuarioGeneral usuarioLogueado, int? idCliente, List<int> idPedidos);
        Task<Respuesta> ObtenerIdDocumentoElectronicoAsync(UsuarioGeneral usuarioLogueado, int idPedido);
        Task<Respuesta> RegistrarEnvioAsync(UsuarioGeneral usuarioLogueado, List<int> idsLinea, int idDocumentoElectronico);
        Task<Respuesta> DesvincularAsync(UsuarioGeneral usuarioLogueado, int idDocumentoElectronico, List<int> idsLineaMantener);
        Task<Respuesta> ActualizarEstadoAsync(UsuarioGeneral usuarioLogueado, int idPedido, int idEstadoFacturacion);
        Task<Respuesta> ObtenerCheckpointsSincronizacionAsync();
        Task<Respuesta> ActualizarCheckpointSincronizacionAsync(List<(int IdEmpresa, int UltimoIdEvento)> checkpoints);
        Task<Respuesta> ActualizarEstadoPorDocumentoAsync(int idEmpresa, List<(int IdDocumentoElectronico, int IdEstadoFacturacion)> documentosConEstado);
        Task<Respuesta> ValidarAccesoResumenAsync(UsuarioGeneral usuarioLogueado);
        Task<Respuesta> ObtenerResumenAnaliticoAsync(UsuarioGeneral usuarioLogueado, FiltroFacturacionAnaliticaRequest filtro);
        Task<Respuesta> ObtenerResumenClientesGlobalAsync(UsuarioGeneral usuarioLogueado);
    }
}
