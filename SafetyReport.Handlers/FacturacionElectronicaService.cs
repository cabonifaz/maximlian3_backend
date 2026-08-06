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
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}/enviar-sunat?idInquilino={idInquilino}";
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

        // El token nunca viene en ObtenerDocumentoAsync a propósito (ms-facturación no lo expone vía
        // Obtener) — este es el único camino autenticado para conseguirlo, y solo sirve para armar el link
        // de verificación pública (ver PedidoFacturaHandler.ObtenerUrlVerificacionAsync).
        public async Task<FacturacionEnvelope<string>?> ObtenerTokenVerificacionAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}/token-verificacion?idInquilino={idInquilino}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<string>>(JsonOptions, cancellationToken);
        }

        // Verificación pública (sin idInquilino, sin login): ms-facturación ya devuelve una proyección
        // público-segura (SP_DocumentoElectronico_ObtenerPorToken), sin ningún Id* interno.
        public async Task<FacturacionEnvelope<object>?> ObtenerDocumentoPorTokenAsync(
            string token, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/token/{Uri.EscapeDataString(token)}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<object>>(JsonOptions, cancellationToken);
        }

        // tipoArchivo: "Xml" o "Pdf". Mismo criterio que ObtenerDocumentoPorTokenAsync.
        public async Task<FacturacionEnvelope<string>?> ObtenerUrlDescargaPorTokenAsync(
            string token, string tipoArchivo, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/token/{Uri.EscapeDataString(token)}/url-descarga?tipoArchivo={Uri.EscapeDataString(tipoArchivo)}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<string>>(JsonOptions, cancellationToken);
        }

        // tipoArchivo: "Xml" o "Pdf". Devuelve una URL presignada de S3 (vigencia 5 min) — el archivo
        // nunca pasa por ninguno de los dos backends, el cliente descarga directo de S3 con esa URL.
        public async Task<FacturacionEnvelope<string>?> ObtenerUrlDescargaAsync(
            int idInquilino, int idDocumentoElectronico, string tipoArchivo, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}/url-descarga?idInquilino={idInquilino}&tipoArchivo={Uri.EscapeDataString(tipoArchivo)}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<string>>(JsonOptions, cancellationToken);
        }

        // Solo los errores/observaciones del último intento de envío a SUNAT (no el historial completo de
        // reintentos anteriores) — ver SP_ErrorDocumento_ListarUltimoEnvio en ms-facturación.
        public async Task<FacturacionEnvelope<List<FacturacionErrorDocumento>>?> ObtenerErroresUltimoEnvioAsync(
            int idInquilino, int idDocumentoElectronico, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}/errores-ultimo-envio?idInquilino={idInquilino}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<List<FacturacionErrorDocumento>>>(JsonOptions, cancellationToken);
        }

        public async Task<FacturacionEnvelope<object>?> GuardarCambiosAsync(
            int idInquilino, int idDocumentoElectronico, FacturacionGuardarCambiosRequest request, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/{idDocumentoElectronico}/guardar-cambios?idInquilino={idInquilino}";
            var respuesta = await _httpClient.PutAsJsonAsync(url, request, JsonOptions, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<object>>(JsonOptions, cancellationToken);
        }

        // Usado por el worker de sincronización — sondea EVENTOS_DOCUMENTO desde el checkpoint de la empresa.
        public async Task<FacturacionEnvelope<List<FacturacionEventoDocumento>>?> ListarEventosRecientesAsync(
            int idInquilino, int ultimoIdEvento, CancellationToken cancellationToken)
        {
            var url = $"api/v1/documentos-electronicos/eventos-recientes?idInquilino={idInquilino}&ultimoIdEvento={ultimoIdEvento}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<List<FacturacionEventoDocumento>>>(JsonOptions, cancellationToken);
        }

        // Crea el lote de Comunicación de Baja y lo envía a SUNAT en el mismo paso (sendSummary). Devuelve
        // un ticket, no un veredicto — el resultado real llega después vía SincronizacionFacturacionWorker.
        public async Task<FacturacionEnvelope<FacturacionLoteDocumentoCreado>?> EnviarComunicacionBajaAsync(
            FacturacionComunicacionBajaRequest request, CancellationToken cancellationToken)
        {
            var respuesta = await _httpClient.PostAsJsonAsync("api/v1/lotes-documento/comunicacion-baja", request, JsonOptions, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<FacturacionLoteDocumentoCreado>>(JsonOptions, cancellationToken);
        }

        // Listado de facturas para la pantalla de PedidoFactura — NumeroFactura/ClienteNombre/FormaPago/
        // Estado ya vienen resueltos por ms-facturación (SP_DocumentoElectronico_ListarParaPedidoFactura).
        public async Task<FacturacionEnvelope<FacturacionResultadoPaginado<FacturacionFacturaResumen>>?> ListarFacturasAsync(
            int idInquilino, int idEmpresa, string? estadoCodigo, int? idFormaPago, DateOnly? fechaDesde, DateOnly? fechaHasta,
            string? busqueda, int pagina, int tamanoPagina, CancellationToken cancellationToken)
        {
            var query = new List<string> { $"idInquilino={idInquilino}", $"idEmpresa={idEmpresa}", $"pagina={pagina}", $"tamanoPagina={tamanoPagina}" };
            if (!string.IsNullOrWhiteSpace(estadoCodigo)) query.Add($"estadoCodigo={Uri.EscapeDataString(estadoCodigo)}");
            if (idFormaPago is not null) query.Add($"idFormaPago={idFormaPago}");
            if (fechaDesde is not null) query.Add($"fechaDesde={fechaDesde:yyyy-MM-dd}");
            if (fechaHasta is not null) query.Add($"fechaHasta={fechaHasta:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(busqueda)) query.Add($"busqueda={Uri.EscapeDataString(busqueda)}");

            var url = $"api/v1/documentos-electronicos/para-pedido-factura?{string.Join('&', query)}";
            var respuesta = await _httpClient.GetAsync(url, cancellationToken);
            return await respuesta.Content.ReadFromJsonAsync<FacturacionEnvelope<FacturacionResultadoPaginado<FacturacionFacturaResumen>>>(JsonOptions, cancellationToken);
        }
    }
}
