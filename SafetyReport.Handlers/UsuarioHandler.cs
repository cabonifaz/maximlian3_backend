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

        public async Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request)
        {
            try
            {
                var respuesta = await _dao.CrearUsuarioAsync(usuarioLogueado, request);

                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var creado = ((List<UsuarioCreado>)respuesta.Result).FirstOrDefault();

                if (creado == null)
                    return respuesta;

                var llaveAcceso = _config["AWS:AccessKey"];
                var llaveSecreta = _config["AWS:SecretKey"];
                var region = _config["AWS:Region"];
                var idPoolUsuarios = _config["Cognito:UserPoolId"];

                var credenciales = new BasicAWSCredentials(llaveAcceso, llaveSecreta);

                var clienteCognito = new AmazonCognitoIdentityProviderClient(
                    credenciales,
                    RegionEndpoint.GetBySystemName(region)
                );

                var solicitudCrear = new AdminCreateUserRequest
                {
                    UserPoolId = idPoolUsuarios,
                    Username = creado.Usuario,
                    DesiredDeliveryMediums = new List<string> { "EMAIL" },
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = request.Correo },
                        new AttributeType { Name = "email_verified", Value = "true" },
                        new AttributeType { Name = "custom:id_empresa", Value = usuarioLogueado.IdEmpresa.ToString() },
                        new AttributeType { Name = "custom:id_usuario", Value = creado.IdUsuario.ToString() }
                    }
                };

                var respuestaCognito = await clienteCognito.AdminCreateUserAsync(solicitudCrear);

                var sub = respuestaCognito.User.Attributes?
                    .FirstOrDefault(x => x.Name == "sub")?.Value;

                if (!string.IsNullOrWhiteSpace(sub))
                {
                    var respuestaSub = await _dao.ActualizarSubAsync(usuarioLogueado, creado.IdUsuario, sub);

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

                if (eliminado != null && !string.IsNullOrWhiteSpace(eliminado.Usuario))
                {
                    var llaveAcceso = _config["AWS:AccessKey"];
                    var llaveSecreta = _config["AWS:SecretKey"];
                    var region = _config["AWS:Region"];
                    var idPoolUsuarios = _config["Cognito:UserPoolId"];

                    var credenciales = new BasicAWSCredentials(llaveAcceso, llaveSecreta);
                    var clienteCognito = new AmazonCognitoIdentityProviderClient(
                        credenciales,
                        RegionEndpoint.GetBySystemName(region)
                    );

                    await clienteCognito.AdminDeleteUserAsync(new AdminDeleteUserRequest
                    {
                        UserPoolId = idPoolUsuarios,
                        Username = eliminado.Usuario
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

        public async Task<Respuesta> ListarUsuariosAsync(UsuarioGeneral usuarioLogueado, string? filtro, int? idEstado, int? numPag)
        {
            try
            {
                return await _dao.ListarUsuariosAsync(usuarioLogueado, filtro, idEstado, numPag);
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

        public async Task<Respuesta> ListarCortaAsync(UsuarioGeneral usuarioLogueado, int idRolFiltro)
        {
            try
            {
                return await _dao.ListarCortaAsync(usuarioLogueado, idRolFiltro);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioListaCortaItem>()
                };
            }
        }

        public async Task<Respuesta> ListarCortaAsignacionAsync(UsuarioGeneral usuarioLogueado, int idRolFiltro, string? filtro, bool esTraductor, List<int>? idiomasPedido)
        {
            try
            {
                return await _dao.ListarCortaAsignacionAsync(usuarioLogueado, idRolFiltro, filtro, esTraductor, idiomasPedido);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioAsignacionListaCortaItem>()
                };
            }
        }
    }
}