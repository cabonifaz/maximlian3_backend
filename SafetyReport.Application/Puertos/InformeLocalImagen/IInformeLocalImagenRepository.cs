using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.InformeLocalImagen;

public interface IInformeLocalImagenRepository
{
    Task<Respuesta> ObtenerUrlsImagenesAsync(UsuarioGeneral usuarioLogueado, List<int> ids);
    Task<Respuesta> ActualizarEstadoCargaAsync(UsuarioGeneral usuarioLogueado, List<int> ids);
    Task ActualizarImagenUrlAsync(UsuarioGeneral usuarioLogueado, int idInformeLocalImagen, string imagenUrl);
}
