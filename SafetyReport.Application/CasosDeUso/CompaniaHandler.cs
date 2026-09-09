using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.Compania;
using SafetyReport.Application.Puertos.Almacenamiento;
using SafetyReport.Models;

namespace SafetyReport.Application.CasosDeUso
{
    public class CompaniaHandler
    {
        private readonly ICompaniaRepository _companiaRepository;
        private readonly ICompaniaNoticiaStorage _companiaNoticiaStorage;
        private readonly ICompaniaNoticiasDetalleExcelExporter _noticiasDetalleExcelExporter;
        private readonly ILogger<CompaniaHandler> _logger;

        public CompaniaHandler(
            ICompaniaRepository companiaRepository,
            ICompaniaNoticiaStorage companiaNoticiaStorage,
            ICompaniaNoticiasDetalleExcelExporter noticiasDetalleExcelExporter,
            ILogger<CompaniaHandler> logger)
        {
            _companiaRepository = companiaRepository;
            _companiaNoticiaStorage = companiaNoticiaStorage;
            _noticiasDetalleExcelExporter = noticiasDetalleExcelExporter;
            _logger = logger;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<CompaniaCrear> lstCompanias)
        {
            try
            {
                return await _companiaRepository.CrearAsync(usuarioLogueado, lstCompanias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, CompaniaEditar request)
        {
            try
            {
                return await _companiaRepository.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, CompaniaObtenerRequest request)
        {
            try
            {
                return await _companiaRepository.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroCompania filtro)
        {
            try
            {
                return await _companiaRepository.ListarAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaListaResult() };
            }
        }

        public async Task<Respuesta> BuscarAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaBusqueda filtro)
        {
            try
            {
                return await _companiaRepository.BuscarAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaBusquedaItem>() };
            }
        }

        public async Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<CompaniaMatchItem> lista)
        {
            try
            {
                return await _companiaRepository.ListarMatchAsync(usuarioLogueado, lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaMatchResultItem>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idCompania)
        {
            try
            {
                return await _companiaRepository.EliminarAsync(usuarioLogueado, idCompania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaEliminada>() };
            }
        }

        public async Task<Respuesta> CrearNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaCrear request)
        {
            try
            {
                await PrepararArchivosNoticiaAsync(usuarioLogueado, request.IdCompania, request.Archivos);
                var respuesta = await _companiaRepository.CrearNoticiaAsync(usuarioLogueado, request);
                AgregarArchivosPresignados(respuesta, request.Archivos);
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> EditarNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaEditar request)
        {
            try
            {
                await PrepararArchivosNoticiaAsync(usuarioLogueado, request.IdCompania, request.Archivos);
                var respuesta = await _companiaRepository.EditarNoticiaAsync(usuarioLogueado, request);
                AgregarArchivosPresignados(respuesta, request.Archivos);
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaObtenerRequest request)
        {
            try
            {
                return await _companiaRepository.ObtenerNoticiaAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaConsulta>() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaArchivoIdRequest request)
        {
            try
            {
                var respuesta = await _companiaRepository.ObtenerNoticiaArchivoAsync(usuarioLogueado, request.IdCompaniaNoticiaArchivo);
                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<CompaniaNoticiaArchivoDescargaConsulta> archivos && archivos.Count > 0)
                {
                    var archivo = archivos[0];
                    if (!string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                        archivo.DownloadUrl = _companiaNoticiaStorage.GenerarDownloadUrl(archivo.ArchivoUrl, archivo.NombreDocumento ?? archivo.ArchivoUrl);
                    archivo.ArchivoUrl = string.Empty;
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaArchivoDescargaConsulta>() };
            }
        }

        public async Task<Respuesta> EliminarNoticiaArchivoAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaArchivoIdRequest request)
        {
            try
            {
                var respuesta = await _companiaRepository.EliminarNoticiaArchivoAsync(usuarioLogueado, request.IdCompaniaNoticiaArchivo);
                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<CompaniaNoticiaArchivoEliminado> archivos && archivos.Count > 0)
                {
                    var archivo = archivos[0];
                    if (!string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                    {
                        try
                        {
                            await _companiaNoticiaStorage.DeleteFileAsync(archivo.ArchivoUrl);
                        }
                        catch
                        {
                        }
                    }
                    archivo.ArchivoUrl = string.Empty;
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaArchivoEliminado>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticia filtro)
        {
            try
            {
                return await _companiaRepository.ListarNoticiasAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaNoticiaListaResult() };
            }
        }

        public async Task<Respuesta> EliminarNoticiaAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticia)
        {
            try
            {
                var obtener = await _companiaRepository.ObtenerNoticiaAsync(usuarioLogueado, new CompaniaNoticiaObtenerRequest
                {
                    IdCompaniaNoticia = idCompaniaNoticia
                });

                var respuesta = await _companiaRepository.EliminarNoticiaAsync(usuarioLogueado, idCompaniaNoticia);

                if (respuesta.IdTipoMensaje == 2)
                    await EliminarArchivosS3Async(obtener);

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaEliminada>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasBalanceAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaBalance filtro)
        {
            try
            {
                return await _companiaRepository.ListarNoticiasBalanceAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaNoticiaBalanceListaResult() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaBalanceAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaBalanceObtenerRequest request)
        {
            try
            {
                return await _companiaRepository.ObtenerNoticiaBalanceAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<CompaniaNoticiaBalanceConsulta>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro)
        {
            try
            {
                return await _companiaRepository.ListarNoticiasDetalleAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new CompaniaNoticiaDetalleListaResult() };
            }
        }

        public async Task<Respuesta> ExportarNoticiasDetalleAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaDetalle filtro)
        {
            try
            {
                var respuesta = await _companiaRepository.ExportarNoticiasDetalleAsync(usuarioLogueado, filtro);
                if (respuesta.IdTipoMensaje != 2)
                    return respuesta;

                var items = respuesta.Result as List<CompaniaNoticiaDetalleListaConsulta> ?? new();
                var archivo = _noticiasDetalleExcelExporter.GenerarExcelNoticiasDetalle(items);

                respuesta.Result = new CompaniaNoticiaDetalleExportacion
                {
                    NombreArchivo = $"companias-noticia-detalle-{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Archivo = archivo
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = null! };
            }
        }

        private async Task PrepararArchivosNoticiaAsync(UsuarioGeneral usuarioLogueado, int idCompania, List<CompaniaNoticiaArchivoItem>? archivos)
        {
            if (archivos == null)
                return;

            foreach (var archivo in archivos)
            {
                if ((archivo.IdCompaniaNoticiaArchivo ?? 0) > 0)
                    continue;

                if (string.IsNullOrWhiteSpace(archivo.NombreArchivo) && string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                    continue;

                var rutaArchivo = string.IsNullOrWhiteSpace(archivo.NombreArchivo)
                    ? archivo.ArchivoUrl!
                    : GenerarRutaCompaniaNoticiaArchivo(idCompania, archivo.NombreArchivo);
                var formatoArchivo = string.IsNullOrWhiteSpace(archivo.FormatoArchivo)
                    ? "application/octet-stream"
                    : archivo.FormatoArchivo;
                var nombreDocumento = string.IsNullOrWhiteSpace(archivo.NombreDocumento)
                    ? archivo.NombreArchivo
                    : archivo.NombreDocumento;

                archivo.ArchivoUrl = rutaArchivo;
                archivo.NombreDocumento = nombreDocumento;
                archivo.UploadUrl = _companiaNoticiaStorage.GenerarUploadUrl(rutaArchivo, formatoArchivo);
            }
        }

        private void AgregarArchivosPresignados(Respuesta respuesta, List<CompaniaNoticiaArchivoItem>? archivos)
        {
            if (archivos == null || respuesta.IdTipoMensaje != 2 || respuesta.Result is not List<CompaniaNoticiaCreada> noticias || noticias.Count == 0)
                return;

            noticias[0].Archivos = archivos
                .Where(a => !string.IsNullOrWhiteSpace(a.UploadUrl))
                .ToList();
        }

        // TODO: EliminarNoticiaAsync no longer deletes S3 files here — CompaniaNoticia_Obtener
        // stopped returning ArchivoUrl. Pending replacement of the delete flow.
        private async Task EliminarArchivosS3Async(Respuesta obtener)
        {
            await Task.CompletedTask;
        }

        private static string GenerarRutaCompaniaNoticiaArchivo(int idCompania, string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo);
            var nombreBase = Path.GetFileNameWithoutExtension(nombreArchivo);
            var nombreLimpio = string.Concat(nombreBase.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));

            if (string.IsNullOrWhiteSpace(nombreLimpio))
                nombreLimpio = "archivo";

            return $"companias/{idCompania}/noticias/adjuntos/{nombreLimpio}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        }

    }
}
