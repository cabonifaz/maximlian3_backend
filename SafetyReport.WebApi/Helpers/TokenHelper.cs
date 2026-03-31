using System.Security.Claims;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Helpers
{
    public static class TokenHelper
    {
        public static UsuarioGeneral GetUsuario(ClaimsPrincipal user, HttpRequest request)
        {
            // El access token de Cognito expone el username en el claim "username"
            var usernameClaim =
                user.FindFirst("username")?.Value ??
                user.FindFirst("cognito:username")?.Value ??
                user.FindFirst(ClaimTypes.Name)?.Value ??
                request.Headers["username"].ToString();

            // Los datos del usuario llegan como headers, ya no provienen del token
            var idUsuarioHeader = request.Headers["idUsuario"].ToString();
            var idEmpresaHeader = request.Headers["idEmpresa"].ToString();
            var idRolHeader     = request.Headers["idRol"].ToString();

            return new UsuarioGeneral
            {
                IdUsuario = int.TryParse(idUsuarioHeader, out var idUsuario) ? idUsuario : 0,
                IdEmpresa = int.TryParse(idEmpresaHeader, out var idEmpresa) ? idEmpresa : 0,
                Usuario   = usernameClaim ?? string.Empty,
                IdRol     = int.TryParse(idRolHeader, out var idRol) ? idRol : 0
            };
        }
    }
}
