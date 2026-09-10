using SafetyReport.Application.Puertos.Informe;

namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeDraftRepository
{
    Task<Respuesta> ObtenerOCrearInformeAsync(UsuarioGeneral usuarioLogueado, int idPedido);
}
