using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Login;

public interface ITokenValidator
{
    Task<UsuarioGeneral?> ValidarTokenAsync(string token);
}
