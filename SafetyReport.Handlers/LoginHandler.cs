using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class LoginHandler
    {
        private readonly LoginDAO _loginDAO;

        public LoginHandler(LoginDAO loginDAO)
        {
            _loginDAO = loginDAO;
        }

        public async Task<Respuesta> AutenticarAsync(CognitoLoginRequest request)
        {
            try
            {
                return await _loginDAO.AutenticarAsync(request);
            }
            catch
            {
                throw;
            }
        }
    }
}