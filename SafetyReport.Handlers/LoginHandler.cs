using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class LoginHandler
    {
        private readonly LoginDAO _loginDAO;
        private readonly CognitoTokenValidator _tokenValidator;

        public LoginHandler(LoginDAO loginDAO, CognitoTokenValidator tokenValidator)
        {
            _loginDAO = loginDAO;
            _tokenValidator = tokenValidator;
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
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = $"Error al autenticar: {ex.Message}",
                    Result = new List<UsuarioLoginResponse>()
                };
            }
        }
    }
}