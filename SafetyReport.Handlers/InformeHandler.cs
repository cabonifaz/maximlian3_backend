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
        private readonly PedidoDAO _pedidoDAO;
        private readonly PlantillaDocumentoDAO _plantillaDAO;
        private readonly IS3UploadService _s3;
        private readonly N8nService _n8n;
        private readonly N8nConfig _n8nConfig;
        private readonly int _s3ExpirationMinutes;

        public InformeHandler(InformeDAO dao, PedidoDAO pedidoDAO, PlantillaDocumentoDAO plantillaDAO, IS3UploadService s3, N8nService n8n, N8nConfig n8nConfig, IConfiguration configuration)
        {
            _dao = dao;
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

        public async Task<Respuesta> GenerarUrlsArchivoAsync(UsuarioGeneral usuarioLogueado, InformeArchivoUrlRequest request)
        {
            try
            {
                var idInforme = request.IdInforme;

                if (idInforme == 0)
                {
                    var resultado = await _dao.ObtenerOCrearInformeAsync(usuarioLogueado, request.IdPedido);
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

        public async Task<Respuesta> GenerarDocumentoAsync(UsuarioGeneral usuarioLogueado, FiltroGenerarDocumento request)
        {
            try
            {
                var respuesta = await _dao.GenerarDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido, 1);
                if (respuesta.IdTipoMensaje != 2 || respuesta.Result is not string jsonStr || string.IsNullOrWhiteSpace(jsonStr))
                    return new Respuesta { IdTipoMensaje = respuesta.IdTipoMensaje, Mensaje = respuesta.Mensaje, Result = null };

                var estructura = JsonNode.Parse(jsonStr);
                if (estructura is null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Error al procesar el documento.", Result = null };

                var logoKey = estructura?["document"]?["header"]?["logo"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(logoKey))
                    estructura!["document"]!["header"]!["logo"] = _s3.GenerarDownloadUrl(logoKey);

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje       = "Documento generado correctamente.",
                    Result        = estructura
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
                var respuesta = await _dao.GenerarDocumentoAsync(usuarioLogueado, request.IdInforme, request.IdPedido, 1);
                if (respuesta.IdTipoMensaje != 2 || respuesta.Result is not string jsonStr || string.IsNullOrWhiteSpace(jsonStr))
                    return new Respuesta { IdTipoMensaje = respuesta.IdTipoMensaje, Mensaje = respuesta.Mensaje, Result = null };

                var estructura = JsonNode.Parse(jsonStr);
                if (estructura is null)
                    return new Respuesta { IdTipoMensaje = 1, Mensaje = "Error al procesar el documento.", Result = null };

                byte[]? logoBytes = null;
                var logoKey = estructura?["document"]?["header"]?["logo"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(logoKey))
                {
                    try
                    {
                        var logoUrl = _s3.GenerarDownloadUrl(logoKey);
                        using var http = new HttpClient();
                        logoBytes = await http.GetByteArrayAsync(logoUrl);
                    }
                    catch { }
                }

                var generador = new DocxGeneratorService();
                using var docxStream = generador.GenerarDocx(estructura!, logoBytes);

                var rutaS3 = $"informes/pedido-{request.IdPedido}/informe-{request.IdInforme}/documento.docx";
                await _s3.UploadStreamAsync(rutaS3, docxStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

                var downloadUrl = _s3.GenerarDownloadUrl(rutaS3);

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Documento DOCX generado correctamente.",
                    Result = new { url = downloadUrl, ruta = rutaS3 }
                };
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = null };
            }
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
