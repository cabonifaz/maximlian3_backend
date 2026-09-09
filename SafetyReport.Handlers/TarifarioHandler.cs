using Microsoft.Extensions.Logging;
using SafetyReport.Application.Ports.Tarifario;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class TarifarioHandler
    {
        private readonly ITarifarioRepository _repository;
        private readonly ILogger<TarifarioHandler> _logger;

        public TarifarioHandler(ITarifarioRepository repository, ILogger<TarifarioHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TarifarioCrear request)
        {
            try
            {
                return await _repository.CrearAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioCreado>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, TarifarioFiltro request)
        {
            try
            {
                return await _repository.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new TarifarioListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request)
        {
            try
            {
                return await _repository.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioConsulta>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, TarifarioEditar request)
        {
            try
            {
                return await _repository.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request)
        {
            try
            {
                return await _repository.EliminarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioEliminado>()
                };
            }
        }
        public async Task<Respuesta> ListarCortaAsync(UsuarioGeneral usuarioLogueado, TarifarioListaCortaFiltro request)
        {
            try
            {
                return await _repository.ListaCortaAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new TarifarioListaCortaResult()
                };
            }
        }
    }
}
