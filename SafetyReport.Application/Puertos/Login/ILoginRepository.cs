using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.Login;

public interface ILoginRepository
{
    Task<Respuesta> AutenticarAsync(UsuarioGeneral usuarioActual);
}
