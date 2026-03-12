using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;

namespace SafetyReport.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly LoginHandler _loginHandler;

        public LoginController(LoginHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            var authHeader = Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Ok(new
                {
                    IdTipoMensaje = 1,
                    Mensaje = "No se envió un token Bearer válido.",
                    Result = new object[] { }
                });
            }

            var token = authHeader["Bearer ".Length..].Trim();

            var respuesta = await _loginHandler.AutenticarAsync(token);

            return Ok(respuesta);
        }
    }
}