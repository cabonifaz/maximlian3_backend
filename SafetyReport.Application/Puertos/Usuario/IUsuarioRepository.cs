using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.Usuario;

public interface IUsuarioRepository
{
    Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request);
    Task<Respuesta> EditarUsuarioAsync(UsuarioGeneral usuarioLogueado, InfoUsuarioEditar request);
    Task<Respuesta> EliminarUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioEliminar);
    Task<Respuesta> ListarUsuariosAsync(UsuarioGeneral usuarioActual, string? filtro, int? idEstado, int? numPag);
    Task<Respuesta> ObtenerUsuarioAsync(UsuarioGeneral usuarioActual, int idUsuarioConsulta);
    Task<Respuesta> ListarCortaAsync(UsuarioGeneral usuarioActual, int idRolFiltro);
    Task<Respuesta> ListarCortaDashboardAsync(UsuarioGeneral usuarioActual, List<int>? idsRolFiltro);
    Task<Respuesta> ListarCortaAsignacionAsync(UsuarioGeneral usuarioActual, int idRolFiltro, string? filtro, bool esTraductor, List<int>? idiomasPedido);
    Task<Respuesta> ActualizarSubAsync(UsuarioGeneral usuarioActual, int idUsuarioActualizar, string sub);
    Task<Respuesta> ObtenerResumenAsync(UsuarioGeneral usuarioLogueado, FiltroUsuarioResumen filtro);
}
