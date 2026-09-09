using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Informe;

public interface IInformeDraftRepository
{
    Task<Respuesta> ObtenerOCrearInformeAsync(UsuarioGeneral usuarioLogueado, int idPedido);
}
