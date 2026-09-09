using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Compania;

public interface ICompaniaRepository
{
    Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<CompaniaCrear> lstCompanias);
    Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, CompaniaEditar request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, CompaniaObtenerRequest request);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroCompania filtro);
    Task<Respuesta> BuscarAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaBusqueda filtro);
    Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<CompaniaMatchItem> lista);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idCompania);
    Task<Respuesta> CrearNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaCrear request);
    Task<Respuesta> EditarNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaEditar request);
    Task<Respuesta> ObtenerNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaObtenerRequest request);
    Task<Respuesta> ObtenerNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticiaArchivo);
    Task<Respuesta> EliminarNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticiaArchivo);
    Task<Respuesta> ListarNoticiasAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticia filtro);
    Task<Respuesta> EliminarNoticiaAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticia);
    Task<Respuesta> ListarNoticiasBalanceAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaBalance filtro);
    Task<Respuesta> ObtenerNoticiaBalanceAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaBalanceObtenerRequest request);
    Task<Respuesta> ListarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro);
    Task<Respuesta> ExportarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro);
}
