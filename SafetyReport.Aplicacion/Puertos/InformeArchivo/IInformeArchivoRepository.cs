namespace SafetyReport.Application.Puertos.InformeArchivo;

public interface IInformeArchivoRepository
{
    Task<Respuesta> ObtenerArchivoAsync(UsuarioGeneral usuarioLogueado, int idInformeArchivo);
    Task<Respuesta> EliminarArchivoAsync(UsuarioGeneral usuarioLogueado, int idInformeArchivo);
    Task<Respuesta> ActualizarArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoActualizarRequest request);
    Task<Respuesta> InsertarArchivoLoteAsync(
        UsuarioGeneral usuarioLogueado, int idInforme, int idPedido, List<InformeArchivoItem> archivos);
}
