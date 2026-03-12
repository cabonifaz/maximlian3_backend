using System.Security.Claims;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Helpers
{
    public static class TokenHelper
    {
        public static UsuarioGeneral GetUsuario(ClaimsPrincipal user)
        {
            var idUsuarioClaim =
                user.FindFirst("custom:id_usuario")?.Value ??
                user.FindFirst("id_usuario")?.Value;

            var idEmpresaClaim =
                user.FindFirst("custom:id_empresa")?.Value ??
                user.FindFirst("id_empresa")?.Value;

            var usernameClaim =
                user.FindFirst("cognito:username")?.Value ??
                user.FindFirst("username")?.Value ??
                user.FindFirst(ClaimTypes.Name)?.Value;

            return new UsuarioGeneral
            {
                IdUsuario = int.TryParse(idUsuarioClaim, out var idUsuario) ? idUsuario : 0,
                IdEmpresa = int.TryParse(idEmpresaClaim, out var idEmpresa) ? idEmpresa : 0,
                Username = usernameClaim ?? string.Empty,
                IdRol = 0
            };
        }
    }
}