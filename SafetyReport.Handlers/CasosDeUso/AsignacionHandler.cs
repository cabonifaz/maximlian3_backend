using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Asignacion;
using SafetyReport.Models;

namespace SafetyReport.Handlers.CasosDeUso
{
    public class AsignacionHandler
    {
        private readonly IAsignacionRepository _asignacionRepository;
        private readonly ILogger<AsignacionHandler> _logger;

        public AsignacionHandler(IAsignacionRepository asignacionRepository, ILogger<AsignacionHandler> logger)
        {
            _asignacionRepository = asignacionRepository;
            _logger = logger;
        }

        public async Task<Respuesta> InsertarAsync(UsuarioGeneral usuarioLogueado, AsignacionCrear request)
        {
            try
            {
                return await _asignacionRepository.InsertarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionCreada>()
                };
            }
        }

        public async Task<Respuesta> ActualizarAsync(UsuarioGeneral usuarioLogueado, AsignacionActualizar request)
        {
            try
            {
                return await _asignacionRepository.ActualizarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionCreada>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroAsignacion request)
        {
            try
            {
                return await _asignacionRepository.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new AsignacionListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, int idAsignacion)
        {
            try
            {
                return await _asignacionRepository.ObtenerAsync(usuarioLogueado, idAsignacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<AsignacionConsulta>()
                };
            }
        }

        public async Task<Respuesta> BandejaAsync(UsuarioGeneral usuarioLogueado, FiltroAsignacionBandeja filtro)
        {
            try
            {
                return await _asignacionRepository.BandejaAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new AsignacionBandejaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, EliminarAsignacion request)
        {
            try
            {
                return await _asignacionRepository.EliminarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<EliminarAsignacionResult>()
                };
            }
        }
    }
}
