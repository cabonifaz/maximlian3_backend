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

        public async Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, Usuario request)
        {
            try
            {
                var respuesta = await _dao.CrearUsuarioAsync(usuarioLogueado, request);

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
                    DesiredDeliveryMediums = new List<string> { "EMAIL" },
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = request.Email },
                        new AttributeType { Name = "email_verified", Value = "true" },
                        new AttributeType { Name = "custom:id_empresa", Value = usuarioLogueado.IdEmpresa.ToString() },
                        new AttributeType { Name = "custom:id_usuario", Value = creado.IdUsuario.ToString() }
                    }
                };

                var cognitoResp = await client.AdminCreateUserAsync(createRequest);

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
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EditarUsuarioAsync(UsuarioGeneral usuarioLogueado, InfoUsuarioEditar request)
        {
            try
            {
                return await _dao.EditarUsuarioAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarUsuarioAsync(UsuarioGeneral usuarioLogueado, EliminarUsuario request)
        {
            try
            {
                var respuesta = await _dao.EliminarUsuarioAsync(usuarioLogueado, request.IdUsuarioEliminar);

                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var eliminado = ((List<EliminarUsuarioResult>)respuesta.Result).FirstOrDefault();

                if (eliminado != null && !string.IsNullOrWhiteSpace(eliminado.Username))
                {
                    var accessKey = _config["AWS:AccessKey"];
                    var secretKey = _config["AWS:SecretKey"];
                    var region = _config["AWS:Region"];
                    var userPoolId = _config["Cognito:UserPoolId"];

                    var credentials = new BasicAWSCredentials(accessKey, secretKey);
                    var client = new AmazonCognitoIdentityProviderClient(
                        credentials,
                        RegionEndpoint.GetBySystemName(region)
                    );

                    await client.AdminDeleteUserAsync(new AdminDeleteUserRequest
                    {
                        UserPoolId = userPoolId,
                        Username = eliminado.Username
                    });
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<EliminarUsuario>()
                };
            }
        }

        public async Task<Respuesta> ListarUsuariosAsync(UsuarioGeneral usuarioLogueado, string? filtro, int? numPag)
        {
            try
            {
                return await _dao.ListarUsuariosAsync(usuarioLogueado, filtro, numPag);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioListaResult>()
                };
            }
        }

        public async Task<Respuesta> ObtenerUsuarioAsync(UsuarioGeneral usuarioLogueado, int idUsuarioConsulta)
        {
            try
            {
                return await _dao.ObtenerUsuarioAsync(usuarioLogueado, idUsuarioConsulta);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioConsulta>()
                };
            }
        }
    }
}