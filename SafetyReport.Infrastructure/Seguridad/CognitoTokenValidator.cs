using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SafetyReport.Application.Puertos.Login;
using SafetyReport.Infrastructure.Almacenamiento;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SafetyReport.Infrastructure.Seguridad
{
    public class CognitoTokenValidator : ITokenValidator
    {
        private readonly AwsConfig _awsConfig;
        private readonly CognitoConfig _cognitoConfig;

        public CognitoTokenValidator(AwsConfig awsConfig, CognitoConfig cognitoConfig)
        {
            _awsConfig = awsConfig;
            _cognitoConfig = cognitoConfig;
        }

        public async Task<UsuarioGeneral?> ValidarTokenAsync(string token)
        {
            var validClientIds = new[] { _cognitoConfig.ClientIdFrontend, _cognitoConfig.ClientIdBackend, _cognitoConfig.ClientIdN8n };

            var issuer = $"https://cognito-idp.{_awsConfig.Region}.amazonaws.com/{_cognitoConfig.UserPoolId}";
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

            var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            return new UsuarioGeneral
            {
                Usuario   = usernameClaim ?? string.Empty,
                Sub       = subClaim ?? string.Empty,
                IdUsuario = 0,
                IdEmpresa = 0,
                IdRol     = 0
            };
        }
    }
}
