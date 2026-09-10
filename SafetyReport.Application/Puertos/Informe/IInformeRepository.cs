using SafetyReport.Application.Puertos.Informe;

namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeRepository : IInformeDraftRepository
{
    Task<(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)> InsertarAsync(
        UsuarioGeneral usuarioLogueado, InformeCrear request);

    Task<(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)> ActualizarAsync(
        UsuarioGeneral usuarioLogueado, InformeEditar request);

    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, int idPedido, int idInforme);

    Task<(Respuesta respuesta, string? nombreInforme, bool requiereTraduccion, int cantidadEnvios, string formatosCliente)>
        GenerarDocumentoAsync(UsuarioGeneral usuarioLogueado, int idInforme, int idPedido);

    Task<(Respuesta respuesta, string? nombreInforme)> GenerarDocumentoXmlAsync(
        UsuarioGeneral usuarioLogueado, int idInforme, int idPedido);

    Task<Respuesta> ObtenerDocumentoAsync(UsuarioGeneral usuarioLogueado, int idInforme, int idPedido);
    Task<Respuesta> ActualizarEstadoAsync(UsuarioGeneral usuarioLogueado, int idInforme, int idEstadoInforme);
    Task<Respuesta> ObtenerDatosNotificacionInformeAsync(UsuarioGeneral usuarioLogueado, int idInforme);
    Task<Respuesta> RegistrarEnvioInformeAsync(UsuarioGeneral usuarioLogueado, int idInforme, int idPedido);
    Task<Respuesta> ObtenerRutaDocumentoAsync(UsuarioGeneral usuarioLogueado, int idInforme, int idPedido);
    Task<Respuesta> ActualizarDocumentoAsync(UsuarioGeneral usuarioLogueado, int idInforme, string urlDocumento);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroInforme filtro);
    Task<Respuesta> ListarIdPorCompaniaAsync(UsuarioGeneral usuarioLogueado, FiltroInformeIdPorCompania filtro);
    Task<Respuesta> CalcularBalanceDesagregadoAsync(UsuarioGeneral usuarioLogueado, InformeBalanceDesagregadoCalcularRequest request);
    Task<Respuesta> CalcularBalanceSeguroAsync(UsuarioGeneral usuarioLogueado, InformeBalanceSeguroCalcularRequest request);
    Task<Respuesta> CalcularBalanceBancoAsync(UsuarioGeneral usuarioLogueado, InformeBalanceBancoCalcularRequest request);
    Task<Respuesta> CalcularBalanceTurquiaAsync(UsuarioGeneral usuarioLogueado, InformeBalanceTurquiaCalcularRequest request);
    Task<Respuesta> CalcularBalanceTotalizadoAsync(UsuarioGeneral usuarioLogueado, InformeBalanceTotalizadoCalcularRequest request);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idInforme);
    Task<Respuesta> ObtenerEvolucionAsync(UsuarioGeneral usuarioLogueado, EvolucionInformesRequest filtro);
}
