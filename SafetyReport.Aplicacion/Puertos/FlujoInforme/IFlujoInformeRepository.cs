namespace SafetyReport.Application.Puertos.FlujoInforme;

public interface IFlujoInformeRepository
{
    Task<Respuesta> ListarPorEtapaAsync(UsuarioGeneral usuarioLogueado, FiltroFlujoAsignacion filtro);
    Task<Respuesta> AsignarRolAsync(UsuarioGeneral usuarioLogueado, int idPedido, int idRolAsignado, int idUsuarioAsignado);
    Task<Respuesta> ListarRepositorioAsync(UsuarioGeneral usuarioLogueado, FiltroRepositorioInformes filtro);
    Task<Respuesta> ListarComplementosAsync(UsuarioGeneral usuarioLogueado, FiltroComplementos filtro);
    Task<Respuesta> CrearComplementoAsync(UsuarioGeneral usuarioLogueado, CrearComplementoRequest request);
    Task<Respuesta> EnviarAprobacionComplementoAsync(UsuarioGeneral usuarioLogueado, int idComplemento);
}
