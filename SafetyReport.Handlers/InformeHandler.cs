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
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() };
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
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() };
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
                        imagenes[i].S3Key      = imagenes[i].ImagenURL;
                        imagenes[i].DownloadUrl = urls[i];
                        imagenes[i].ImagenURL   = string.Empty;
                    }
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeLocalImagenUrl>() };
            }
        }

        public async Task<Respuesta> ActualizarEstadoCargaAsync(UsuarioGeneral usuarioLogueado, InformeLocalImagenEstadoCargaRequest request)
        {
            try
            {
                return await _dao.ActualizarEstadoCargaAsync(usuarioLogueado, request.Ids);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<object>() };
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
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroInforme request)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new InformeListaResult() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, InformeIdRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request.IdInforme);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeEliminado>() };
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
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = null };
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
            catch (Exception ex)
            {
                return Task.FromResult(new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = null });
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
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = null };
            }
        }
    }
}
