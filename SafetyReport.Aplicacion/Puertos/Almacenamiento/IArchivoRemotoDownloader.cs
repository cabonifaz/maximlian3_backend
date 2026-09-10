namespace SafetyReport.Application.Puertos.Almacenamiento;

public interface IArchivoRemotoDownloader
{
    Task<byte[]> DescargarBytesAsync(string url);
}
