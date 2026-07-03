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
        private readonly InformeLocalImagenDAO _localImagenDAO;
        private readonly PedidoDAO _pedidoDAO;
        private readonly PlantillaDocumentoDAO _plantillaDAO;
        private readonly IS3UploadService _s3;
        private readonly N8nService _n8n;
        private readonly N8nConfig _n8nConfig;
        private readonly int _s3ExpirationMinutes;

        public InformeHandler(InformeDAO dao, InformeLocalImagenDAO localImagenDAO, PedidoDAO pedidoDAO, PlantillaDocumentoDAO plantillaDAO, IS3UploadService s3, N8nService n8n, N8nConfig n8nConfig, IConfiguration configuration)
        {
            _dao = dao;
            _localImagenDAO = localImagenDAO;
            _pedidoDAO = pedidoDAO;
            _plantillaDAO = plantillaDAO;
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
                var rutasAnteriores = await ObtenerRutasImagenesExistentesAsync(usuarioLogueado, request.lstLocales);

                var error = ValidarExtensionesImagenes(request.lstLocales);
                if (error != null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = error, Result = new List<InformeCreado>() };

                var (respuesta, imagenes) = await _dao.InsertarAsync(usuarioLogueado, request);

                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<InformeCreado> creados && creados.Count > 0)
                    await ProcesarImagenesPostInsercionAsync(usuarioLogueado, imagenes, rutasAnteriores, request.lstLocales, request.IdPedido ?? 0, creados[0].IdInforme);

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
                var error = AsignarRutasLocalImagenes(request.lstLocales, request.IdPedido ?? 0, request.IdInforme);
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

        private async Task<Dictionary<int, string>> ObtenerRutasImagenesExistentesAsync(UsuarioGeneral u, List<InformeLocalItem> locales)
        {
            var idsExistentes = locales.SelectMany(l => l.Imagenes)
                .Where(i => i.IdInformeLocalImagen is not null and not 0)
                .Select(i => i.IdInformeLocalImagen!.Value)
                .ToList();

            if (idsExistentes.Count == 0)
                return new Dictionary<int, string>();

            var respuesta = await _localImagenDAO.ObtenerUrlsImagenesAsync(u, idsExistentes);

            if (respuesta.Result is List<InformeLocalImagenUrl> urls)
                return urls
                    .Where(x => !string.IsNullOrWhiteSpace(x.ImagenURL))
                    .ToDictionary(x => x.IdInformeLocalImagen, x => x.ImagenURL);

            return new Dictionary<int, string>();
        }

        private async Task ProcesarImagenesPostInsercionAsync(UsuarioGeneral u, List<InformeLocalImagenPendiente> imagenes, Dictionary<int, string> rutasAnteriores, List<InformeLocalItem> locales, int idPedido, int idInforme)
        {
            var imagenesRequest = locales.SelectMany(l => l.Imagenes).ToList();

            for (int i = 0; i < imagenes.Count; i++)
            {
                var imagen = imagenes[i];
                var ext = Path.GetExtension(imagen.Nombre);
                var nombre = Path.GetFileNameWithoutExtension(imagen.Nombre);
                var rutaDestino = $"informes/pedido-{idPedido}/informe-{idInforme}/locales/{nombre}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{ext}";

                if (i < imagenesRequest.Count
                    && imagenesRequest[i].IdInformeLocalImagen is not null and not 0
                    && rutasAnteriores.TryGetValue(imagenesRequest[i].IdInformeLocalImagen!.Value, out var rutaOrigen))
                    await _s3.CopiarArchivoAsync(rutaOrigen, rutaDestino);

                await _localImagenDAO.ActualizarImagenUrlAsync(u, imagen.IdInformeLocalImagen, rutaDestino);
                imagen.S3Key = rutaDestino;
            }
        }

        private static string? ValidarExtensionesImagenes(List<InformeLocalItem> locales)
        {
            foreach (var local in locales)
                foreach (var imagen in local.Imagenes)
                    if (imagen.IdInformeLocalImagen is null or 0)
                    {
                        var ext = Path.GetExtension(imagen.Nombre);
                        if (!_extensionesImagenPermitidas.Contains(ext))
                            return $"El archivo '{imagen.Nombre}' no es una imagen válida. Extensiones permitidas: {string.Join(", ", _extensionesImagenPermitidas)}.";
                    }
            return null;
        }

        private static string? AsignarRutasLocalImagenes(List<InformeLocalItem> locales, int idPedido, int idInforme)
        {
            foreach (var local in locales)
                foreach (var imagen in local.Imagenes)
                    if (imagen.IdInformeLocalImagen is null or 0)
                    {
                        var ext = Path.GetExtension(imagen.Nombre);
                        if (!_extensionesImagenPermitidas.Contains(ext))
                            return $"El archivo '{imagen.Nombre}' no es una imagen válida. Extensiones permitidas: {string.Join(", ", _extensionesImagenPermitidas)}.";
                        var nombre = Path.GetFileNameWithoutExtension(imagen.Nombre);
                        imagen.ImagenURL = $"informes/pedido-{idPedido}/informe-{idInforme}/locales/{nombre}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{ext}";
                    }
            return null;
        }


        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroInformeObtener request)
        {
            try
            {
                if (request.IdInforme <= 0)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "El IdInforme es obligatorio.",
                        Result = new List<InformeConsulta>()
                    };
                }

                var respuesta = await _dao.ObtenerAsync(usuarioLogueado, request.IdPedido, request.IdInforme);

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

        public async Task<Respuesta> CalcularBalanceTurquiaAsync(UsuarioGeneral usuarioLogueado, InformeBalanceTurquiaCalcularRequest request)
        {
            try
            {
                return await _dao.CalcularBalanceTurquiaAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeBalanceTurquiaCalculado>() };
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

        public async Task<Respuesta> ActualizarEstadoAsync(UsuarioGeneral usuarioLogueado, InformeActualizarEstadoRequest request)
        {
            try
            {
                return await _dao.ActualizarEstadoAsync(usuarioLogueado, request.IdInforme, request.IdEstadoInforme);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }

        public async Task<Respuesta> ObtenerDocumentoAsync(UsuarioGeneral usuarioLogueado, FiltroGenerarDocumento request)
        {
            try
            {
                var formato = string.IsNullOrWhiteSpace(request.Formato) ? ".pdf" : request.Formato;

                var respuestaRuta = await _dao.ObtenerRutaDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido);

                string? ruta = null;
                JsonArray? formatos = null;

                if (respuestaRuta.IdTipoMensaje == 2 && respuestaRuta.Result is string urlDocumento && !string.IsNullOrWhiteSpace(urlDocumento))
                {
                    try
                    {
                        var docJson = JsonNode.Parse(urlDocumento);
                        ruta = docJson?["ruta"]?.GetValue<string>();
                        formatos = docJson?["formatos"]?.AsArray();
                    }
                    catch { }
                }

                var tieneFormato = formatos?.Any(f => f?.GetValue<string>() == formato) == true;

                if (ruta == null || formatos == null || formatos.Count == 0 || !tieneFormato)
                {
                    var generado = formato == ".docx"
                        ? await GenerarDocumentoDocxAsync(usuarioLogueado, request)
                        : await GenerarDocumentoPdfAsync(usuarioLogueado, request);
                    if (generado.IdTipoMensaje != 2) return generado;

                    respuestaRuta = await _dao.ObtenerRutaDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido);
                    if (respuestaRuta.IdTipoMensaje != 2 || respuestaRuta.Result is not string urlGenerado || string.IsNullOrWhiteSpace(urlGenerado))
                        return new Respuesta { IdTipoMensaje = 1, Mensaje = "Error al obtener el documento generado.", Result = null };

                    var docJson = JsonNode.Parse(urlGenerado);
                    ruta = docJson?["ruta"]?.GetValue<string>();
                }

                var nombreBase = ruta!.Split('/').Last();
                var s3Key = ruta + formato;
                var nombreDescarga = nombreBase + formato;
                var downloadUrl = _s3.GenerarDownloadUrl(s3Key, nombreDescarga);

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Documento obtenido correctamente.",
                    Result = new { url = downloadUrl, nombre = nombreDescarga }
                };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null };
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

        public async Task<Respuesta> PrevisualizarDocumentoAsync(UsuarioGeneral usuarioLogueado, FiltroGenerarDocumento request)
        {
            try
            {
                var (respuesta, nombreInforme) = await _dao.GenerarDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido);
                if (respuesta.IdTipoMensaje != 2 || respuesta.Result is not string jsonStr || string.IsNullOrWhiteSpace(jsonStr))
                    return new Respuesta { IdTipoMensaje = respuesta.IdTipoMensaje, Mensaje = respuesta.Mensaje, Result = null };

                var estructura = JsonNode.Parse(jsonStr);
                if (estructura is null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Error al procesar el documento.", Result = null };

                var assets = estructura?["assets"]?.AsObject();
                if (assets is not null)
                {
                    var resolved = assets
                        .Where(kv => !string.IsNullOrWhiteSpace(kv.Value?.GetValue<string>()))
                        .ToDictionary(kv => kv.Key, kv => _s3.GenerarDownloadUrl(kv.Value!.GetValue<string>()));

                    var json = estructura!.ToJsonString();
                    foreach (var (name, url) in resolved)
                        json = json.Replace($"{{{name}}}", url);

                    estructura = JsonNode.Parse(json);
                    estructura!.AsObject().Remove("assets");
                }

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje       = "Documento generado correctamente.",
                    Result        = new { documento = estructura, nombreInforme }
                };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null };
            }
        }

        public async Task<Respuesta> GenerarDocumentoDocxAsync(UsuarioGeneral usuarioLogueado, FiltroGenerarDocumento request)
        {
            try
            {
                var docExistente = await ObtenerDocumentoExistente(usuarioLogueado, request, ".docx");
                if (docExistente != null) return docExistente;

                var (respuesta, nombreInforme) = await _dao.GenerarDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido);
                if (respuesta.IdTipoMensaje != 2 || respuesta.Result is not string jsonStr || string.IsNullOrWhiteSpace(jsonStr))
                    return new Respuesta { IdTipoMensaje = respuesta.IdTipoMensaje, Mensaje = respuesta.Mensaje, Result = null };

                var estructura = JsonNode.Parse(jsonStr);
                if (estructura is null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Error al procesar el documento.", Result = null };

                await DescargarFuentesAsync(estructura!);
                var allAssets = await DescargarTodosAssetsAsync(estructura);

                var generador = new DocxGeneratorService();
                using var docxStream = generador.GenerarDocx(estructura!, allAssets);

                var nombreArchivo = !string.IsNullOrWhiteSpace(nombreInforme) ? nombreInforme : "documento";
                var rutaBase = $"informes/pedido-{request.IdPedido}/informe-{request.IdInforme}/{nombreArchivo}";
                await _s3.UploadStreamAsync(rutaBase + ".docx", docxStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

                await ActualizarDocumentoJsonAsync(usuarioLogueado, request, rutaBase, ".docx");

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Documento DOCX generado correctamente.",
                    Result = new { nombreInforme }
                };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null };
            }
        }

        public async Task<Respuesta> GenerarDocumentoPdfAsync(UsuarioGeneral usuarioLogueado, FiltroGenerarDocumento request)
        {
            try
            {
                var docExistente = await ObtenerDocumentoExistente(usuarioLogueado, request, ".pdf");
                if (docExistente != null) return docExistente;

                var (respuesta, nombreInforme) = await _dao.GenerarDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido);
                if (respuesta.IdTipoMensaje != 2 || respuesta.Result is not string jsonStr || string.IsNullOrWhiteSpace(jsonStr))
                    return new Respuesta { IdTipoMensaje = respuesta.IdTipoMensaje, Mensaje = respuesta.Mensaje, Result = null };

                var estructura = JsonNode.Parse(jsonStr);
                if (estructura is null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Error al procesar el documento.", Result = null };

                await DescargarFuentesAsync(estructura!);
                var allAssets = await DescargarTodosAssetsAsync(estructura);

                var generador = new PdfGeneratorService();
                using var pdfStream = generador.GenerarPdf(estructura!, allAssets);

                var nombreArchivo = !string.IsNullOrWhiteSpace(nombreInforme) ? nombreInforme : "documento";
                var rutaBase = $"informes/pedido-{request.IdPedido}/informe-{request.IdInforme}/{nombreArchivo}";
                await _s3.UploadStreamAsync(rutaBase + ".pdf", pdfStream, "application/pdf");

                await ActualizarDocumentoJsonAsync(usuarioLogueado, request, rutaBase, ".pdf");

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Documento PDF generado correctamente.",
                    Result = new { nombreInforme }
                };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null };
            }
        }

        private async Task<Dictionary<string, byte[]>> DescargarTodosAssetsAsync(JsonNode? estructura)
        {
            var result = new Dictionary<string, byte[]>();
            var assets = estructura?["assets"]?.AsObject();
            if (assets is null) return result;
            var seen = new Dictionary<string, byte[]>();
            using var http = new HttpClient();
            foreach (var kv in assets)
            {
                var s3Key = kv.Value?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(s3Key)) continue;
                if (!seen.TryGetValue(s3Key, out var bytes))
                {
                    try
                    {
                        bytes = await _s3.DescargarBytesAsync(s3Key);
                        if (bytes is null || bytes.Length == 0)
                        {
                            var url = _s3.GenerarDownloadUrl(s3Key);
                            bytes = await http.GetByteArrayAsync(url);
                        }
                        seen[s3Key] = bytes;
                    }
                    catch { continue; }
                }
                result[kv.Key] = bytes;
            }
            return result;
        }

        private async Task DescargarFuentesAsync(JsonNode estructura)
        {
            var fontFamily = estructura["document"]?["font"]?["family"]?.GetValue<string>() ?? "Calibri";
            var variantes = PdfGeneratorService.DetectarVariantesFuente(estructura);
            var rutasS3 = PdfGeneratorService.ObtenerRutasS3Fuentes(fontFamily, variantes);

            var fuentes = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            var tareas = rutasS3.Select(async kv =>
            {
                var bytes = await _s3.DescargarBytesAsync(kv.Value);
                return (Nombre: kv.Key, Bytes: bytes);
            });

            foreach (var resultado in await Task.WhenAll(tareas))
            {
                if (resultado.Bytes != null)
                    fuentes[resultado.Nombre] = resultado.Bytes;
            }

            if (fuentes.Count > 0)
                PdfGeneratorService.ConfigurarFuentes(fuentes);
        }

        private async Task<Respuesta?> ObtenerDocumentoExistente(UsuarioGeneral usuarioLogueado, FiltroGenerarDocumento request, string formato)
        {
            var respuestaDoc = await _dao.ObtenerRutaDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido);
            if (respuestaDoc.IdTipoMensaje != 2 || respuestaDoc.Result is not string urlDocumento || string.IsNullOrWhiteSpace(urlDocumento))
                return null;

            try
            {
                var docJson = JsonNode.Parse(urlDocumento);
                var ruta = docJson?["ruta"]?.GetValue<string>();
                var formatos = docJson?["formatos"]?.AsArray();
                if (ruta == null || formatos == null) return null;

                if (!formatos.Any(f => f?.GetValue<string>() == formato)) return null;

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = formato == ".pdf" ? "Documento PDF ya existe." : "Documento DOCX ya existe.",
                    Result = new { nombreInforme = ruta.Split('/').Last() }
                };
            }
            catch { return null; }
        }

        private async Task ActualizarDocumentoJsonAsync(UsuarioGeneral usuarioLogueado, FiltroGenerarDocumento request, string rutaBase, string formato)
        {
            var formatos = new JsonArray { JsonValue.Create(formato) };

            var respuestaDoc = await _dao.ObtenerRutaDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido);
            if (respuestaDoc.IdTipoMensaje == 2 && respuestaDoc.Result is string urlDocumento && !string.IsNullOrWhiteSpace(urlDocumento))
            {
                try
                {
                    var existente = JsonNode.Parse(urlDocumento);
                    var formatosExistentes = existente?["formatos"]?.AsArray();
                    if (formatosExistentes != null)
                    {
                        formatos = new JsonArray();
                        foreach (var f in formatosExistentes)
                        {
                            var val = f?.GetValue<string>();
                            if (val != null && val != formato)
                                formatos.Add(JsonValue.Create(val));
                        }
                        formatos.Add(JsonValue.Create(formato));
                    }
                }
                catch { }
            }

            var docJson = new JsonObject
            {
                ["ruta"] = rutaBase,
                ["formatos"] = formatos
            };

            await _dao.ActualizarDocumentoAsync(usuarioLogueado, request.IdInforme, docJson.ToJsonString());
        }

        private static string MapearPlantillaHtml(string html, JsonNode? informe, JsonNode? pedido)
        {
            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"\{\{#chunks\s+([^\s}]+)(?:\s+(\d+))?(?:\s+(\d+))?\}\}(.*?)\{\{/chunks\}\}",
                match =>
                {
                    var sourceKey = match.Groups[1].Value.Trim();
                    var maxCharacters = match.Groups[2].Success
                        ? int.Parse(match.Groups[2].Value)
                        : 2200;
                    var firstChunkSize = match.Groups[3].Success
                        ? int.Parse(match.Groups[3].Value)
                        : maxCharacters;
                    var template = match.Groups[4].Value;
                    var source = ResolverRuta(informe, sourceKey)
                        ?? ResolverRuta(pedido, sourceKey);
                    var text = source?.ToString() ?? string.Empty;

                    return string.Concat(SplitTextChunks(text, maxCharacters, firstChunkSize).Select((chunk, index) =>
                    {
                        var item = new JsonObject { ["chunk"] = chunk, ["chunkIndex"] = index };
                        return ReemplazarTexto(JsonValue.Create(template)!, informe, pedido, item).GetValue<string>();
                    }));
                },
                System.Text.RegularExpressions.RegexOptions.Singleline);

            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"\{\{#chunkFirst\s+([^\s}]+)(?:\s+(\d+))?\}\}(.*?)\{\{/chunkFirst\}\}",
                match =>
                {
                    var sourceKey = match.Groups[1].Value.Trim();
                    var firstChunkSize = match.Groups[2].Success
                        ? int.Parse(match.Groups[2].Value)
                        : 2200;
                    var template = match.Groups[3].Value;
                    var source = ResolverRuta(informe, sourceKey)
                        ?? ResolverRuta(pedido, sourceKey);
                    var text = source?.ToString() ?? string.Empty;

                    var firstChunk = SplitTextChunks(text, firstChunkSize).First();
                    var item = new JsonObject { ["chunk"] = firstChunk };
                    return ReemplazarTexto(JsonValue.Create(template)!, informe, pedido, item).GetValue<string>();
                },
                System.Text.RegularExpressions.RegexOptions.Singleline);

            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"\{\{#chunkRest\s+([^\s}]+)(?:\s+(\d+))?(?:\s+(\d+))?\}\}(.*?)\{\{/chunkRest\}\}",
                match =>
                {
                    var sourceKey = match.Groups[1].Value.Trim();
                    var maxCharacters = match.Groups[2].Success
                        ? int.Parse(match.Groups[2].Value)
                        : 2200;
                    var firstChunkSize = match.Groups[3].Success
                        ? int.Parse(match.Groups[3].Value)
                        : maxCharacters;
                    var template = match.Groups[4].Value;
                    var source = ResolverRuta(informe, sourceKey)
                        ?? ResolverRuta(pedido, sourceKey);
                    var text = source?.ToString() ?? string.Empty;

                    var allChunks = SplitTextChunks(text, maxCharacters, firstChunkSize).ToList();
                    if (allChunks.Count <= 1) return string.Empty;

                    return string.Concat(allChunks.Skip(1).Select((chunk, index) =>
                    {
                        var item = new JsonObject { ["chunk"] = chunk, ["chunkIndex"] = index + 1 };
                        return ReemplazarTexto(JsonValue.Create(template)!, informe, pedido, item).GetValue<string>();
                    }));
                },
                System.Text.RegularExpressions.RegexOptions.Singleline);

            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"\{\{#each\s+([^}]+)\}\}(.*?)\{\{/each\}\}",
                match =>
                {
                    var sourceKey = match.Groups[1].Value.Trim();
                    var template = match.Groups[2].Value;
                    var source = ResolverRuta(informe, sourceKey) as JsonArray
                        ?? ResolverRuta(pedido, sourceKey) as JsonArray
                        ?? new JsonArray();

                    return string.Concat(source.Select(item =>
                        ReemplazarTexto(JsonValue.Create(template)!, informe, pedido, item).GetValue<string>()));
                },
                System.Text.RegularExpressions.RegexOptions.Singleline);

            var htmlMapeado = ReemplazarTexto(JsonValue.Create(html)!, informe, pedido).GetValue<string>();
            var node = new JsonObject
            {
                ["html"] = htmlMapeado
            };
            return node.ToJsonString();
        }

        private static IEnumerable<string> SplitTextChunks(string text, int maxCharacters, int firstChunkSize = 0)
        {
            maxCharacters = Math.Max(maxCharacters, 500);
            if (firstChunkSize <= 0) firstChunkSize = maxCharacters;
            firstChunkSize = Math.Max(firstChunkSize, 200);
            text = text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return new[] { string.Empty };

            var chunks = new List<string>();
            var remaining = text;
            var currentMax = firstChunkSize;
            while (remaining.Length > currentMax)
            {
                var splitAt = remaining.LastIndexOf('\n', currentMax);
                if (splitAt < currentMax / 2)
                    splitAt = remaining.LastIndexOf(". ", currentMax, StringComparison.Ordinal);
                if (splitAt < currentMax / 2)
                    splitAt = remaining.LastIndexOf(' ', currentMax);
                if (splitAt < currentMax / 2)
                    splitAt = currentMax;

                chunks.Add(remaining[..splitAt].Trim());
                remaining = remaining[splitAt..].Trim();
                currentMax = maxCharacters;
            }

            chunks.Add(remaining);
            return chunks;
        }

        private static JsonNode? ResolverRuta(JsonNode? source, string ruta)
        {
            if (source is null) return null;
            JsonNode? cursor = source;
            foreach (var parte in ruta.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                cursor = cursor?[parte];
            return cursor;
        }

        private static void MapearNodo(JsonNode node, JsonNode? informe, JsonNode? pedido)
        {
            switch (node)
            {
                case JsonObject obj:
                    var tipo = obj["type"]?.GetValue<string>();

                    if (tipo == "repeat" && obj["source"] is JsonValue repeatSrc)
                    {
                        var sourceKey = repeatSrc.GetValue<string>();
                        var array = ResolverRuta(informe, sourceKey) as JsonArray ?? new JsonArray();
                        var secciones = (obj["sections"] as JsonArray)?.ToList() ?? new List<JsonNode?>();
                        obj.Remove("sections");

                        var expanded = new JsonArray();
                        foreach (var item in array)
                        {
                            foreach (var seccion in secciones)
                            {
                                var seccionNode = JsonNode.Parse(seccion?.ToJsonString() ?? "{}")!;
                                MapearNodoConItem(seccionNode, informe, pedido, item);
                                expanded.Add(seccionNode);
                            }
                        }
                        obj["sections"] = expanded;
                        return;
                    }

                    if (tipo == "dataTable" && obj["source"] is JsonValue tableSrc)
                    {
                        var sourceKey = tableSrc.GetValue<string>();
                        var array = ResolverRuta(informe, sourceKey) as JsonArray ?? new JsonArray();
                        var columnas = (obj["columns"] as JsonArray)?.ToList() ?? new List<JsonNode?>();

                        var rows = new JsonArray();
                        foreach (var item in array)
                        {
                            var row = new JsonArray();
                            foreach (var col in columnas)
                            {
                                var fieldTemplate = col?["field"]?.GetValue<string>() ?? "";
                                var resolved = ReemplazarTexto(JsonValue.Create(fieldTemplate)!, informe, pedido, item).GetValue<string>();
                                row.Add(JsonValue.Create(resolved));
                            }
                            rows.Add(row);
                        }
                        obj["rows"] = rows;
                        return;
                    }

                    if (tipo == "repeatDetail" && obj["source"] is JsonValue detailSrc)
                    {
                        var sourceKey = detailSrc.GetValue<string>();
                        var array = ResolverRuta(informe, sourceKey) as JsonArray ?? new JsonArray();
                        var titleField = obj["titleField"]?.GetValue<string>() ?? "";
                        var contentField = obj["contentField"]?.GetValue<string>() ?? "";

                        var items = new JsonArray();
                        foreach (var item in array)
                        {
                            var titulo = ReemplazarTexto(JsonValue.Create(titleField)!, informe, pedido, item).GetValue<string>();
                            var contenido = ReemplazarTexto(JsonValue.Create(contentField)!, informe, pedido, item).GetValue<string>();
                            items.Add(new JsonObject { ["title"] = titulo, ["content"] = contenido });
                        }
                        obj["items"] = items;
                        return;
                    }

                    if (tipo == "each" && obj["source"] is JsonValue eachSrc)
                    {
                        var sourceKey = eachSrc.GetValue<string>();
                        var array = ResolverRuta(informe, sourceKey) as JsonArray ?? new JsonArray();
                        var bloques = (obj["blocks"] as JsonArray)?.ToList() ?? new List<JsonNode?>();
                        obj.Remove("blocks");

                        var expanded = new JsonArray();
                        foreach (var item in array)
                        {
                            foreach (var bloque in bloques)
                            {
                                var bloqueNode = JsonNode.Parse(bloque?.ToJsonString() ?? "{}")!;
                                MapearNodoConItem(bloqueNode, informe, pedido, item);
                                expanded.Add(bloqueNode);
                            }
                        }
                        obj["blocks"] = expanded;
                        return;
                    }

                    if (obj["each"] is JsonValue eachVal && obj["rowTemplate"] is JsonArray rowTemplate)
                    {
                        var sourceKey = eachVal.GetValue<string>();
                        var array = ResolverRuta(informe, sourceKey) as JsonArray ?? new JsonArray();
                        var rows = new JsonArray();

                        foreach (var item in array)
                        {
                            var condition = obj["condition"]?.GetValue<string>();
                            if (condition != null && !(item?[condition]?.GetValue<bool>() ?? false))
                                continue;

                            var rowNode = JsonNode.Parse(rowTemplate.ToJsonString())!;
                            MapearNodoConItem(rowNode, informe, pedido, item);
                            rows.Add(rowNode);
                        }

                        obj.Remove("each");
                        obj.Remove("rowTemplate");
                        obj.Remove("condition");
                        obj["rows"] = rows;
                        return;
                    }

                    foreach (var key in obj.Select(kv => kv.Key).ToList())
                    {
                        if (obj[key] is JsonValue strVal && strVal.GetValueKind() == System.Text.Json.JsonValueKind.String)
                            obj[key] = ReemplazarTexto(strVal, informe, pedido);
                        else if (obj[key] is JsonNode child)
                            MapearNodo(child, informe, pedido);
                    }
                    break;

                case JsonArray arr:
                    for (int i = 0; i < arr.Count; i++)
                    {
                        if (arr[i] is JsonValue strVal && strVal.GetValueKind() == System.Text.Json.JsonValueKind.String)
                            arr[i] = ReemplazarTexto(strVal, informe, pedido);
                        else if (arr[i] is JsonNode child)
                            MapearNodo(child, informe, pedido);
                    }
                    break;
            }
        }

        private static void MapearNodoConItem(JsonNode node, JsonNode? informe, JsonNode? pedido, JsonNode? item)
        {
            switch (node)
            {
                case JsonObject obj:
                    foreach (var key in obj.Select(kv => kv.Key).ToList())
                    {
                        if (obj[key] is JsonValue strVal && strVal.GetValueKind() == System.Text.Json.JsonValueKind.String)
                            obj[key] = ReemplazarTexto(strVal, informe, pedido, item);
                        else if (obj[key] is JsonNode child)
                            MapearNodoConItem(child, informe, pedido, item);
                    }
                    break;

                case JsonArray arr:
                    for (int i = 0; i < arr.Count; i++)
                    {
                        if (arr[i] is JsonValue strVal && strVal.GetValueKind() == System.Text.Json.JsonValueKind.String)
                            arr[i] = ReemplazarTexto(strVal, informe, pedido, item);
                        else if (arr[i] is JsonNode child)
                            MapearNodoConItem(child, informe, pedido, item);
                    }
                    break;
            }
        }

        private static JsonNode ReemplazarTexto(JsonNode node, JsonNode? informe, JsonNode? pedido, JsonNode? item = null)
        {
            if (node is not JsonValue val || val.GetValueKind() != System.Text.Json.JsonValueKind.String)
                return node;

            var texto = val.GetValue<string>();
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\{\{([^}]+)\}\}", match =>
            {
                var campo = match.Groups[1].Value.Trim();

                if (campo.StartsWith("pedido."))
                    return ResolverCampo(pedido, campo["pedido.".Length..]) ?? match.Value;

                // Dot notation: CuentaBalance.Field → item["CuentaBalance"]["Field"]
                var partes = campo.Split('.');
                if (partes.Length > 1)
                {
                    JsonNode? cursor = item;
                    foreach (var parte in partes)
                        cursor = cursor?[parte];
                    return cursor?.ToString() ?? match.Value;
                }

                return ResolverCampo(item, campo)
                    ?? ResolverCampo(informe, campo)
                    ?? match.Value;
            });

            return JsonValue.Create(texto)!;
        }

        private static string? ResolverCampo(JsonNode? source, string campo)
        {
            if (source is null) return null;
            var valor = source[campo];
            return valor is null || valor.GetValueKind() == System.Text.Json.JsonValueKind.Null
                ? null
                : valor.ToString();
        }

    }
}
