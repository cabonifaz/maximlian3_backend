using SafetyReport.Models;

namespace SafetyReport.Application.Ports.InformeObservacion;

public interface IInformeObservacionRepository
{
    Task<Respuesta> ListarObservacionesAsync(UsuarioGeneral usuarioLogueado, int idPedido);
    Task<Respuesta> InsertarObservacionesLoteAsync(
        UsuarioGeneral usuarioLogueado, int idInforme, int idPedido, List<InformeObservacionItem> observaciones);
    Task<Respuesta> EditarObservacionAsync(UsuarioGeneral usuarioLogueado, InformeObservacionEditarRequest request);
    Task<Respuesta> EliminarObservacionAsync(UsuarioGeneral usuarioLogueado, int idInformeObservacion);
}
