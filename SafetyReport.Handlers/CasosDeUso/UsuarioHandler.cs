using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Usuario;
using SafetyReport.Models;

namespace SafetyReport.Handlers.CasosDeUso
{
    public class UsuarioHandler
    {
        private readonly IUsuarioRepository _repository;
        private readonly IUsuarioIdentityProvider _identityProvider;
        private readonly ILogger<UsuarioHandler> _logger;

        public UsuarioHandler(IUsuarioRepository repository, IUsuarioIdentityProvider identityProvider, ILogger<UsuarioHandler> logger)
        {
            _repository = repository;
            _identityProvider = identityProvider;
            _logger = logger;
        }

        public async Task<Respuesta> CrearUsuarioAsync(UsuarioGeneral usuarioLogueado, UsuarioCrear request)
        {
            try
            {
                var respuesta = await _repository.CrearUsuarioAsync(usuarioLogueado, request);

                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var creado = ObtenerPrimerResultado<UsuarioCreado>(respuesta.Result);

                if (creado == null)
                    return respuesta;

                var sub = await _identityProvider.CrearUsuarioAsync(usuarioLogueado, request, creado);

                if (!string.IsNullOrWhiteSpace(sub))
                {
                    var respuestaSub = await _repository.ActualizarSubAsync(usuarioLogueado, creado.IdUsuario, sub);

                    if (respuestaSub.IdTipoMensaje != 2)
                        return respuestaSub;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                return await _repository.EditarUsuarioAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                var respuesta = await _repository.EliminarUsuarioAsync(usuarioLogueado, request.IdUsuarioEliminar);

                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var eliminado = ObtenerPrimerResultado<EliminarUsuarioResult>(respuesta.Result);

                if (eliminado != null && !string.IsNullOrWhiteSpace(eliminado.Usuario))
                {
                    await _identityProvider.EliminarUsuarioAsync(eliminado.Usuario);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                return await _repository.ListarUsuariosAsync(usuarioLogueado, filtro, idEstado, numPag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                return await _repository.ObtenerUsuarioAsync(usuarioLogueado, idUsuarioConsulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                return await _repository.ListarCortaAsync(usuarioLogueado, idRolFiltro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioListaCortaItem>()
                };
            }
        }

        public async Task<Respuesta> ListarCortaDashboardAsync(UsuarioGeneral usuarioLogueado, List<int>? idsRolFiltro)
        {
            try
            {
                return await _repository.ListarCortaDashboardAsync(usuarioLogueado, idsRolFiltro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioListaCortaDashboardItem>()
                };
            }
        }

        public async Task<Respuesta> ListarCortaAsignacionAsync(UsuarioGeneral usuarioLogueado, int idRolFiltro, string? filtro, bool esTraductor, List<int>? idiomasPedido)
        {
            try
            {
                return await _repository.ListarCortaAsignacionAsync(usuarioLogueado, idRolFiltro, filtro, esTraductor, idiomasPedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<UsuarioAsignacionListaCortaItem>()
                };
            }
        }

        public async Task<Respuesta> ObtenerResumenAsync(UsuarioGeneral usuarioLogueado, FiltroUsuarioResumen filtro)
        {
            try
            {
                return await _repository.ObtenerResumenAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new UsuarioCumplimientoResult()
                };
            }
        }

        private static T? ObtenerPrimerResultado<T>(object? result)
        {
            return result switch
            {
                T item => item,
                IEnumerable<T> items => items.FirstOrDefault(),
                _ => default
            };
        }
    }
}
