using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Login;

public interface ILoginRepository
{
    Task<Respuesta> AutenticarAsync(UsuarioGeneral usuarioActual);
}
