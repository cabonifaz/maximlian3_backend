using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SafetyReport.DAO;
using SafetyReport.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SafetyReport.Handlers
{
    public class InformeHandler
    {
        private readonly InformeDAO _dao;
        private readonly IS3UploadService _s3;
        private readonly N8nService _n8n;
        private readonly N8nConfig _n8nConfig;
        private readonly int _s3ExpirationMinutes;

        public InformeHandler(InformeDAO dao, IS3UploadService s3, N8nService n8n, N8nConfig n8nConfig, IConfiguration configuration)
        {
            _dao = dao;
            _s3 = s3;
            _n8n = n8n;
            _n8nConfig = n8nConfig;
            _s3ExpirationMinutes = int.TryParse(configuration["AWS:S3ExpirationTime"], out var exp) ? exp : 15;
        }

        private static readonly HashSet<string> _extensionesImagenPermitidas =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff" };

        public async Task<Respuesta> InsertarAsync(UsuarioGeneral usuarioLogueado, InformeCrear request)
        {
            try
            {
                var error = AsignarRutasLocalImagenes(request.lstLocales, request.IdPedido ?? 0);
                if (error != null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = error, Result = new List<InformeCreado>() };

                var (respuesta, imagenes) = await _dao.InsertarAsync(usuarioLogueado, request);
                AgregarUrlsPrefirmadas(respuesta, imagenes);
                return respuesta;
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeCreado>() };
            }
        }

        public async Task<Respuesta> ActualizarAsync(UsuarioGeneral usuarioLogueado, InformeEditar request)
        {
            try
            {
                var error = AsignarRutasLocalImagenes(request.lstLocales, request.IdInforme);
                if (error != null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = error, Result = new List<InformeCreado>() };

                var (respuesta, imagenes) = await _dao.ActualizarAsync(usuarioLogueado, request);
                AgregarUrlsPrefirmadas(respuesta, imagenes);
                return respuesta;
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeCreado>() };
            }
        }

        private void AgregarUrlsPrefirmadas(Respuesta respuesta, List<InformeLocalImagenPendiente> imagenes)
        {
            if (respuesta.IdTipoMensaje != 2 || imagenes.Count == 0) return;

            var sufijos = imagenes.Select(i => i.Nombre).ToList();
            var extensionMime = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" }, { ".jpeg", "image/jpeg" }, { ".png", "image/png" },
                { ".gif", "image/gif" }, { ".webp", "image/webp" }, { ".bmp", "image/bmp" }, { ".tiff", "image/tiff" }
            };

            if (respuesta.Result is List<InformeCreado> creados && creados.Count > 0)
            {
                var creado = creados[0];
                creado.ImagenesPendientes = imagenes.Select(img =>
                {
                    var ext = Path.GetExtension(img.Nombre);
                    var mime = extensionMime.GetValueOrDefault(ext, "application/octet-stream");
                    img.UploadUrl = _s3.GenerarUploadUrl(img.S3Key, mime);
                    return img;
                }).ToList();
            }
        }

        private static string? AsignarRutasLocalImagenes(List<InformeLocalItem> locales, int id)
        {
            foreach (var local in locales)
                foreach (var imagen in local.Imagenes)
                    if (imagen.IdInformeLocalImagen is null or 0)
                    {
                        var ext = Path.GetExtension(imagen.Nombre);
                        if (!_extensionesImagenPermitidas.Contains(ext))
                            return $"El archivo '{imagen.Nombre}' no es una imagen válida. Extensiones permitidas: {string.Join(", ", _extensionesImagenPermitidas)}.";
                        var nombre = Path.GetFileNameWithoutExtension(imagen.Nombre);
                        imagen.ImagenURL = $"informes/pedido-{id}/locales/{nombre}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{ext}";
                    }
            return null;
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

        public Respuesta GenerarUrlsArchivoAsync(InformeArchivoUrlRequest request)
        {
            try
            {
                var pendientes = new List<InformeArchivoPendiente>();
                foreach (var nombre in request.Nombres)
                {
                    var ext = Path.GetExtension(nombre);
                    var nombreSinExt = Path.GetFileNameWithoutExtension(nombre);
                    var s3Key = $"informes/pedido-{request.IdPedido}/adjunto/{nombreSinExt}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{ext}";
                    pendientes.Add(new InformeArchivoPendiente
                    {
                        Nombre = nombre,
                        ArchivoUrl = s3Key,
                        UploadUrl = _s3.GenerarUploadUrl(s3Key, "application/octet-stream")
                    });
                }

                return new Respuesta { IdTipoMensaje = 2, Mensaje = "URLs generadas correctamente.", Result = pendientes };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeArchivoPendiente>() };
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

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroInformeObtener request)
        {
            try
            {
                var respuesta = await _dao.ObtenerAsync(usuarioLogueado, request.IdPedido);

                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<InformeConsulta> informes)
                {
                    var imagenes = informes
                        .SelectMany(i => i.Locales)
                        .SelectMany(l => l.Imagenes)
                        .Where(img => !string.IsNullOrWhiteSpace(img.ImagenURL))
                        .ToList();

                    if (imagenes.Count > 0)
                    {
                        var urls = _s3.GenerarDownloadUrlsBatch(imagenes.Select(img => img.ImagenURL).ToList());
                        for (int i = 0; i < imagenes.Count; i++)
                            imagenes[i].ImagenURL = urls[i];
                    }
                }

                return respuesta;
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroInforme request)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new InformeListaResult() };
            }
        }

        public async Task<Respuesta> CalcularBalanceDesagregadoAsync(UsuarioGeneral usuarioLogueado, InformeBalanceDesagregadoCalcularRequest request)
        {
            try
            {
                return await _dao.CalcularBalanceDesagregadoAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeBalanceDesagregadoCalculado>() };
            }
        }

        public async Task<Respuesta> CalcularBalanceSeguroAsync(UsuarioGeneral usuarioLogueado, InformeBalanceSeguroCalcularRequest request)
        {
            try
            {
                return await _dao.CalcularBalanceSeguroAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeBalanceSeguroCalculado>() };
            }
        }

        public async Task<Respuesta> CalcularBalanceBancoAsync(UsuarioGeneral usuarioLogueado, InformeBalanceBancoCalcularRequest request)
        {
            try
            {
                return await _dao.CalcularBalanceBancoAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeBalanceBancoCalculado>() };
            }
        }

        public async Task<Respuesta> CalcularBalanceTotalizadoAsync(UsuarioGeneral usuarioLogueado, InformeBalanceTotalizadoCalcularRequest request)
        {
            try
            {
                return await _dao.CalcularBalanceTotalizadoAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeBalanceTotalizadoCalculado>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, InformeIdRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request.IdInforme);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeEliminado>() };
            }
        }

        public async Task<Respuesta> AutocompletarAsync(InformeAutocompletar request)
        {
            try
            {
                var payload = new
                {
                    fileKey = request.FileKey,
                    mimeType = request.MimeType,
                    secciones = request.Secciones,
                    prompt = request.Prompt ?? string.Empty
                };

                var n8nRespuesta = await _n8n.PostAsync(_n8nConfig.WebhookObtenerCampos, payload);

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Autocompletado completado.",
                    Result = JsonSerializer.Deserialize<object>(n8nRespuesta, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null };
            }
        }

        public Task<Respuesta> ObtenerUrlPrefirmadaAsync(InformeUrlPrefirmadaRequest request)
        {
            try
            {
                var extension = Path.GetExtension(request.FileName);
                var fileKey = $"autocompletado/{Guid.NewGuid()}{extension}";
                var uploadUrl = _s3.GenerarUploadUrl(fileKey, request.MimeType);

                return Task.FromResult(new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "URL prefirmada generada correctamente.",
                    Result = new InformeUrlPrefirmada
                    {
                        UploadUrl = uploadUrl,
                        FileKey = fileKey,
                        ExpiresIn = _s3ExpirationMinutes
                    }
                });
            }
            catch (Exception)
            {
                return Task.FromResult(new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null });
            }
        }

        public async Task<Respuesta> ExtraerDocumentoAsync(IFormFile archivo, string secciones, string? prompt)
        {
            try
            {
                if (archivo is null || archivo.Length == 0)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "El archivo es requerido.", Result = null };

                JsonNode? seccionesJson;
                try { seccionesJson = JsonNode.Parse(secciones); }
                catch { return new Respuesta { IdTipoMensaje = 1, Mensaje = "El campo Secciones no es un JSON válido.", Result = null }; }

                var extension = Path.GetExtension(archivo.FileName);
                var fileKey = $"autocompletado/{Guid.NewGuid()}{extension}";
                await _s3.UploadFileAsync(fileKey, archivo);

                var payload = new { fileKey, mimeType = archivo.ContentType, secciones = seccionesJson, prompt = prompt ?? string.Empty };
                var n8nRespuesta = await _n8n.PostAsync(_n8nConfig.WebhookObtenerCampos, payload);

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Extracción completada.",
                    Result = JsonSerializer.Deserialize<object>(n8nRespuesta, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null };
            }
        }
    }
}
