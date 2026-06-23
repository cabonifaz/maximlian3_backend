using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeArchivoHandler
    {
        private readonly InformeArchivoDAO _dao;
        private readonly InformeDAO _informeDAO;
        private readonly IS3UploadService _s3;

        public InformeArchivoHandler(InformeArchivoDAO dao, InformeDAO informeDAO, IS3UploadService s3)
        {
            _dao = dao;
            _informeDAO = informeDAO;
            _s3 = s3;
        }

        public async Task<Respuesta> GenerarUrlsArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoUrlRequest request)
        {
            try
            {
                var idInforme = request.IdInforme;

                if (idInforme == 0)
                {
                    var resultado = await _informeDAO.ObtenerOCrearInformeAsync(usuarioLogueado, request.IdPedido);
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
                        UploadUrl = _s3.GenerarUploadUrl(s3Key, "application/octet-stream")
                    });
                }

                var result = new InformeArchivoUrlResult { IdInforme = idInforme, Archivos = pendientes };
                return new Respuesta { IdTipoMensaje = 2, Mensaje = "URLs generadas correctamente.", Result = result };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeArchivoUrlResult>() };
            }
        }

        public async Task<Respuesta> ObtenerArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoIdRequest request)
        {
            try
            {
                var respuesta = await _dao.ObtenerArchivoAsync(usuarioLogueado, request.IdInformeArchivo);
                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<InformeArchivoConsulta> archivos && archivos.Count > 0)
                {
                    var archivo = archivos[0];
                    archivo.DownloadUrl = _s3.GenerarDownloadUrl(archivo.ArchivoUrl);
                    archivo.ArchivoUrl = string.Empty;
                }
                return respuesta;
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeArchivoConsulta>() };
            }
        }

        public async Task<Respuesta> EliminarArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoIdRequest request)
        {
            try
            {
                var obtener = await _dao.ObtenerArchivoAsync(usuarioLogueado, request.IdInformeArchivo);
                if (obtener.IdTipoMensaje != 2)
                    return obtener;

                if (obtener.Result is List<InformeArchivoConsulta> archivos && archivos.Count > 0)
                    await _s3.DeleteFileAsync(archivos[0].ArchivoUrl);

                return await _dao.EliminarArchivoAsync(usuarioLogueado, request.IdInformeArchivo);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ActualizarArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoActualizarRequest request)
        {
            try
            {
                return await _dao.ActualizarArchivoAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }

        public async Task<Respuesta> InsertarArchivoLoteAsync(UsuarioGeneral usuarioLogueado, InformeArchivoInsertarRequest request)
        {
            try
            {
                return await _dao.InsertarArchivoLoteAsync(usuarioLogueado, request.IdInforme, request.IdPedido, request.Archivos);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }
    }
}
