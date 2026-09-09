namespace SafetyReport.Application.Puertos.Asignacion;

public interface IAsignacionRepository
{
    Task<Respuesta> InsertarAsync(UsuarioGeneral usuarioLogueado, AsignacionCrear request);
    Task<Respuesta> ActualizarAsync(UsuarioGeneral usuarioLogueado, AsignacionActualizar request);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroAsignacion request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, int idAsignacion);
    Task<Respuesta> BandejaAsync(UsuarioGeneral usuarioLogueado, FiltroAsignacionBandeja filtro);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, EliminarAsignacion request);
}
