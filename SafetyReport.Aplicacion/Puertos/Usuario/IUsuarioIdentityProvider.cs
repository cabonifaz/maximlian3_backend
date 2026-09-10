namespace SafetyReport.Application.Puertos.Usuario;

public interface IUsuarioIdentityProvider
{
    Task<string?> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request, UsuarioCreado usuarioCreado);
    Task EliminarUsuarioAsync(string usuario);
}
