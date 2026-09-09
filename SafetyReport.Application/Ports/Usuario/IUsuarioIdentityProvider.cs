using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Usuario;

public interface IUsuarioIdentityProvider
{
    Task<string?> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request, UsuarioCreado usuarioCreado);
    Task EliminarUsuarioAsync(string usuario);
}
