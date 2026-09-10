namespace SafetyReport.Application.Puertos.TablaMaestra;

public interface ITablaMaestraRepository
{
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, string? idsMaestro, string? busqueda, int? numPag);
    Task<Respuesta> ListaCortaAsync(UsuarioGeneral usuarioLogueado, int idMaestro);
    Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TablaMaestraRequest request);
    Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarTablaMaestraRequest request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ObtenerTablaMaestraRequest request);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idTablaMaestra);
}
