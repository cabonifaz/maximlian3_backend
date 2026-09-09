using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.PedidoArchivo;

public interface IPedidoArchivoRepository
{
    Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoCrear request);
    Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoEditar request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoArchivo request);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request);
}
