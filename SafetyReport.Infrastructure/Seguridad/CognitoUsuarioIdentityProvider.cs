using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using SafetyReport.Application.Puertos.Usuario;

namespace SafetyReport.Infrastructure.Seguridad;

public class CognitoUsuarioIdentityProvider : IUsuarioIdentityProvider
{
    private readonly IConfiguration _config;

    public CognitoUsuarioIdentityProvider(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string?> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request, UsuarioCreado usuarioCreado)
    {
        using var clienteCognito = CrearClienteCognito();

        var respuestaCognito = await clienteCognito.AdminCreateUserAsync(new AdminCreateUserRequest
        {
            UserPoolId = _config["Cognito:UserPoolId"],
            Username = usuarioCreado.Usuario,
            DesiredDeliveryMediums = new List<string> { "EMAIL" },
            UserAttributes = new List<AttributeType>
            {
                new() { Name = "email", Value = request.Correo },
                new() { Name = "email_verified", Value = "true" },
                new() { Name = "custom:id_empresa", Value = usuarioLogueado.IdEmpresa.ToString() },
                new() { Name = "custom:id_usuario", Value = usuarioCreado.IdUsuario.ToString() }
            }
        });

        return respuestaCognito.User.Attributes?
            .FirstOrDefault(x => x.Name == "sub")?.Value;
    }

    public async Task EliminarUsuarioAsync(string usuario)
    {
        using var clienteCognito = CrearClienteCognito();

        await clienteCognito.AdminDeleteUserAsync(new AdminDeleteUserRequest
        {
            UserPoolId = _config["Cognito:UserPoolId"],
            Username = usuario
        });
    }

    private AmazonCognitoIdentityProviderClient CrearClienteCognito()
    {
        var credenciales = new BasicAWSCredentials(_config["AWS:AccessKey"], _config["AWS:SecretKey"]);

        return new AmazonCognitoIdentityProviderClient(
            credenciales,
            RegionEndpoint.GetBySystemName(_config["AWS:Region"])
        );
    }
}
