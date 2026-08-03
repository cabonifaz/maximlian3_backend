using System.Net.Http.Json;
using System.Text.Json;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class FacturacionElectronicaService
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public FacturacionElectronicaService(FacturacionElectronicaConfig config, HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(config.BaseUrl);
            httpClient.DefaultRequestHeaders.Add("X-Api-Key", config.ApiKey);
            _httpClient = httpClient;
        }

        public async Task<FacturacionEnvelope<FacturacionDocumentoCreado>?> InsertarDocumentoAsync(
            FacturacionInsertarDocumentoRequest request, CancellationToken cancellationToken)
        {
            var respuesta = await _httpClient.PostAsJsonAsync("api/v1/documentos-electronicos", request, JsonOptions, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<FacturacionDocumentoCreado>>(JsonOptions, cancellationToken);
        }

        public async Task<FacturacionEnvelope<FacturacionResultadoEnvioSunat>?> EnviarASunatAsync(
            int idInquilino, int idDocumentoElectronico, string ambienteCodigo, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}/enviar-sunat?idInquilino={idInquilino}&ambienteCodigo={ambienteCodigo}";
            var respuesta = await _httpClient.PostAsync(url, null, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<FacturacionResultadoEnvioSunat>>(JsonOptions, cancellationToken);
        }

        public async Task<FacturacionEnvelope<object>?> ObtenerDocumentoAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}?idInquilino={idInquilino}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<object>>(JsonOptions, cancellationToken);
        }

        public async Task<FacturacionEnvelope<object>?> GuardarCambiosAsync(
            int idInquilino, int idDocumentoElectronico, FacturacionGuardarCambiosRequest request, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}/guardar-cambios?idInquilino={idInquilino}";
            var respuesta = await _httpClient.PutAsJsonAsync(url, request, JsonOptions, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<object>>(JsonOptions, cancellationToken);
        }
    }
}
