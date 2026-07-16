using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class LoginHandler
    {
        private readonly LoginDAO _loginDAO;
        private readonly CognitoTokenValidator _tokenValidator;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(LoginDAO loginDAO, CognitoTokenValidator tokenValidator, ILogger<LoginHandler> logger)
        {
            _loginDAO = loginDAO;
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

                return await _loginDAO.AutenticarAsync(usuarioLogueado);
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