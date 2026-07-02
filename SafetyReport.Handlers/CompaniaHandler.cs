using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class CompaniaHandler
    {
        private readonly CompaniaDAO _dao;
        private readonly IS3UploadService _s3UploadService;

        public CompaniaHandler(CompaniaDAO dao, IS3UploadService s3UploadService)
        {
            _dao = dao;
            _s3UploadService = s3UploadService;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<CompaniaCrear> lstCompanias)
        {
            try
            {
                return await _dao.CrearAsync(usuarioLogueado, lstCompanias);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, CompaniaEditar request)
        {
            try
            {
                return await _dao.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, CompaniaObtenerRequest request)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroCompania filtro)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new CompaniaListaResult() };
            }
        }

        public async Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<CompaniaMatchItem> lista)
        {
            try
            {
                return await _dao.ListarMatchAsync(usuarioLogueado, lista);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaMatchResultItem>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idCompania)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, idCompania);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaEliminada>() };
            }
        }

        public async Task<Respuesta> CrearNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaCrear request)
        {
            try
            {
                PrepararArchivosNoticia(request.IdCompania, request.Archivos);
                var respuesta = await _dao.CrearNoticiaAsync(usuarioLogueado, request);
                AgregarArchivosPresignados(respuesta, request.Archivos);
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> EditarNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaEditar request)
        {
            try
            {
                PrepararArchivosNoticia(request.IdCompania, request.Archivos);
                var respuesta = await _dao.EditarNoticiaAsync(usuarioLogueado, request);
                AgregarArchivosPresignados(respuesta, request.Archivos);
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaCreada>() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaObtenerRequest request)
        {
            try
            {
                var respuesta = await _dao.ObtenerNoticiaAsync(usuarioLogueado, request);
                AgregarUrlsDescarga(respuesta);
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaConsulta>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticia filtro)
        {
            try
            {
                var respuesta = await _dao.ListarNoticiasAsync(usuarioLogueado, filtro);
                AgregarUrlsDescarga(respuesta);
                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new CompaniaNoticiaListaResult() };
            }
        }

        public async Task<Respuesta> EliminarNoticiaAsync(UsuarioGeneral usuarioLogueado, int idCompaniaNoticia)
        {
            try
            {
                var obtener = await _dao.ObtenerNoticiaAsync(usuarioLogueado, new CompaniaNoticiaObtenerRequest
                {
                    IdCompaniaNoticia = idCompaniaNoticia
                });

                var respuesta = await _dao.EliminarNoticiaAsync(usuarioLogueado, idCompaniaNoticia);

                if (respuesta.IdTipoMensaje == 2)
                    await EliminarArchivosS3Async(obtener);

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaEliminada>() };
            }
        }

        public async Task<Respuesta> ListarNoticiasBalanceAsync(UsuarioGeneral usuarioLogueado, FiltroCompaniaNoticiaBalance filtro)
        {
            try
            {
                return await _dao.ListarNoticiasBalanceAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new CompaniaNoticiaBalanceListaResult() };
            }
        }

        public async Task<Respuesta> ObtenerNoticiaBalanceAsync(UsuarioGeneral usuarioLogueado, CompaniaNoticiaBalanceObtenerRequest request)
        {
            try
            {
                return await _dao.ObtenerNoticiaBalanceAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<CompaniaNoticiaBalanceConsulta>() };
            }
        }

        private void PrepararArchivosNoticia(int idCompania, List<CompaniaNoticiaArchivoItem>? archivos)
        {
            if (archivos == null)
                return;

            foreach (var archivo in archivos)
            {
                if (!string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                {
                    if (!string.IsNullOrWhiteSpace(archivo.FormatoArchivo))
                        archivo.UploadUrl = _s3UploadService.GenerarUploadUrl(archivo.ArchivoUrl, archivo.FormatoArchivo);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(archivo.NombreArchivo))
                    continue;

                var rutaArchivo = GenerarRutaCompaniaNoticiaArchivo(idCompania, archivo.NombreArchivo);
                var formatoArchivo = string.IsNullOrWhiteSpace(archivo.FormatoArchivo)
                    ? "application/octet-stream"
                    : archivo.FormatoArchivo;

                archivo.ArchivoUrl = rutaArchivo;
                archivo.UploadUrl = _s3UploadService.GenerarUploadUrl(rutaArchivo, formatoArchivo);
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

        private void AgregarUrlsDescarga(Respuesta respuesta)
        {
            if (respuesta.IdTipoMensaje != 2)
                return;

            if (respuesta.Result is List<CompaniaNoticiaConsulta> noticias)
            {
                foreach (var noticia in noticias)
                    AgregarUrlsDescarga(noticia.Archivos);
            }
        }

        private void AgregarUrlsDescarga(List<CompaniaNoticiaArchivoConsulta> archivos)
        {
            foreach (var archivo in archivos)
            {
                if (!string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                    archivo.DownloadUrl = _s3UploadService.GenerarDownloadUrl(archivo.ArchivoUrl);
            }
        }

        private async Task EliminarArchivosS3Async(Respuesta obtener)
        {
            if (obtener.IdTipoMensaje != 2 || obtener.Result is not List<CompaniaNoticiaConsulta> noticias)
                return;

            foreach (var archivo in noticias.SelectMany(n => n.Archivos))
            {
                if (string.IsNullOrWhiteSpace(archivo.ArchivoUrl))
                    continue;

                try
                {
                    await _s3UploadService.DeleteFileAsync(archivo.ArchivoUrl);
                }
                catch
                {
                }
            }
        }

        private static string GenerarRutaCompaniaNoticiaArchivo(int idCompania, string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo);
            var nombreBase = Path.GetFileNameWithoutExtension(nombreArchivo);
            var nombreLimpio = string.Concat(nombreBase.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));

            if (string.IsNullOrWhiteSpace(nombreLimpio))
                nombreLimpio = "archivo";

            return $"companias/{idCompania}/noticias/{nombreLimpio}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
        }
    }
}
