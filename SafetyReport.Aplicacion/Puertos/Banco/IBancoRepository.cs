namespace SafetyReport.Application.Puertos.Banco;

public interface IBancoRepository
{
    Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<BancoCrear> lstBancos);
    Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, BancoEditar request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, BancoObtenerRequest request);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroBanco filtro);
    Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<BancoMatchItem> lista);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idBanco);
}
