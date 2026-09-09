using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.ClienteContacto;

public interface IClienteContactoRepository
{
    Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, ClienteContactoCrear request);
    Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoFiltro request);
    Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ClienteContactoIdRequest request);
    Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoEditar request);
    Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, ClienteContactoIdRequest request);
}
