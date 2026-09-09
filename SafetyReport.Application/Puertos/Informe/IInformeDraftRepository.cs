using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeDraftRepository
{
    Task<Respuesta> ObtenerOCrearInformeAsync(UsuarioGeneral usuarioLogueado, int idPedido);
}
