using SafetyReport.Application.Puertos.Informe;

namespace SafetyReport.Application.Puertos.Login;

public interface ILoginRepository
{
    Task<Respuesta> AutenticarAsync(UsuarioGeneral usuarioActual);
}
