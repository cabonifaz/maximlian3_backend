using Microsoft.Extensions.Logging;
using SafetyReport.Application.Ports.Informe;
using SafetyReport.Application.Ports.InformeArchivo;
using SafetyReport.Application.Ports.Storage;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeArchivoHandler
    {
        private readonly IInformeArchivoRepository _informeArchivoRepository;
        private readonly IInformeDraftRepository _informeDraftRepository;
        private readonly IInformeArchivoStorage _informeArchivoStorage;
        private readonly ILogger<InformeArchivoHandler> _logger;

        public InformeArchivoHandler(
            IInformeArchivoRepository informeArchivoRepository,
            IInformeDraftRepository informeDraftRepository,
            IInformeArchivoStorage informeArchivoStorage,
            ILogger<InformeArchivoHandler> logger)
        {
            _informeArchivoRepository = informeArchivoRepository;
            _informeDraftRepository = informeDraftRepository;
            _informeArchivoStorage = informeArchivoStorage;
            _logger = logger;
        }

        public async Task<Respuesta> GenerarUrlsArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoUrlRequest request)
        {
            try
            {
                var idInforme = request.IdInforme;

                if (idInforme == 0)
                {
                    var resultado = await _informeDraftRepository.ObtenerOCrearInformeAsync(usuarioLogueado, request.IdPedido);
                    if (resultado.IdTipoMensaje != 2 || resultado.Result is not List<InformeIdResult> ids || ids.Count == 0)
                        return new Respuesta { IdTipoMensaje = resultado.IdTipoMensaje, Mensaje = resultado.Mensaje, Result = new List<InformeArchivoUrlResult>() };
                    idInforme = ids[0].IdInforme;
                }

                var pendientes = new List<InformeArchivoPendiente>();
                foreach (var nombre in request.Nombres)
                {
                    var ext = Path.GetExtension(nombre);
                    var nombreSinExt = Path.GetFileNameWithoutExtension(nombre);
                    var s3Key = $"informes/pedido-{request.IdPedido}/informe-{idInforme}/adjunto/{nombreSinExt}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{ext}";
                    pendientes.Add(new InformeArchivoPendiente
                    {
                        Nombre = nombre,
                        ArchivoUrl = s3Key,
                        UploadUrl = _informeArchivoStorage.GenerarUploadUrl(s3Key, "application/octet-stream")
                    });
                }

                var result = new InformeArchivoUrlResult { IdInforme = idInforme, Archivos = pendientes };
                return new Respuesta { IdTipoMensaje = 2, Mensaje = "URLs generadas correctamente.", Result = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeArchivoUrlResult>() };
            }
        }

        public async Task<Respuesta> ObtenerArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoIdRequest request)
        {
            try
            {
                var respuesta = await _informeArchivoRepository.ObtenerArchivoAsync(usuarioLogueado, request.IdInformeArchivo);
                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<InformeArchivoConsulta> archivos && archivos.Count > 0)
                {
                    var archivo = archivos[0];
                    archivo.DownloadUrl = _informeArchivoStorage.GenerarDownloadUrl(archivo.ArchivoUrl);
                    archivo.ArchivoUrl = string.Empty;
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeArchivoConsulta>() };
            }
        }

        public async Task<Respuesta> EliminarArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoIdRequest request)
        {
            try
            {
                var obtener = await _informeArchivoRepository.ObtenerArchivoAsync(usuarioLogueado, request.IdInformeArchivo);
                if (obtener.IdTipoMensaje != 2)
                    return obtener;

                if (obtener.Result is List<InformeArchivoConsulta> archivos && archivos.Count > 0)
                    await _informeArchivoStorage.DeleteFileAsync(archivos[0].ArchivoUrl);

                return await _informeArchivoRepository.EliminarArchivoAsync(usuarioLogueado, request.IdInformeArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ActualizarArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoActualizarRequest request)
        {
            try
            {
                return await _informeArchivoRepository.ActualizarArchivoAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }

        public async Task<Respuesta> InsertarArchivoLoteAsync(UsuarioGeneral usuarioLogueado, InformeArchivoInsertarRequest request)
        {
            try
            {
                return await _informeArchivoRepository.InsertarArchivoLoteAsync(
                    usuarioLogueado, request.IdInforme, request.IdPedido, request.Archivos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
            }
        }
    }
}
