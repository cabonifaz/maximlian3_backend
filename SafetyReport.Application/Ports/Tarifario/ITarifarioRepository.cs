using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Tarifario;

public interface ITarifarioRepository
{
    Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TarifarioCrear request);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, TarifarioFiltro request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request);
    Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, TarifarioEditar request);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request);
    Task<Respuesta> ListaCortaAsync(UsuarioGeneral usuarioLogueado, TarifarioListaCortaFiltro request);
}
