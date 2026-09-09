using Microsoft.Extensions.Logging;
using SafetyReport.Application.Ports.InformeLocalImagen;
using SafetyReport.Application.Ports.Storage;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeLocalImagenHandler
    {
        private readonly IInformeLocalImagenRepository _informeLocalImagenRepository;
        private readonly IInformeLocalImagenStorage _informeLocalImagenStorage;
        private readonly ILogger<InformeLocalImagenHandler> _logger;

        public InformeLocalImagenHandler(
            IInformeLocalImagenRepository informeLocalImagenRepository,
            IInformeLocalImagenStorage informeLocalImagenStorage,
            ILogger<InformeLocalImagenHandler> logger)
        {
            _informeLocalImagenRepository = informeLocalImagenRepository;
            _informeLocalImagenStorage = informeLocalImagenStorage;
            _logger = logger;
        }

        public async Task<Respuesta> ObtenerUrlsImagenesAsync(UsuarioGeneral usuarioLogueado, InformeLocalImagenEstadoCargaRequest request)
        {
            try
            {
                var respuesta = await _informeLocalImagenRepository.ObtenerUrlsImagenesAsync(usuarioLogueado, request.Ids);

                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<InformeLocalImagenUrl> imagenes && imagenes.Count > 0)
                {
                    var urls = _informeLocalImagenStorage.GenerarDownloadUrlsBatch(imagenes.Select(i => i.ImagenURL).ToList());
                    for (int i = 0; i < imagenes.Count; i++)
                    {
                        imagenes[i].DownloadUrl = urls[i];
                        imagenes[i].ImagenURL   = string.Empty;
                    }
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeLocalImagenUrl>() };
            }
        }

        public async Task<Respuesta> ActualizarEstadoCargaAsync(UsuarioGeneral usuarioLogueado, InformeLocalImagenEstadoCargaRequest request)
        {
            try
            {
                return await _informeLocalImagenRepository.ActualizarEstadoCargaAsync(usuarioLogueado, request.Ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }
    }
}
