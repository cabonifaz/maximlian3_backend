using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeLocalImagenHandler
    {
        private readonly InformeLocalImagenDAO _dao;
        private readonly IS3UploadService _s3;

        public InformeLocalImagenHandler(InformeLocalImagenDAO dao, IS3UploadService s3)
        {
            _dao = dao;
            _s3 = s3;
        }

        public async Task<Respuesta> ObtenerUrlsImagenesAsync(UsuarioGeneral usuarioLogueado, InformeLocalImagenEstadoCargaRequest request)
        {
            try
            {
                var respuesta = await _dao.ObtenerUrlsImagenesAsync(usuarioLogueado, request.Ids);

                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<InformeLocalImagenUrl> imagenes && imagenes.Count > 0)
                {
                    var urls = _s3.GenerarDownloadUrlsBatch(imagenes.Select(i => i.ImagenURL).ToList());
                    for (int i = 0; i < imagenes.Count; i++)
                    {
                        imagenes[i].DownloadUrl = urls[i];
                        imagenes[i].ImagenURL   = string.Empty;
                    }
                }

                return respuesta;
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeLocalImagenUrl>() };
            }
        }

        public async Task<Respuesta> ActualizarEstadoCargaAsync(UsuarioGeneral usuarioLogueado, InformeLocalImagenEstadoCargaRequest request)
        {
            try
            {
                return await _dao.ActualizarEstadoCargaAsync(usuarioLogueado, request.Ids);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }
    }
}
