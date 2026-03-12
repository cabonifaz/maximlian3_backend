using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class UsuarioHandler
    {
        private readonly UsuarioDAO _dao;
        private readonly IConfiguration _config;

        public UsuarioHandler(UsuarioDAO dao, IConfiguration config)
        {
            _dao = dao;
            _config = config;
        }

        public async Task<Respuesta> CrearUsuarioAsync(CrearUsuario request)
        {
            try
            {
                var respuesta = await _dao.CrearUsuarioAsync(request);

                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var creado = ((List<UsuarioCreado>)respuesta.Result).FirstOrDefault();

                if (creado == null)
                    return respuesta;

                var accessKey = _config["AWS:AccessKey"];
                var secretKey = _config["AWS:SecretKey"];
                var region = _config["AWS:Region"];
                var userPoolId = _config["Cognito:UserPoolId"];

                var credentials = new BasicAWSCredentials(accessKey, secretKey);

                var client = new AmazonCognitoIdentityProviderClient(
                    credentials,
                    RegionEndpoint.GetBySystemName(region)
                );

                var createRequest = new AdminCreateUserRequest
                {
                    UserPoolId = userPoolId,
                    Username = creado.Username,
                    TemporaryPassword = request.Password,
                    MessageAction = MessageActionType.SUPPRESS,
                    DesiredDeliveryMediums = new List<string>(),
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = request.Email },
                        new AttributeType { Name = "email_verified", Value = "true" },
                        new AttributeType { Name = "custom:id_empresa", Value = request.UsuarioLogueado.IdEmpresa.ToString() },
                        new AttributeType { Name = "custom:id_usuario", Value = creado.IdUsuario.ToString() }
                    }
                };

                var cognitoResp = await client.AdminCreateUserAsync(createRequest);

                await client.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
                {
                    UserPoolId = userPoolId,
                    Username = creado.Username,
                    Password = request.Password,
                    Permanent = true
                });

                var sub = cognitoResp.User.Attributes?
                    .FirstOrDefault(x => x.Name == "sub")?.Value;

                if (!string.IsNullOrWhiteSpace(sub))
                {
                    var respuestaSub = await _dao.ActualizarSubAsync(creado.IdUsuario, sub);

                    if (respuestaSub.IdTipoMensaje != 2)
                        return respuestaSub;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioCreado>()
                };
            }
        }
    }
}