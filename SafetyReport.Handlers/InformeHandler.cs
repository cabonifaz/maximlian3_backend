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

        public async Task<Respuesta> InsertarAsync(UsuarioGeneral usuarioLogueado, InformeCrear request)
        {
            try
            {
                return await _dao.InsertarAsync(usuarioLogueado, request);
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
                return await _dao.ActualizarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<InformeCreado>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroInformeObtener request)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, request.IdInforme);
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
                    IdTipoMensaje = 1,
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
                    IdTipoMensaje = 1,
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
                    return new Respuesta { IdTipoMensaje = 2, Mensaje = "El archivo es requerido.", Result = null };

                JsonNode? seccionesJson;
                try { seccionesJson = JsonNode.Parse(secciones); }
                catch { return new Respuesta { IdTipoMensaje = 2, Mensaje = "El campo Secciones no es un JSON válido.", Result = null }; }

                var extension = Path.GetExtension(archivo.FileName);
                var fileKey = $"autocompletado/{Guid.NewGuid()}{extension}";
                await _s3.UploadFileAsync(fileKey, archivo);

                var payload = new { fileKey, mimeType = archivo.ContentType, secciones = seccionesJson, prompt = prompt ?? string.Empty };
                var n8nRespuesta = await _n8n.PostAsync(_n8nConfig.WebhookObtenerCampos, payload);

                return new Respuesta
                {
                    IdTipoMensaje = 1,
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
