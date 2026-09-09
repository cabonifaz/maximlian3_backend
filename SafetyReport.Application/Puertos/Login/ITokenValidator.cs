using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.Login;

public interface ITokenValidator
{
    Task<UsuarioGeneral?> ValidarTokenAsync(string token);
}
