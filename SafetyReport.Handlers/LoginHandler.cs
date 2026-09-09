using Microsoft.Extensions.Logging;
using SafetyReport.Application.Ports.Login;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class LoginHandler
    {
        private readonly ILoginRepository _loginRepository;
        private readonly ITokenValidator _tokenValidator;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(ILoginRepository loginRepository, ITokenValidator tokenValidator, ILogger<LoginHandler> logger)
        {
            _loginRepository = loginRepository;
            _tokenValidator = tokenValidator;
            _logger = logger;
        }

        public async Task<Respuesta> AutenticarAsync(string token)
        {
            try
            {
                var usuarioLogueado = await _tokenValidator.ValidarTokenAsync(token);

                if (usuarioLogueado == null)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "Token inválido.",
                        Result = new List<UsuarioLoginResponse>()
                    };
                }

                return await _loginRepository.AutenticarAsync(usuarioLogueado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioLoginResponse>()
                };
            }
        }
    }
}
