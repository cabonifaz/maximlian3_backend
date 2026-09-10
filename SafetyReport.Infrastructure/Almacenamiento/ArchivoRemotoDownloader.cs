using SafetyReport.Application.Puertos.Almacenamiento;

namespace SafetyReport.Infrastructure.Almacenamiento;

public class ArchivoRemotoDownloader : IArchivoRemotoDownloader
{
    private readonly HttpClient _httpClient;

    public ArchivoRemotoDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<byte[]> DescargarBytesAsync(string url) =>
        _httpClient.GetByteArrayAsync(url);
}
