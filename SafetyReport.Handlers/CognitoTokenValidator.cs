using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SafetyReport.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SafetyReport.Handlers
{
    public class CognitoTokenValidator
    {
        private readonly IConfiguration _configuration;

        public CognitoTokenValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<UsuarioGeneral?> ValidarTokenAsync(string token)
        {
            var region = _configuration["AWS:Region"];
            var userPoolId = _configuration["Cognito:UserPoolId"];
            var clientIdFrontend = _configuration["Cognito:ClientIdFrontend"];
            var clientIdBackend = _configuration["Cognito:ClientIdBackend"];
            var validClientIds = new[] { clientIdFrontend, clientIdBackend };

            var issuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
            var metadataAddress = $"{issuer}/.well-known/openid-configuration";

            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever());

            var openIdConfig = await configManager.GetConfigurationAsync();

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConfig.SigningKeys,
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;

            var tokenUse = jwt.Claims.FirstOrDefault(c => c.Type == "token_use")?.Value;
            var clientIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;

            if (!string.Equals(tokenUse, "access", StringComparison.OrdinalIgnoreCase))
                return null;

            if (!validClientIds.Contains(clientIdClaim, StringComparer.Ordinal))
                return null;

            // El access token expone el username en el claim "username"
            var usernameClaim =
                principal.FindFirst("username")?.Value ??
                principal.FindFirst("cognito:username")?.Value ??
                principal.FindFirst(ClaimTypes.Name)?.Value;

            return new UsuarioGeneral
            {
                Username  = usernameClaim ?? string.Empty,
                IdUsuario = 0,
                IdEmpresa = 0,
                IdRol     = 0
            };
        }
    }
}
