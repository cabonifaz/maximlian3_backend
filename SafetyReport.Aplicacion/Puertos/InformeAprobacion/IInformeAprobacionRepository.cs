namespace SafetyReport.Application.Puertos.InformeAprobacion;

public interface IInformeAprobacionRepository
{
    Task<Respuesta> ListarPendientesAsync(UsuarioGeneral usuarioLogueado, FiltroInformeAprobacionPendientes filtro);
    Task<Respuesta> AprobarAsync(UsuarioGeneral usuarioLogueado, int idInforme);
}
